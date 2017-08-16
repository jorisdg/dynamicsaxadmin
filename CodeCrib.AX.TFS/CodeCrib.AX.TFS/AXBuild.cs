//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System;
using System.Activities;
using System.IO;
using System.Diagnostics;
using System.Management;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class AXBuildCompile : AsyncCodeActivity
    {
        public InArgument<int> TimeOutMinutes { get; set; }
        public InArgument<int> Workers { get; set; }
        public InArgument<string> ConfigurationFile { get; set; }
        public InArgument<string> AlternateBinaryFolder { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int timeOutMinutes = TimeOutMinutes.Get(context);

            AXBuild.Commands.Compile compile = new AXBuild.Commands.Compile()
            {
                Workers = Workers.Get(context),
                LogPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())).FullName
            };

            string configurationFile = ConfigurationFile.Get(context);
            string serverBinPath = Helper.GetServerConfig(configurationFile).AlternateBinDirectory;

            if (string.IsNullOrEmpty(serverBinPath))
            {
                throw new Exception("Could not determine server binaries path");
            }

            string altBin = AlternateBinaryFolder.Get(context);
            if (string.IsNullOrEmpty(altBin))
            {
                altBin = Helper.GetClientConfig(configurationFile).BinaryDirectory;
            }

            compile.Compiler = Path.Combine(serverBinPath, "Ax32Serv.exe");
            compile.AOSInstance = Helper.GetServerNumber(configurationFile).ToString("D2");
            compile.AltBinDir = altBin;

            context.TrackBuildMessage("Compiling application using AXBuild", BuildMessageImportance.Normal);
            Process process = AXBuild.AXBuild.StartCommand(serverBinPath, compile);

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, AutoRun = null, AutoRunFile = null, LogFile = Path.Combine(compile.LogPath, "AxCompileAll.html") };
            return processWaitDelegate.BeginInvoke(process.Id, timeOutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {
            CommandContext userState = context.UserState as CommandContext;

            if (userState != null && userState.Process != null)
            {
                // Kill the AxBuild process
                int processId = userState.Process.Id;
                userState.Process.Kill();

                // Find all Ax32Serv.exe sub-processes of the AxBuild
                using (var mos = new ManagementObjectSearcher(string.Format("SELECT ProcessId, Name FROM Win32_Process WHERE ParentProcessId = {0}", processId)))
                {
                    foreach (var obj in mos.Get())
                    {
                        string processName = (string)obj.Properties["Name"].Value;
                        if (processName == "Ax32Serv.exe")
                        {
                            UInt32 pid = (UInt32)obj.Properties["ProcessId"].Value;
                            if (pid != 0)
                            {
                                Process subProcess = Process.GetProcessById(Convert.ToInt32(pid));
                                if (!subProcess.HasExited)
                                    subProcess.Kill();
                            }
                        }
                    }
                }

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
}
