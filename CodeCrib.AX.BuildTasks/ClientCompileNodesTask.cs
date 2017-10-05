using CodeCrib.AX.BuildRuntime;
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
    public class ClientCompileNodesTask : ClientBuildTask
    {
        protected IEnumerable<String> AOTCompilePaths;
        protected string ClientExecutablePath;

        public ClientCompileNodesTask(
            IBuildLogger buildLogger,
            int timeoutMinutes,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string clientExecutablePath,
            IEnumerable<string> aotCompilePaths) : base(buildLogger, timeoutMinutes, configurationFile, layerCodes, modelManifest)
        {
            ClientExecutablePath = clientExecutablePath;
            AOTCompilePaths = aotCompilePaths;
        }

        public ClientCompileNodesTask(
            IBuildLogger buildLogger,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string clientExecutablePath,
            IEnumerable<string> aotCompilePaths) : this(buildLogger, 0, configurationFile, layerCodes, modelManifest, clientExecutablePath, aotCompilePaths)
        {
        }

        public ClientCompileNodesTask(
            IBuildLogger buildLogger,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            IEnumerable<string> aotCompilePaths) : this(buildLogger, 0, configurationFile, layerCodes, modelManifest, string.Empty, aotCompilePaths)
        {
        }

        public ClientCompileNodesTask()
        {
        }

        public override void End()
        {
            BuildLogger.StoreLogFile(AutoRun.LogFile);
        }

        public override Process Start()
        {
            if (AOTCompilePaths == null ||
                !AOTCompilePaths.Any())
            {
                return null;
            }

            Deploy.Configs.ExtractClientLayerModelInfo(ConfigurationFile, LayerCodes, ModelManifest, out ModelName, out Publisher, out Layer, out LayerCode);

            Client.Commands.AutoRun command = new Client.Commands.AutoRun
            {
                Minimize = true,
                LazyClassLoading = true,
                LazyTableLoading = true,
                Model = ModelName,
                ModelPublisher = Publisher,
                Layer = Layer,
                LayerCode = LayerCode,
            };

            if (!string.IsNullOrEmpty(ConfigurationFile))
            {
                command.ConfigurationFile = ConfigurationFile;
            }

            AutoRun = new Client.AutoRun.AxaptaAutoRun
            {
                ExitWhenDone = true,
                LogFile = Path.Combine(BuildPaths.Temp, string.Format("AutoRunLog_{0}.xml", Guid.NewGuid())),
            };
            AutoRun.Steps.AddRange(AOTCompilePaths.Select(p => new Client.AutoRun.CompileApplication { UpdateCrossReference = false, Node = p }));

            AutoRunFile = Path.Combine(BuildPaths.Temp, string.Format("AutoRun_{0}.xml", Guid.NewGuid()));
            Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(AutoRun, AutoRunFile);
            command.Filename = AutoRunFile;

            BuildLogger.StoreLogFile(AutoRunFile);
            BuildLogger.LogInformation("Compiling individual AOT nodes");

            Process process = null;
            if (string.IsNullOrEmpty(ClientExecutablePath))
            {
                process = Client.Client.StartCommand(command);
            }
            else
            {
                process = Client.Client.StartCommand(ClientExecutablePath, command);
            }

            return process;
        }
    }
}
