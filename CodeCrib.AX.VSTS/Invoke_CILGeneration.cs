using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsLifecycle.Invoke, "CILGeneration")]
    public class Invoke_CILGeneration : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        protected override void ProcessRecord()
        {
            ClientGenerateCILTask task = new ClientGenerateCILTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile);
            task.RunInAppDomain();
        }
    }
}
