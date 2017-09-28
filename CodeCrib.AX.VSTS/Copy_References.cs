using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsCommon.Copy, "References")]
    public class Copy_References : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Folder path containing reference files to deploy", Mandatory = false)]
        public string ReferencesFolderPath;

        protected override void ProcessRecord()
        {
            DeployReferencesTask task = new DeployReferencesTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, ReferencesFolderPath);
            task.RunInAppDomain();
        }
    }
}
