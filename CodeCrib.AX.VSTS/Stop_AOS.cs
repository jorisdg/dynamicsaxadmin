using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsLifecycle.Stop, "AOS")]
    public class Stop_AOS : PSCmdlet
    {
        [Parameter(HelpMessage = "Configuration file path", Mandatory = false)]
        public string ConfigurationFile = null;

        protected override void ProcessRecord()
        {
            AOSStopTask task = new AOSStopTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile);
            task.RunInAppDomain();            
        }
    }
}
