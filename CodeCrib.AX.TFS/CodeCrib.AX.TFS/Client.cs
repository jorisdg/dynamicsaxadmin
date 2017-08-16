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

namespace CodeCrib.AX.TFS
{
    class AutoRunLogOutput
    {
        protected static void OutputLog(CodeActivityContext context, List<KeyValuePair<string, string>> log, bool outputAllAsInfo = false)
        {
            if (log != null)
            {
                foreach (var message in log)
                {
                    if (outputAllAsInfo)
                    {
                        context.TrackBuildMessage(message.Value);
                    }
                    else
                    {
                        switch (message.Key)
                        {
                            case "Info":
                                context.TrackBuildMessage(message.Value);
                                break;
                            case "Warning":
                                context.TrackBuildWarning(message.Value);
                                break;
                            case "Error":
                                context.TrackBuildError(message.Value);
                                break;
                        }
                    }
                }
            }
        }

        public static void Output(CodeActivityContext context, Client.AutoRun.AxaptaAutoRun autoRun, bool outputAllAsInfo = false, bool skipInfoLog = false)
        {
            autoRun.ParseLog();

            //OutputLog(context, autoRun.Log, outputAllAsInfo);

            foreach (var step in autoRun.Steps)
            {
                OutputLog(context, step.Log, outputAllAsInfo);
            }

            if (!skipInfoLog && !string.IsNullOrEmpty(autoRun.InfoLog))
            {
                var lines = autoRun.InfoLog.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string line in lines)
                {
                    context.TrackBuildMessage(line.Trim());
                }
            }
        }
    }

    class CommandContext
    {
        public Func<int, int, Exception> Delegate { get; set; }
        public Process Process { get; set; }
        public Client.AutoRun.AxaptaAutoRun AutoRun { get; set; }
        public string AutoRunFile { get; set; }
        public string LogFile { get; set; }

        public static Exception WaitForProcess(int processId, int timeOutMinutes)
        {
            Exception returnException = null;

            if (processId != 0)
            {
                Process process = Process.GetProcessById(processId);

                if (timeOutMinutes > 0)
                {
                    if (!process.WaitForExit((int)new TimeSpan(0, timeOutMinutes, 0).TotalMilliseconds))
                    {
                        // Process is still running after the timeout has elapsed.

                        try
                        {
                            process.Kill();
                            returnException = new TimeoutException(string.Format("Client time out of {0} minutes exceeded", timeOutMinutes));
                        }
                        catch (Exception ex)
                        {
                            // Error trying to kill the process
                            returnException = new TimeoutException(string.Format("Client time out of {0} minutes exceeded, additionally an exception was encountered while trying to kill the client process (see innerexception)", timeOutMinutes), ex);
                        }
                    }
                }
                else
                {
                    process.WaitForExit();
                }
            }

            return returnException;
        }
    }

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

            Helper.ExtractClientLayerModelInfo(configurationFile, layerCodes, modelManifest, out modelName, out publisher, out layer, out layerCode);

            var clientConfig = Helper.GetClientConfig(configurationFile);

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

                AutoRunLogOutput.Output(context, userState.AutoRun, true);

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

            var clientConfig = Helper.GetClientConfig(configurationFile);

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

                AutoRunLogOutput.Output(context, userState.AutoRun);

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

        public static bool IsEmptyLabelFile(string labelFile)
        {
            bool isEmptyFile = true;

            if (File.Exists(labelFile))
            {
                using (StreamReader streamReader = new StreamReader(File.OpenRead(labelFile)))
                {
                    int lineCounter = 0;
                    while (isEmptyFile && !streamReader.EndOfStream && lineCounter < 50)
                    {
                        string line = streamReader.ReadLine().Trim();

                        Match match = Regex.Match(line, @"@.{3}\d+\s.+");
                        if (match.Success)
                        {
                            isEmptyFile = false;
                        }

                        lineCounter++;
                    }
                }
            }

            return isEmptyFile;
        }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int timeoutMinutes = TimeOutMinutes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);
            StringList layerCodes = LayerCodes.Get(context);
            string labelFilesFolder = LabelFilesFolder.Get(context);
            
            if (!Directory.Exists(labelFilesFolder))
            {
                context.TrackBuildWarning(string.Format("Label file folder {0} not found.", labelFilesFolder));

                // TODO Is there a better way with this? can we just return null or something?
                Func<int, int, Exception> bogusDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
                context.UserState = new CommandContext { Delegate = bogusDelegate, Process = null, AutoRun = null, AutoRunFile = null };
                return bogusDelegate.BeginInvoke(0, 0, callback, state);
            }

            string modelName;
            string publisher;
            string layer;
            string layerCode;
            string modelManifest = ModelManifestFile.Get(context);

            Helper.ExtractClientLayerModelInfo(configurationFile, layerCodes, modelManifest, out modelName, out publisher, out layer, out layerCode);

            var clientConfig = Helper.GetClientConfig(configurationFile);
            Client.AutoRun.AxaptaAutoRun autoRun = new Client.AutoRun.AxaptaAutoRun() { ExitWhenDone = true, LogFile = string.Format(@"{0}\LabelFlushLog-{1}.xml", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), Guid.NewGuid()) };
            Client.Commands.ImportLabelFile importCommand = new Client.Commands.ImportLabelFile() { ConfigurationFile = configurationFile, Layer = layer, LayerCode = layerCode, Model = modelName, ModelPublisher = publisher };

            foreach (string filename in Directory.GetFiles(labelFilesFolder, "*.ald"))
            {
                if (!IsEmptyLabelFile(filename))
                {
                    context.TrackBuildMessage(string.Format("Importing label file {0} into model {1} ({2})", filename, modelName, publisher));
                    importCommand.Filename = filename;
                    Client.Client.ExecuteCommand(importCommand, timeoutMinutes);

                    string labelFile = Path.GetFileNameWithoutExtension(filename).Substring(2, 3);
                    string labelLanguage = Path.GetFileNameWithoutExtension(filename).Substring(5);

                    autoRun.Steps.Add(new Client.AutoRun.Run() { Type = Client.AutoRun.RunType.@class, Name = "Global", Method = "info", Parameters = string.Format("strFmt(\"Flush label {0} language {1}: %1\", Label::flush(\"{0}\",\"{1}\"))", labelFile, labelLanguage) });
                }
            }

            string autoRunFile = string.Format(@"{0}\AutoRun-LabelFlush-{1}.xml", Environment.GetEnvironmentVariable("temp"), Guid.NewGuid());
            Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(autoRun, autoRunFile);
            context.TrackBuildMessage(string.Format("Flushing imported label files"));
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

                AutoRunLogOutput.Output(context, userState.AutoRun, true);

                if (File.Exists(userState.LogFile))
                    File.Delete(userState.LogFile);
                File.Delete(userState.AutoRunFile);
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
            string modelManifest = ModelManifestFile.Get(context);

            Helper.ExtractClientLayerModelInfo(configurationFile, layerCodes, modelManifest, out modelName, out publisher, out layer, out layerCode);

            var clientConfig = Helper.GetClientConfig(configurationFile);


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

                AutoRunLogOutput.Output(context, userState.AutoRun, true);

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
