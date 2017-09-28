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
    public class ClientCompileTask : ClientBuildTask
    {
        protected string ClientExecutablePath;
        protected bool UpdateCrossReference;
        protected string LogFile;

        public ClientCompileTask(
            IBuildLogger buildLogger,
            int timeoutMinutes,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string clientExecutablePath,
            bool updateCrossReference) : base(buildLogger, timeoutMinutes, configurationFile, layerCodes, modelManifest)
        {
            ClientExecutablePath = clientExecutablePath;
            UpdateCrossReference = updateCrossReference;
        }

        public ClientCompileTask(
            IBuildLogger buildLogger,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string clientExecutablePath,
            bool updateCrossReference) : this(buildLogger, 0, configurationFile, layerCodes, modelManifest, clientExecutablePath, updateCrossReference)
        {
        }

        public ClientCompileTask()
        {
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

        public override Process Start()
        {
            BuildLogger.LogInformation(string.Format("Compiling application {0} cross-reference update.", UpdateCrossReference ? "with" : "without"));

            Deploy.Configs.ExtractClientLayerModelInfo(ConfigurationFile, LayerCodes, ModelManifest, out ModelName, out Publisher, out Layer, out LayerCode);

            Client.Commands.Compile compile = new Client.Commands.Compile
            {
                Minimize = true,
                LazyClassLoading = true,
                LazyTableLoading = true,
                UpdateCrossReference = UpdateCrossReference,
                Model = ModelName,
                ModelPublisher = Publisher,
                Layer = Layer,
                LayerCode = LayerCode,
            };

            if (!string.IsNullOrEmpty(ConfigurationFile))
            {
                compile.ConfigurationFile = ConfigurationFile;
            }

            Config.Client clientConfig = Deploy.Configs.GetClientConfig(ConfigurationFile);
            LogFile = Path.Combine(Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), "AxCompileAll.html");

            Process process = null;
            if (string.IsNullOrEmpty(ClientExecutablePath))
            {
                process = Client.Client.StartCommand(compile);
            }
            else
            {
                process = Client.Client.StartCommand(ClientExecutablePath, compile);
            }

            return process;
        }
    }
}
