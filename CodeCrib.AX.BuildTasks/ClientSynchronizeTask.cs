using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ClientSynchronizeTask : ClientBuildTask
    {
        protected string LogFile;

        public ClientSynchronizeTask(IBuildLogger buildLogger, string configurationFile) : base(buildLogger, 0, configurationFile)
        {
        }

        public ClientSynchronizeTask(IBuildLogger buildLogger, int timeoutMinutes, string configurationFile) : base(buildLogger, timeoutMinutes, configurationFile)
        {
        }

        public ClientSynchronizeTask()
        {
        }

        public override Process Start()
        {
            Config.Client clientConfig = Deploy.Configs.GetClientConfig(ConfigurationFile);

            LogFile = Path.Combine(Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), string.Format("SynchronizeLog-{0}.xml", Guid.NewGuid()));

            AutoRun = new Client.AutoRun.AxaptaAutoRun
            {
                ExitWhenDone = true,
                LogFile = LogFile,
            };
            AutoRun.Steps.Add(new Client.AutoRun.Synchronize { SyncDB = true, SyncRoles = true });

            AutoRunFile = Path.Combine(Environment.GetEnvironmentVariable("temp"), string.Format("AutoRun-Synchronize-{0}.xml", Guid.NewGuid()));
            Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(AutoRun, AutoRunFile);

            Process process = Client.Client.StartCommand(new Client.Commands.AutoRun { ConfigurationFile = ConfigurationFile, Filename = AutoRunFile });

            return process;
        }

        public override void End()
        {
            BuildLogger.StoreLogFile(LogFile);
            AutoRunLogOutput.Output(BuildLogger, AutoRun);
        }

    }
}
