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
    public class ClientImportVSProjectTask : ClientBuildTask
    {
        protected string VSProjectsFolder;
        protected string LogFile;
        
        public ClientImportVSProjectTask(
            IBuildLogger buildLogger,
            int timeoutMinutes,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string vsProjectsFolder) : base(buildLogger, timeoutMinutes, configurationFile, layerCodes, modelManifest)
        {
            VSProjectsFolder = vsProjectsFolder;
        }

        public ClientImportVSProjectTask(
            IBuildLogger buildLogger,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string vsProjectsFolder) : this(buildLogger, 0, configurationFile, layerCodes, modelManifest, vsProjectsFolder)
        {
        }

        public ClientImportVSProjectTask()
        {
        }

        public override void End()
        {
            BuildLogger.StoreLogFile(LogFile);
            AutoRunLogOutput.Output(BuildLogger, AutoRun);
        }

        public override Process Start()
        {
            if (!CheckVSProjectsDirectoryExists())
            {
                return null;
            }

            Deploy.Configs.ExtractClientLayerModelInfo(ConfigurationFile, LayerCodes, ModelManifest, out ModelName, out Publisher, out Layer, out LayerCode);

            LogFile = Path.Combine(BuildPaths.Temp, string.Format(@"AutoRun_VSImportLog_{0}.xml", Guid.NewGuid()));

            AutoRun = new Client.AutoRun.AxaptaAutoRun
            {
                ExitWhenDone = true,
                LogFile = LogFile,
            };

            var filesToProcess = from filter in new[] { "*.csproj", "*.dynamicsproj", "*.vbproj", "*.dwproj" }
                                 select Directory.GetFiles(VSProjectsFolder, filter, SearchOption.AllDirectories);

            foreach (string filename in filesToProcess.SelectMany(f => f))
            {
                AutoRun.Steps.Add(new Client.AutoRun.Run() { Type = Client.AutoRun.RunType.@class, Name = "SysTreeNodeVSProject", Method = "importProject", Parameters = string.Format("@'{0}'", filename) });
            }

            AutoRunFile = Path.Combine(BuildPaths.Temp, string.Format(@"AutoRun_VSImport_{0}.xml", Guid.NewGuid()));
            Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(AutoRun, AutoRunFile);
            BuildLogger.StoreLogFile(AutoRunFile);

            BuildLogger.LogInformation(string.Format("Importing VS Projects from folder {0} into model {1}", VSProjectsFolder, ModelName));
            Process process = Client.Client.StartCommand(new Client.Commands.AutoRun { ConfigurationFile = ConfigurationFile, Layer = Layer, LayerCode = LayerCode, Model = ModelName, ModelPublisher = Publisher, Filename = AutoRunFile });

            return process;
        }

        public bool CheckVSProjectsDirectoryExists()
        {
            if (!Directory.Exists(VSProjectsFolder))
            {
                BuildLogger.LogInformation(string.Format("Visual Studio projects folder {0} not found.", VSProjectsFolder));
                return false;
            }

            return true;
        }
    }
}
