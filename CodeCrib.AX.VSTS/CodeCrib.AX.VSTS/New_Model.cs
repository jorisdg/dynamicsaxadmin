using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsCommon.New, "Model")]
    public class New_Model : PSCmdlet
    {
        [Parameter(HelpMessage = "Configuration file path", Mandatory = false)]
        public string ConfigurationFile = null;

        [Parameter(HelpMessage = "Path to the model manifest", Mandatory = true)]
        public string ModelManifest;

        [Parameter(HelpMessage = "Version number to override version contained in the model manifest", Mandatory = false)]
        public string VersionOverride;

        [Parameter(HelpMessage = "Model description to override description contained in the model manifest", Mandatory = false)]
        public string DescriptionOverride;

        [Parameter(HelpMessage = "Sets the noinstallmode flag in the model store after model creation", Mandatory = false)]
        public SwitchParameter SetNoInstallMode;

        protected override void ProcessRecord()
        {
            CreateModelTask task = new CreateModelTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, ModelManifest, VersionOverride, DescriptionOverride, SetNoInstallMode);
            task.RunInAppDomain();
        }
    }
}
