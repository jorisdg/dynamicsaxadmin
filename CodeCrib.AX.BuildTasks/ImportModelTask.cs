using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ImportModelTask : BuildTask
    {
        protected string ModelFilePath;
        protected bool SetNoInstallMode;
        protected bool OverwriteExisting;

        public ImportModelTask()
        {
        }

        public ImportModelTask(
            IBuildLogger buildLogger,
            string configurationFile,
            string modelFilePath,
            bool setNoInstallMode,
            bool overwriteExisting) : base(buildLogger, 0, configurationFile)
        {
            ModelFilePath = modelFilePath;
            SetNoInstallMode = setNoInstallMode;
            OverwriteExisting = overwriteExisting;
        }

        public override void Run()
        {
            Config.Server serverConfig = Deploy.Configs.GetServerConfig(ConfigurationFile);

            Manage.ModelStore store;
            if (serverConfig.AOSVersionOrigin.StartsWith("6.0", StringComparison.InvariantCultureIgnoreCase))
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, serverConfig.Database);
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            if (OverwriteExisting)
            {
                store.InstallModelOverwrite(ModelFilePath);
            }
            else
            {
                store.InstallModel(ModelFilePath);
            }

            if (SetNoInstallMode)
            {
                store.SetNoInstallMode();
            }
        }
    }
}
