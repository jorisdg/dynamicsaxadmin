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
            string configurationFile = ConfigurationFile.Get(context);
            int timeoutMinutes = TimeOutMinutes.Get(context);
            string xpoFile = XPOFile.Get(context);
            StringList layerCodes = LayerCodes.Get(context);
            string modelManifest = ModelManifestFile.Get(context);
            string modelName;
            string publisher;
            string layer;
            string layerCode;

            CodeCrib.AX.Deploy.Configs.ExtractClientLayerModelInfo(configurationFile, layerCodes, modelManifest, out modelName, out publisher, out layer, out layerCode);

            var clientConfig = CodeCrib.AX.Deploy.Configs.GetClientConfig(configurationFile);

            Client.AutoRun.AxaptaAutoRun autoRun = new Client.AutoRun.AxaptaAutoRun() { ExitWhenDone = true, LogFile = string.Format(@"{0}\ImportLog-{1}.xml", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), Guid.NewGuid()) };
            autoRun.Steps.Add(new Client.AutoRun.XpoImport() { File = xpoFile });

            string autoRunFile = string.Format(@"{0}\AutoRun-ImportXPO-{1}.xml", Environment.GetEnvironmentVariable("temp"), Guid.NewGuid());
            Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(autoRun, autoRunFile);

            context.TrackBuildMessage(string.Format("Importing XPO {0} into model {1}", xpoFile, modelName));
            Process process = Client.Client.StartCommand(new Client.Commands.AutoRun() { ConfigurationFile = configurationFile, Layer = layer, LayerCode = layerCode, Model = modelName, ModelPublisher = publisher, Filename = autoRunFile });

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, AutoRun = autoRun, AutoRunFile = autoRunFile, LogFile = autoRun.LogFile };
            return processWaitDelegate.BeginInvoke(process.Id, timeoutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.Process != null)
            {
                userState.Process.Kill();

                if (File.Exists(userState.LogFile))
                    File.Delete(userState.LogFile);
                File.Delete(userState.AutoRunFile);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.AutoRun != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                AutoRunLogOutput.Output(context.DefaultLogger(), userState.AutoRun, true);

                if (File.Exists(userState.LogFile))
                    File.Delete(userState.LogFile);
                File.Delete(userState.AutoRunFile);
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

            var clientConfig = CodeCrib.AX.Deploy.Configs.GetClientConfig(configurationFile);

            Client.AutoRun.AxaptaAutoRun autoRun = new Client.AutoRun.AxaptaAutoRun() { ExitWhenDone = true, LogFile = string.Format(@"{0}\SynchronizeLog-{1}.xml", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), Guid.NewGuid()) };
            autoRun.Steps.Add(new Client.AutoRun.Synchronize() { SyncDB = true, SyncRoles = true });

            string autoRunFile = string.Format(@"{0}\AutoRun-Synchronize-{1}.xml", Environment.GetEnvironmentVariable("temp"), Guid.NewGuid());
            Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(autoRun, autoRunFile);

            Process process = Client.Client.StartCommand(new Client.Commands.AutoRun() { ConfigurationFile = configurationFile, Filename = autoRunFile });

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, AutoRun = autoRun, AutoRunFile = autoRunFile, LogFile = autoRun.LogFile };
            return processWaitDelegate.BeginInvoke(process.Id, timeoutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.Process != null)
            {
                userState.Process.Kill();

                if (File.Exists(userState.LogFile))
                    File.Delete(userState.LogFile);
                File.Delete(userState.AutoRunFile);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.AutoRun != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                AutoRunLogOutput.Output(context.DefaultLogger(), userState.AutoRun);

                if (File.Exists(userState.LogFile))
                    File.Delete(userState.LogFile);
                File.Delete(userState.AutoRunFile);
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
                context.UserState = new CommandContext { Delegate = bogusDelegate, Process = null, AutoRun = null, AutoRunFile = null };
                return bogusDelegate.BeginInvoke(0, 0, callback, state);
            }

            Process process = importer.Start();

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, AutoRun = importer.AutoRun, AutoRunFile = importer.AutoRunFile, LogFile = importer.AutoRun.LogFile };

            return processWaitDelegate.BeginInvoke(process.Id, timeoutMinutes, callback, state);
        }



        protected override void Cancel(AsyncCodeActivityContext context)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null)
            {
                new ClientImportLabelsTask().Cleanup(userState.Process, userState.LogFile, userState.AutoRunFile);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }

        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.AutoRun != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                ClientImportLabelsTask importer = new ClientImportLabelsTask();
                importer.End(context.DefaultLogger(), userState.AutoRun);
                importer.Cleanup(null, userState.LogFile, userState.AutoRunFile);
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

            if (!Directory.Exists(vsProjectsFolder))
            {
                context.TrackBuildMessage(string.Format("VS Projects folder {0} not found.", vsProjectsFolder));

                // TODO Is there a better way with this? can we just return null or something?
                Func<int, int, Exception> bogusDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
                context.UserState = new CommandContext { Delegate = bogusDelegate, Process = null, AutoRun = null, AutoRunFile = null };
                return bogusDelegate.BeginInvoke(0, 0, callback, state);
            }

            string modelName;
            string publisher;
            string layer;
            string layerCode;

            CodeCrib.AX.Deploy.Configs.ExtractClientLayerModelInfo(configurationFile, layerCodes, modelManifest, out modelName, out publisher, out layer, out layerCode);

            var clientConfig = CodeCrib.AX.Deploy.Configs.GetClientConfig(configurationFile);


            Client.AutoRun.AxaptaAutoRun autoRun = new Client.AutoRun.AxaptaAutoRun() { ExitWhenDone = true, LogFile = string.Format(@"{0}\VSImportLog-{1}.xml", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), Guid.NewGuid()) };

            var filesToProcess = from filter in new[] { "*.csproj", "*.dynamicsproj", "*.vbproj" }
                                 select Directory.GetFiles(vsProjectsFolder, filter, SearchOption.AllDirectories);

            foreach (string filename in filesToProcess.SelectMany(f => f))
            {
                autoRun.Steps.Add(new Client.AutoRun.Run() { Type = Client.AutoRun.RunType.@class, Name = "SysTreeNodeVSProject", Method = "importProject", Parameters = string.Format("@'{0}'", filename) });
            }

            string autoRunFile = string.Format(@"{0}\AutoRun-VSImport-{1}.xml", Environment.GetEnvironmentVariable("temp"), Guid.NewGuid());
            Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(autoRun, autoRunFile);

            context.TrackBuildMessage(string.Format("Importing VS Projects from folder {0} into model {1}", vsProjectsFolder, modelName));
            Process process = Client.Client.StartCommand(new Client.Commands.AutoRun() { ConfigurationFile = configurationFile, Layer = layer, LayerCode = layerCode, Model = modelName, ModelPublisher = publisher, Filename = autoRunFile });

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, AutoRun = autoRun, AutoRunFile = autoRunFile, LogFile = autoRun.LogFile };
            return processWaitDelegate.BeginInvoke(process.Id, timeoutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.Process != null)
            {
                userState.Process.Kill();

                if (File.Exists(userState.LogFile))
                    File.Delete(userState.LogFile);
                File.Delete(userState.AutoRunFile);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }

        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.AutoRun != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                AutoRunLogOutput.Output(context.DefaultLogger(), userState.AutoRun, true);

                if (File.Exists(userState.LogFile))
                    File.Delete(userState.LogFile);
                File.Delete(userState.AutoRunFile);
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
