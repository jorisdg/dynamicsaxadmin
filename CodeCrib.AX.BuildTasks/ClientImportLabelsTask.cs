using CodeCrib.AX.Client.AutoRun;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ClientImportLabelsTask : ClientBuildTask
    {
        protected string LabelFilesFolder;

        public ClientImportLabelsTask(
            IBuildLogger buildLogger,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest,
            string labelFilesFolder) : base(buildLogger, 0, configurationFile, layerCodes, modelManifest)
        {
            LabelFilesFolder = labelFilesFolder;
        }

        public ClientImportLabelsTask(
            IBuildLogger buildLogger, 
            int timeoutMinutes, 
            string configurationFile, 
            List<string> layerCodes, 
            string modelManifest, 
            string labelFilesFolder) : base(buildLogger, timeoutMinutes, configurationFile, layerCodes, modelManifest)
        {
            LabelFilesFolder = labelFilesFolder;
        }

        public ClientImportLabelsTask()
        {
        }

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

        public override Process Start()
        {
            if (!CheckLabelDirectoryExists())
            {
                return null;
            }

            Deploy.Configs.ExtractClientLayerModelInfo(ConfigurationFile, LayerCodes, ModelManifest, out ModelName, out Publisher, out Layer, out LayerCode);

            var clientConfig = Deploy.Configs.GetClientConfig(ConfigurationFile);
            AutoRun = new AxaptaAutoRun() { ExitWhenDone = true, LogFile = string.Format(@"{0}\LabelFlushLog-{1}.xml", Environment.ExpandEnvironmentVariables(clientConfig.LogDirectory), Guid.NewGuid()) };
            Client.Commands.ImportLabelFile importCommand = new Client.Commands.ImportLabelFile() { ConfigurationFile = ConfigurationFile, Layer = Layer, LayerCode = LayerCode, Model = ModelName, ModelPublisher = Publisher };

            foreach (string filename in Directory.GetFiles(LabelFilesFolder, "*.ald"))
            {
                if (!IsEmptyLabelFile(filename))
                {
                    BuildLogger.LogInformation(string.Format("Importing label file {0} into model {1} ({2})", filename, ModelName, Publisher));
                    importCommand.Filename = filename;
                    Client.Client.ExecuteCommand(importCommand, TimeoutMinutes);

                    string labelFile = Path.GetFileNameWithoutExtension(filename).Substring(2, 3);
                    string labelLanguage = Path.GetFileNameWithoutExtension(filename).Substring(5);

                    AutoRun.Steps.Add(new Client.AutoRun.Run() { Type = Client.AutoRun.RunType.@class, Name = "Global", Method = "info", Parameters = string.Format("strFmt(\"Flush label {0} language {1}: %1\", Label::flush(\"{0}\",\"{1}\"))", labelFile, labelLanguage) });
                }
            }

            AutoRunFile = string.Format(@"{0}\AutoRun-LabelFlush-{1}.xml", Environment.GetEnvironmentVariable("temp"), Guid.NewGuid());
            AxaptaAutoRun.SerializeAutoRun(AutoRun, AutoRunFile);

            BuildLogger.LogInformation(string.Format("Flushing imported label files"));
            BuildLogger.StoreLogFile(AutoRunFile);

            Process process = Client.Client.StartCommand(new Client.Commands.AutoRun() { ConfigurationFile = ConfigurationFile, Layer = Layer, LayerCode = LayerCode, Model = ModelName, ModelPublisher = Publisher, Filename = AutoRunFile });

            return process;
        }

        public override void End()
        {
            AutoRunLogOutput.Output(BuildLogger, AutoRun, true);
        }

        public bool CheckLabelDirectoryExists()
        {
            if (!Directory.Exists(LabelFilesFolder))
            {
                BuildLogger.LogInformation(string.Format("Label file folder {0} not found.", LabelFilesFolder));
                return false;
            }

            return true;
        }

    }
}