using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class AxBuildTask : CancelableBuildTask
    {
        protected int Workers;
        protected string AlternateBinaryFolder;
        protected string LogFile;

        public AxBuildTask(IBuildLogger buildLogger, string configurationFile, int workers, string alternateBinaryFolder) : this(buildLogger, 0, configurationFile, workers, alternateBinaryFolder)
        {
        }

        public AxBuildTask(IBuildLogger buildLogger, int timeoutMinutes, string configurationFile, int workers, string alternateBinaryFolder) : base(buildLogger, timeoutMinutes, configurationFile)
        {
            Workers = workers;
            AlternateBinaryFolder = alternateBinaryFolder;
        }

        public AxBuildTask()
        {
        }

        public override Process Start()
        {
            AXBuild.Commands.Compile compile = new AXBuild.Commands.Compile()
            {
                Workers = Workers,
                LogPath = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())).FullName
            };

            LogFile = Path.Combine(compile.LogPath, "AxCompileAll.html");

            string serverBinPath = CodeCrib.AX.Deploy.Configs.GetServerConfig(ConfigurationFile).AlternateBinDirectory;

            if (string.IsNullOrEmpty(serverBinPath))
            {
                throw new Exception("Could not determine server binaries path");
            }

            if (string.IsNullOrEmpty(AlternateBinaryFolder))
            {
                AlternateBinaryFolder = Deploy.Configs.GetClientConfig(ConfigurationFile).BinaryDirectory;
            }

            compile.Compiler = Path.Combine(serverBinPath, "Ax32Serv.exe");
            compile.AOSInstance = Deploy.Configs.GetServerNumber(ConfigurationFile).ToString("D2");
            compile.AltBinDir = AlternateBinaryFolder;

            BuildLogger.LogInformation("Compiling application using AXBuild");

            Process process = AXBuild.AXBuild.CreateCommand(serverBinPath, compile);

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;

            process.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) { BuildLogger.LogInformation(e.Data); };
            process.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) { BuildLogger.LogError(e.Data); };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        public override void End()
        {
            BuildLogger.StoreLogFile(LogFile);

            Client.CompileOutput output = null;

            try
            {
                output = Client.CompileOutput.CreateFromFile(LogFile);
            }
            catch (FileNotFoundException ex)
            {
                throw new Exception("Compile log could not be found", ex);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error parsing compile log: {0}", ex.Message), ex);
            }

            CompileMessageReporter.ReportCompileMessages(BuildLogger, output);
        }

        public override void Run()
        {
            Process process = Start();

            Exception executionException = CommandContext.WaitForProcess(process.Id, TimeoutMinutes);
            if (executionException != null)
            {
                throw executionException;
            }

            End();
            Cleanup(process);
        }

        public override void Cleanup(Process process)
        {
            if (process != null && !process.HasExited)
            {
                // Kill the AxBuild process
                int processId = process.Id;
                process.Kill();

                // Find all Ax32Serv.exe sub-processes of the AxBuild
                using (var mos = new ManagementObjectSearcher(string.Format("SELECT ProcessId, Name FROM Win32_Process WHERE ParentProcessId = {0}", processId)))
                {
                    foreach (var obj in mos.Get())
                    {
                        string processName = (string)obj.Properties["Name"].Value;
                        if (processName.Equals("Ax32Serv.exe", StringComparison.CurrentCultureIgnoreCase))
                        {
                            UInt32 pid = (UInt32)obj.Properties["ProcessId"].Value;
                            if (pid != 0)
                            {
                                Process subProcess = Process.GetProcessById(Convert.ToInt32(pid));
                                if (!subProcess.HasExited)
                                {
                                    subProcess.Kill();
                                }
                            }
                        }
                    }
                }
            }

            try
            {
                // TODO:  Deferred deletion by storing the log file elsewhere in the build
                //File.Delete(logFile);
            }
            catch (FileNotFoundException)
            {
            }
        }
    }
}
