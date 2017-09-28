//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using CodeCrib.AX.BuildTasks;

namespace CodeCrib.AX.TFS
{
    



    [BuildActivity(HostEnvironmentOption.Agent)]
    public class ImportXPO : AsyncCodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<int> TimeOutMinutes { get; set; }

        [RequiredArgument]
        public InArgument<string> XPOFile { get; set; }
        [RequiredArgument]
        public InArgument<StringList> LayerCodes { get; set; }
        [RequiredArgument]
        public InArgument<string> ModelManifestFile { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int timeoutMinutes = TimeOutMinutes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);
            StringList layerCodes = LayerCodes.Get(context);
            string modelManifest = ModelManifestFile.Get(context);
            string xpoFile = XPOFile.Get(context);

            ClientImportXpoTask importer = new ClientImportXpoTask(context.DefaultLogger(), configurationFile, layerCodes, modelManifest, xpoFile);

            Process process = importer.Start();
            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);

            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, CancelableBuildTask = importer };

            return processWaitDelegate.BeginInvoke(process.Id, timeoutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            if (context.UserState is CommandContext userState &&
                userState.CancelableBuildTask != null)
            {
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            if (context.UserState is CommandContext userState)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                if (userState.CancelableBuildTask != null)
                {
                    userState.CancelableBuildTask.End();
                    userState.CancelableBuildTask.Cleanup(userState.Process);
                }
            }
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class Synchronize : AsyncCodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<int> TimeOutMinutes { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int timeoutMinutes = TimeOutMinutes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);

            ClientSynchronizeTask task = new ClientSynchronizeTask(context.DefaultLogger(), timeoutMinutes, configurationFile);
            Process process = task.Start();

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, CancelableBuildTask = task };
            return processWaitDelegate.BeginInvoke(process.Id, timeoutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            if (context.UserState is CommandContext userState && 
                userState.CancelableBuildTask != null)
            {
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                if (userState.CancelableBuildTask != null)
                {
                    userState.CancelableBuildTask.End();
                    userState.CancelableBuildTask.Cleanup(userState.Process);
                }
            }
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class ImportLabels : AsyncCodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        [RequiredArgument]
        public InArgument<string> LabelFilesFolder { get; set; }
        [RequiredArgument]
        public InArgument<StringList> LayerCodes { get; set; }
        [RequiredArgument]
        public InArgument<string> ModelManifestFile { get; set; }

        public InArgument<int> TimeOutMinutes { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int timeoutMinutes = TimeOutMinutes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);
            StringList layerCodes = LayerCodes.Get(context);
            string labelFilesFolder = LabelFilesFolder.Get(context);
            string modelManifest = ModelManifestFile.Get(context);

            ClientImportLabelsTask importer = new ClientImportLabelsTask(context.DefaultLogger(), timeoutMinutes, configurationFile, layerCodes, modelManifest, labelFilesFolder);

            if (!importer.CheckLabelDirectoryExists())
            {
                // TODO Is there a better way with this? can we just return null or something?
                Func<int, int, Exception> bogusDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
                context.UserState = new CommandContext { Delegate = bogusDelegate, Process = null, CancelableBuildTask = null };
                return bogusDelegate.BeginInvoke(0, 0, callback, state);
            }

            Process process = importer.Start();

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            //context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, AutoRun = importer.AutoRun, AutoRunFile = importer.AutoRunFile, LogFile = importer.AutoRun.LogFile };
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, CancelableBuildTask = importer };

            return processWaitDelegate.BeginInvoke(process.Id, timeoutMinutes, callback, state);
        }



        protected override void Cancel(AsyncCodeActivityContext context)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.CancelableBuildTask != null)
            {
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }

        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            if (context.UserState is CommandContext userState)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                if (userState.CancelableBuildTask != null)
                {
                    userState.CancelableBuildTask.End();
                    userState.CancelableBuildTask.Cleanup(userState.Process);
                }
            }
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class ImportVSProject : AsyncCodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<int> TimeOutMinutes { get; set; }

        [RequiredArgument]
        public InArgument<string> VSProjectsFolder { get; set; }
        [RequiredArgument]
        public InArgument<StringList> LayerCodes { get; set; }
        [RequiredArgument]
        public InArgument<string> ModelManifestFile { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            string configurationFile = ConfigurationFile.Get(context);
            int timeoutMinutes = TimeOutMinutes.Get(context);
            string vsProjectsFolder = VSProjectsFolder.Get(context);
            StringList layerCodes = LayerCodes.Get(context);
            string modelManifest = ModelManifestFile.Get(context);

            ClientImportVSProjectTask task = new ClientImportVSProjectTask(context.DefaultLogger(), configurationFile, layerCodes, modelManifest, vsProjectsFolder);

            if (!task.CheckVSProjectsDirectoryExists())
            {
                // TODO Is there a better way with this? can we just return null or something?
                Func<int, int, Exception> bogusDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
                context.UserState = new CommandContext { Delegate = bogusDelegate, Process = null, CancelableBuildTask = null };
                return bogusDelegate.BeginInvoke(0, 0, callback, state);
            }

            Process process = task.Start();

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, CancelableBuildTask = task };
            return processWaitDelegate.BeginInvoke(process.Id, timeoutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {

            if (context.UserState is CommandContext userState && 
                userState.CancelableBuildTask != null)
            {
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.CancelableBuildTask != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                userState.CancelableBuildTask.End();
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }
        }
    }
/*
    private void CreateVSProjectImportXML(string sourcesFolder, bool recursive, string xmlFilename, bool append, string logFile, CodeActivityContext context)
        {
            if (System.IO.Directory.Exists(sourcesFolder))
            {
                AutoRun.AxaptaAutoRun autoRun = null;
                if (append)
                {
                    autoRun = AutoRun.AxaptaAutoRun.FindOrCreate(xmlFilename);
                }
                else
                {
                    autoRun = new AutoRun.AxaptaAutoRun();
                }

                autoRun.ExitWhenDone = true;
                if (logFile != "")
                {
                    autoRun.LogFile = logFile;
                }
                autoRun.Version = "4.0";
                autoRun.Steps = new List<AutoRun.AutorunElement>();

                List<string> files = new List<string>();

                GetProjectFiles(sourcesFolder, recursive, files, "*.csproj");

                if (files.Count != 0)
                {
                    List<string>.Enumerator fileEnumerator = files.GetEnumerator();
                    // Add import steps
                    while (fileEnumerator.MoveNext())
                    {
                        autoRun.Steps.Add(new AutoRun.Run() { type = AutoRun.RunType.@class, name = "SysTreeNodeVSProject", method = "importProject", parameters = string.Format("@'{0}'", fileEnumerator.Current) });
                    }
                    fileEnumerator = files.GetEnumerator();
                    // Add compile steps
                    while (fileEnumerator.MoveNext())
                    {
                        autoRun.Steps.Add(new AutoRun.CompileApplication() { node = string.Format(@"\Visual Studio Projects\C Sharp Projects\{0}", System.IO.Path.GetFileNameWithoutExtension(fileEnumerator.Current)), crossReference = false });
                    }

                    AutoRun.AxaptaAutoRun.SerializeAutoRun(autoRun, xmlFilename);
                }
                else
                {
                    context.TrackBuildMessage("No Visual Studio Projects found.");
                    // do not create autorun.xml file if there's nothing in it
                }
            }
            else
            {
                context.TrackBuildMessage("No Visual Studio Projects found.");
            }
        }
*/
}
