using CodeCrib.AX.BuildRuntime;
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
            AutoRun = new AxaptaAutoRun() { ExitWhenDone = true, LogFile = Path.Combine(BuildPaths.Temp, string.Format(@"AutoRun_LabelFlushLog_{0}.xml", Guid.NewGuid())) };
            Client.Commands.ImportLabelFile importCommand = new Client.Commands.ImportLabelFile() { ConfigurationFile = ConfigurationFile, Layer = Layer, LayerCode = LayerCode, Model = ModelName, ModelPublisher = Publisher };

            foreach (string filename in Directory.GetFiles(LabelFilesFolder, "*.ald"))
            {
                if (!IsEmptyLabelFile(filename))
                {
                    string labelFile = Path.GetFileNameWithoutExtension(filename).Substring(2, 3);
                    string labelLanguage = Path.GetFileNameWithoutExtension(filename).Substring(5);

                    BuildLogger.LogInformation(string.Format("Importing label file {0} into model {1} ({2})", filename, ModelName, Publisher));
                    importCommand.Filename = filename;
                    importCommand.Language = labelLanguage;
                    Client.Client.ExecuteCommand(importCommand, TimeoutMinutes);

                    AutoRun.Steps.Add(new Client.AutoRun.Run() { Type = Client.AutoRun.RunType.@class, Name = "Global", Method = "info", Parameters = string.Format("strFmt(\"Flush label {0} language {1}: %1\", Label::flush(\"{0}\",\"{1}\"))", labelFile, labelLanguage) });

                    AutoRunFile = Path.Combine(BuildPaths.Temp, string.Format(@"AutoRun_LabelFlush_{0}.xml", Guid.NewGuid()));
                    AxaptaAutoRun.SerializeAutoRun(AutoRun, AutoRunFile);

                    BuildLogger.LogInformation(string.Format($"Flushing imported label file {labelFile} language {labelLanguage}"));
                    BuildLogger.StoreLogFile(AutoRunFile);

                    Client.Commands.AutoRun flushCommand = new Client.Commands.AutoRun() { ConfigurationFile = ConfigurationFile, Layer = Layer, LayerCode = LayerCode, Model = ModelName, ModelPublisher = Publisher, Filename = AutoRunFile };
                    Client.Client.ExecuteCommand(flushCommand, TimeoutMinutes);
                }
            }

            return null;
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