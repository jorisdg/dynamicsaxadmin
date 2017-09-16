//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System.IO;
using System.Diagnostics;
using CodeCrib.AX.BuildTasks;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class GenerateCIL : AsyncCodeActivity
    {
        public InArgument<string> ClientExecutablePath { get; set; }

        public InArgument<int> TimeOutMinutes { get; set; }

        public InArgument<string> ConfigurationFile { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string clientExePath = ClientExecutablePath.Get(context);
            string configurationFile = ConfigurationFile.Get(context);

            context.TrackBuildMessage("Generating CIL.");

            Client.Commands.GenerateCIL compile = new Client.Commands.GenerateCIL()
            {
                Minimize = true,
                LazyClassLoading = true,
                LazyTableLoading = true
            };

            if (!string.IsNullOrEmpty(configurationFile))
            {
                compile.ConfigurationFile = configurationFile;
            }

            Process process = null;
            if (string.IsNullOrEmpty(clientExePath))
                process = Client.Client.StartCommand(compile);
            else
                process = Client.Client.StartCommand(clientExePath, compile);

            var alternateBinDirectory = CodeCrib.AX.Deploy.Configs.GetServerConfig(configurationFile).AlternateBinDirectory;

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, AutoRun = null, AutoRunFile = null, LogFile = string.Format(@"{0}\XppIL\Dynamics.Ax.Application.dll.log", Environment.ExpandEnvironmentVariables(alternateBinDirectory)) };
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

            if (userState != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                    throw processWaitException;

                Client.CILGenerationOutput output = null;
                try
                {
                    output = Client.CILGenerationOutput.CreateFromFile(userState.LogFile);
                }
                catch (FileNotFoundException)
                {
                    throw new Exception("CIL generation log could not be found");
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Error parsing CIL generation log: {0}", ex.Message));
                }

                bool hasErrors = false;
                foreach (var item in output.Output)
                {
                    string compileMessage;

                    if (item.LineNumber > 0)
                        compileMessage = string.Format("Object {0} method {1}, line {2} : {3}", item.ElementName, item.MethodName, item.LineNumber, item.Message);
                    else
                        compileMessage = string.Format("Object {0} method {1} : {2}", item.ElementName, item.MethodName, item.Message);

                    switch (item.Severity)
                    {
                        // Compile Errors
                        case 0:
                            context.TrackBuildError(compileMessage);
                            hasErrors = true;
                            break;
                        // Compile Warnings
                        case 1:
                            context.TrackBuildWarning(compileMessage);
                            break;
                        // "Other"
                        case 4:
                        default:
                            context.TrackBuildMessage(item.Message);
                            break;
                    }
                }
                if (hasErrors)
                    throw new Exception("CIL error(s) found");
            }
        }
    }
}
