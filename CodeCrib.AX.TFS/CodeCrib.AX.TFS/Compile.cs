//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System.IO;
using System.Diagnostics;
using CodeCrib.AX.BuildTasks;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class Compile : AsyncCodeActivity
    {
        public InArgument<string> ClientExecutablePath { get; set; }

        public InArgument<bool> UpdateCrossReference { get; set; }
        public InArgument<int> TimeOutMinutes { get; set; }

        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<StringList> LayerCodes { get; set; }
        public InArgument<string> ModelManifestFile { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            bool updateXRef = UpdateCrossReference.Get(context);
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string clientExePath = ClientExecutablePath.Get(context);

            string configurationFile = ConfigurationFile.Get(context);

            context.TrackBuildMessage(string.Format("Compiling application {0} cross-reference update.", updateXRef ? "with" : "without"));

            Client.Commands.Compile compile = new Client.Commands.Compile()
            {
                Minimize = true,
                LazyClassLoading = true,
                LazyTableLoading = true,
                UpdateCrossReference = updateXRef
            };

            if (!string.IsNullOrEmpty(configurationFile))
            {
                compile.ConfigurationFile = configurationFile;
            }

            StringList layerCodes = LayerCodes.Get(context);
            if (layerCodes != null)
            {
                string modelManifest = ModelManifestFile.Get(context);
                if (!string.IsNullOrEmpty(modelManifest))
                {
                    string model;
                    string publisher;
                    string layer;
                    string layerCode;

                    CodeCrib.AX.Deploy.Configs.ExtractClientLayerModelInfo(configurationFile, layerCodes, modelManifest, out model, out publisher, out layer, out layerCode);

                    compile.Model = model;
                    compile.ModelPublisher = publisher;
                    compile.Layer = layer;
                    compile.LayerCode = layerCode;
                }
            }

            Process process = null;
            if (string.IsNullOrEmpty(clientExePath))
                process = Client.Client.StartCommand(compile);
            else
                process = Client.Client.StartCommand(clientExePath, compile);

            var clientConfig = CodeCrib.AX.Deploy.Configs.GetClientConfig(configurationFile);

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, AutoRun = null, AutoRunFile = null, LogFile = string.Format(@"{0}\{1}", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), "AxCompileAll.html") };
            return processWaitDelegate.BeginInvoke(process.Id, timeOutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.Process != null)
            {
                userState.Process.Kill();

                if (File.Exists(userState.LogFile))
                    File.Delete(userState.LogFile);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && !string.IsNullOrEmpty(userState.LogFile))
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                Client.CompileOutput output = null;

                try
                {
                    output = Client.CompileOutput.CreateFromFile(userState.LogFile);
                }
                catch (FileNotFoundException)
                {
                    throw new Exception("Compile log could not be found");
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error parsing compile log: {0}", ex.Message));
                }

                Helper.ReportCompileMessages(context, output);
            }
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class CompileNodes : CodeActivity
    {
        public InArgument<string> ClientExecutablePath { get; set; }

        public InArgument<bool> UpdateCrossReference { get; set; }
        public InArgument<int> TimeOutMinutes { get; set; }

        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<StringList> LayerCodes { get; set; }
        public InArgument<string> ModelManifestFile { get; set; }

        [RequiredArgument]
        public InArgument<StringList> AOTCompilePaths { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            bool updateXRef = UpdateCrossReference.Get(context);
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string clientExePath = ClientExecutablePath.Get(context);

            StringList aotCompilePaths = AOTCompilePaths.Get(context);
            if (aotCompilePaths == null ||
                aotCompilePaths.Count == 0)
            {
                // Nothing to do.
                return;
            }

            string configurationFile = ConfigurationFile.Get(context);

            Client.Commands.AutoRun command = new Client.Commands.AutoRun
            {
                LazyClassLoading = true,
                LazyTableLoading = true,
                Minimize = true
            };

            if (!string.IsNullOrEmpty(configurationFile))
            {
                command.ConfigurationFile = configurationFile;
            }

            StringList layerCodes = LayerCodes.Get(context);
            if (layerCodes != null)
            {
                string modelManifest = ModelManifestFile.Get(context);
                if (!string.IsNullOrEmpty(modelManifest))
                {
                    string model;
                    string publisher;
                    string layer;
                    string layerCode;

                    CodeCrib.AX.Deploy.Configs.ExtractClientLayerModelInfo(configurationFile, layerCodes, modelManifest, out model, out publisher, out layer, out layerCode);

                    command.Model = model;
                    command.ModelPublisher = publisher;
                    command.Layer = layer;
                    command.LayerCode = layerCode;
                }
            }

            Client.AutoRun.AxaptaAutoRun axaptaAutoRun = new Client.AutoRun.AxaptaAutoRun()
            {
                ExitWhenDone = true
            };

            foreach (string path in aotCompilePaths)
            {
                axaptaAutoRun.Steps.Add(new Client.AutoRun.CompileApplication()
                {
                    UpdateCrossReference = false,
                    Node = path
                });
            }

            string autorunFilename = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(axaptaAutoRun, autorunFilename);
            command.Filename = autorunFilename;

            context.TrackBuildMessage("Compiling individual AOT nodes");

            if (string.IsNullOrEmpty(clientExePath))
            {
                Client.Client.ExecuteCommand(command, timeOutMinutes);
            }
            else
            {
                Client.Client.ExecuteCommand(clientExePath, command, timeOutMinutes);
            }

            // Compile log is not parsed at this point.  We expect to
            // run a full compile at a later step to get the compile results.
        }
    }
}
