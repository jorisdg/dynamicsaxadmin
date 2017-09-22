using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Initialize, "EnvironmentFiles")]
    public class Initialize_EnvironmentFiles : PSCmdlet
    {
        [Parameter(HelpMessage = "Preserve XPPIL files", Mandatory = false)]
        public SwitchParameter LeaveXppIL = false;

        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile = null;

        protected override void ProcessRecord()
        {
            CleanTask task = new CleanTask(VSTSBuildLogger.CreateDefault(), LeaveXppIL, ConfigurationFile);
            task.Run();
        }
    }
}
