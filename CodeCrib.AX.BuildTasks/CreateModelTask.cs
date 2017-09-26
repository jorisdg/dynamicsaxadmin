using CodeCrib.AX.BuildRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class CreateModelTask : BuildTask
    {
        protected string ModelManifest;
        protected string VersionOverride;
        protected string DescriptionOverride;
        protected bool SetNoInstallMode;

        public CreateModelTask(
            IBuildLogger buildLogger,
            int timeoutMinutes,
            string configurationFile,
            string modelManifest,
            string versionOverride,
            string descriptionOverride,
            bool setNoInstallMode) : base(buildLogger, timeoutMinutes, configurationFile)
        {
            ModelManifest = modelManifest;
            VersionOverride = versionOverride;
            DescriptionOverride = descriptionOverride;
            SetNoInstallMode = setNoInstallMode;
        }

        public CreateModelTask(
            IBuildLogger buildLogger, 
            string configurationFile, 
            string modelManifest, 
            string versionOverride, 
            string descriptionOverride, 
            bool setNoInstallMode) : base(buildLogger, 0, configurationFile)
        {
            ModelManifest = modelManifest;
            VersionOverride = versionOverride;
            DescriptionOverride = descriptionOverride;
            SetNoInstallMode = setNoInstallMode;
        }

        public CreateModelTask()
        {
        }

        public override void Run()
        {
            Config.Server serverConfig = Deploy.Configs.GetServerConfig(ConfigurationFile);

            Manage.ModelStore store = null;
            if (serverConfig.AOSVersionOrigin.StartsWith("6.0", StringComparison.OrdinalIgnoreCase))
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, serverConfig.Database);
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            BuildLogger.LogInformation(string.Format("Creating model for manifest {0}", ModelManifest));
            if (!string.IsNullOrEmpty(VersionOverride) ||
                !string.IsNullOrEmpty(DescriptionOverride))
            {
                store.CreateModel(ModelManifest, VersionOverride, DescriptionOverride);
            }
            else
            {
                store.CreateModel(ModelManifest);
            }

            if (SetNoInstallMode)
            {
                BuildLogger.LogInformation("Setting model store noinstallmode");
                store.SetNoInstallMode();
            }
        }
    }
}
