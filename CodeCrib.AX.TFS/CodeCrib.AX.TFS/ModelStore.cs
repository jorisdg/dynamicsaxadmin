//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System.IO;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class CreateModel : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        [RequiredArgument]
        public InArgument<string> ModelManifestFile { get; set; }

        public InArgument<string> VersionOverride { get; set; }
        public InArgument<string> DescriptionOverride { get; set; }

        public InArgument<bool> SetNoInstallMode { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            string manifestFile = ModelManifestFile.Get(context);
            string version = VersionOverride.Get(context);
            string description = DescriptionOverride.Get(context);
            var serverConfig = Helper.GetServerConfig(configurationFile);

            CodeCrib.AX.Manage.ModelStore store = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3) == "6.0")
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database));
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            if (!string.IsNullOrEmpty(version) || !string.IsNullOrEmpty(description))
                store.CreateModel(manifestFile, version, description);
            else
                store.CreateModel(manifestFile);

            if (SetNoInstallMode.Get(context))
                store.SetNoInstallMode();
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class ExportModel : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        [RequiredArgument]
        public InArgument<string> ModelFile { get; set; }
        [RequiredArgument]
        public InArgument<string> ModelManifestFile { get; set; }

        public InArgument<string> AxUtilBinaryFolder { get; set; }

        public InArgument<string> StrongNameKeyFile { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            string modelFile = ModelFile.Get(context);
            string axutilFolder = AxUtilBinaryFolder.Get(context);
            string modelName, publisher, layer;
            CodeCrib.AX.Manage.ModelStore.ExtractModelInfo(ModelManifestFile.Get(context), out publisher, out modelName, out layer);
            string keyFile = StrongNameKeyFile.Get(context);
            var serverConfig = Helper.GetServerConfig(configurationFile);

            CodeCrib.AX.Manage.ModelStore store = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3) == "6.0")
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database), axutilFolder);
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database), axutilFolder);
            }

            context.TrackBuildMessage(string.Format("Exporting model {0} ({1})", modelName, publisher));
            store.ExportModel(modelName, publisher, modelFile, keyFile);
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class InstallModel : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        [RequiredArgument]
        public InArgument<string> ModelFile { get; set; }

        public InArgument<bool> SetNoInstallMode { get; set; }

        public InArgument<bool> OverwriteExisting { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            string modelFile = ModelFile.Get(context);
            var serverConfig = Helper.GetServerConfig(configurationFile);
            bool overwrite = OverwriteExisting.Get(context);

            CodeCrib.AX.Manage.ModelStore store = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3) == "6.0")
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database));
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            if (overwrite)
            {
                store.InstallModelOverwrite(modelFile);
            }
            else
            {
            store.InstallModel(modelFile);
            }
            if (SetNoInstallMode.Get(context))
                store.SetNoInstallMode();
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class ExportModelStore : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        [RequiredArgument]
        public InArgument<string> ModelStoreFile { get; set; }

        public InArgument<string> AxUtilBinaryFolder { get; set; }

        public InArgument<bool> CompressModelStore { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            string modelStoreFile = ModelStoreFile.Get(context);
            string axutilFolder = AxUtilBinaryFolder.Get(context);
            var serverConfig = Helper.GetServerConfig(configurationFile);
            bool compressModelStore = CompressModelStore.Get(context);

            CodeCrib.AX.Manage.ModelStore store = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3) == "6.0")
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database), axutilFolder);
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database), axutilFolder);
            }

            store.ExportModelStore(modelStoreFile);

            if (compressModelStore)
            {
                Helper.CompressFile(new FileInfo(modelStoreFile));
            }
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class UninstallModel : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        [RequiredArgument]
        public InArgument<string> ModelManifestFile { get; set; }

        public InArgument<bool> SetNoInstallMode { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            string modelName, publisher, layer;
            CodeCrib.AX.Manage.ModelStore.ExtractModelInfo(ModelManifestFile.Get(context), out publisher, out modelName, out layer);
            var serverConfig = Helper.GetServerConfig(configurationFile);

            CodeCrib.AX.Manage.ModelStore store = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3) == "6.0")
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database));
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            context.TrackBuildMessage(string.Format("Uninstalling model {0} ({1})", modelName, publisher));
            store.UninstallModel(modelName, publisher);
            if (SetNoInstallMode.Get(context))
                store.SetNoInstallMode();
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class UninstallAllModelsFromLayer : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        [RequiredArgument]
        public InArgument<string> Layer { get; set; }

        public InArgument<bool> SetNoInstallMode { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            string layer = Layer.Get(context);
            var serverConfig = Helper.GetServerConfig(configurationFile);

            CodeCrib.AX.Manage.ModelStore store = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3) == "6.0")
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database));
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            context.TrackBuildMessage(string.Format("Uninstalling all models from layer {0}", layer));
            store.UninstallAllLayerModels(layer);
            if (SetNoInstallMode.Get(context))
                store.SetNoInstallMode();
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class UninstallAllModelsFromModelLayer : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        [RequiredArgument]
        public InArgument<string> ModelManifestFile { get; set; }

        public InArgument<bool> SetNoInstallMode { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            string modelName, publisher, layer;
            CodeCrib.AX.Manage.ModelStore.ExtractModelInfo(ModelManifestFile.Get(context), out publisher, out modelName, out layer);
            var serverConfig = Helper.GetServerConfig(configurationFile);

            CodeCrib.AX.Manage.ModelStore store = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3) == "6.0")
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database));
            }
            else
            {
                store = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            context.TrackBuildMessage(string.Format("Uninstalling all models from layer {0}", layer));
            store.UninstallAllLayerModels(layer);
            if (SetNoInstallMode.Get(context))
                store.SetNoInstallMode();
        }
    }
}
