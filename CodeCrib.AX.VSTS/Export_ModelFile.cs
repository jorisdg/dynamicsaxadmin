using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Export, "ModelFile")]
    public class Export_ModelFile : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Destination path to the exported model", Mandatory = true)]
        public string ModelFilePath;

        [Parameter(HelpMessage = "Path to the model manifest file indicating which model to export", Mandatory = true)]
        public string ModelManifestFilePath;

        [Parameter(HelpMessage = "Folder path to an alternate axutil tool for use during model export", Mandatory = false)]
        public string AxUtilBinaryFolderPath;

        [Parameter(HelpMessage = "Path to a strong name key file for use in model signing", Mandatory = false)]
        public string StrongNameKeyFilePath;

        protected override void ProcessRecord()
        {
            ExportModelTask task = new ExportModelTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, ModelManifestFilePath, ModelFilePath, AxUtilBinaryFolderPath, StrongNameKeyFilePath);
            task.RunInAppDomain();
        }
    }
}
