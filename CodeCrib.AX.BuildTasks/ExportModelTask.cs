using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ExportModelTask : BuildTask
    {
        protected string ModelManifestFilePath;
        protected string ModelFilePath;
        protected string AxUtilBinaryFolderPath;
        public string StrongNameKeyFilePath;

        public ExportModelTask()
        {
        }

        public ExportModelTask(
            IBuildLogger buildLogger, 
            string configurationFile, 
            string modelManifestFilePath, 
            string modelFilePath, 
            string axUtilBinaryFolderPath, 
            string strongNameKeyFilePath) : base(buildLogger, 0, configurationFile)
        {
            ModelManifestFilePath = modelManifestFilePath;
            ModelFilePath = modelFilePath;
            AxUtilBinaryFolderPath = axUtilBinaryFolderPath;
            StrongNameKeyFilePath = strongNameKeyFilePath;
        }

        public override void Run()
        {
            Manage.ModelStore.ExtractModelInfo(ModelManifestFilePath, out string publisher, out string modelName, out string layer);
            Config.Server serverConfig = Deploy.Configs.GetServerConfig(ConfigurationFile);

            Manage.ModelStore modelStore = null;
            if (serverConfig.AOSVersionOrigin.StartsWith("6.0", StringComparison.OrdinalIgnoreCase))
            {
                modelStore = new Manage.ModelStore(serverConfig.DatabaseServer, serverConfig.Database, AxUtilBinaryFolderPath);
            }
            else
            {
                modelStore = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database), AxUtilBinaryFolderPath);
            }

            BuildLogger.LogInformation(string.Format("Exporting model {0} ({1})", modelName, publisher));
            modelStore.ExportModel(modelName, publisher, ModelFilePath, StrongNameKeyFilePath);
        }
    }
}
