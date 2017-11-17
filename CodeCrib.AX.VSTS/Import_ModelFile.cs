using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Import, "ModelFiles")]
    public class Import_ModelFiles : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Destination paths to the exported model", Mandatory = false)]
        public string[] ModelFilePaths;

        [Parameter(HelpMessage = "Set the noinstallmode flag in the model store after importing the model", Mandatory = false)]
        public SwitchParameter SetNoInstallMode;

        [Parameter(HelpMessage = "Overwrite model if it already exists in the model store", Mandatory = false)]
        public SwitchParameter OverwriteExisting;

        protected override void ProcessRecord()
        {
            if (ModelFilePaths != null &&
                ModelFilePaths.Length > 0)
            {
                foreach (string modelFilePath in ModelFilePaths)
                {
                    ImportModelTask importModelTask = new ImportModelTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, modelFilePath, SetNoInstallMode, OverwriteExisting);
                    importModelTask.RunInAppDomain();
                }
            }
        }
    }
}
