using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeCrib.AX.Client.AutoRun;
using System.IO;

namespace CodeCrib.AX.BuildTasks
{
    public class ClientImportXpoTask : ClientBuildTask
    {
        protected string XpoFilePath;

        public ClientImportXpoTask(
            IBuildLogger buildLogger,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string xpoFilePath) : base(buildLogger, 0, configurationFile, layerCodes, modelManifest)
        {
            XpoFilePath = xpoFilePath;
        }

        public ClientImportXpoTask(
            IBuildLogger buildLogger,
            int timeoutMinutes,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string xpoFilePath) : base(buildLogger, timeoutMinutes, configurationFile, layerCodes, modelManifest)
        {
            XpoFilePath = xpoFilePath;
        }

        public ClientImportXpoTask()
        {
        }

        public override void End(IBuildLogger buildLogger, AxaptaAutoRun autoRun)
        {
            AutoRunLogOutput.Output(buildLogger, autoRun, true);
        }

        public override void Run()
        {
            Process process = Start();

            Exception executionException = CommandContext.WaitForProcess(process.Id, TimeoutMinutes);
            if (executionException != null)
            {
                throw executionException;
            }

            End(BuildLogger, AutoRun);
            Cleanup(null, AutoRun.LogFile, AutoRunFile);
        }

        public override Process Start()
        {
            Deploy.Configs.ExtractClientLayerModelInfo(ConfigurationFile, LayerCodes, ModelManifest, out ModelName, out Publisher, out Layer, out LayerCode);

            Config.Client clientConfig = Deploy.Configs.GetClientConfig(ConfigurationFile);

            AutoRun = new AxaptaAutoRun() { ExitWhenDone = true, LogFile = string.Format(@"{0}\ImportLog-{1}.xml", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), Guid.NewGuid()) };
            //Client.Commands.ImportXPO importCommand = new Client.Commands.ImportXPO() { ConfigurationFile = ConfigurationFile, Layer = Layer, LayerCode = LayerCode, Model = ModelName, ModelPublisher = Publisher };

            AutoRunFile = Path.Combine(Environment.GetEnvironmentVariable("temp"), string.Format("AutoRun-ImportXPO-{0}", Guid.NewGuid()));
            AxaptaAutoRun.SerializeAutoRun(AutoRun, AutoRunFile);

            BuildLogger.LogInformation(string.Format("Importing XPO {0} into model {1}", XpoFilePath, ModelName));

            Process process = Client.Client.StartCommand(new Client.Commands.AutoRun() { ConfigurationFile = ConfigurationFile, Layer = Layer, Model = ModelName, ModelPublisher = Publisher, Filename = AutoRunFile });

            return process;
        }
    }
}
