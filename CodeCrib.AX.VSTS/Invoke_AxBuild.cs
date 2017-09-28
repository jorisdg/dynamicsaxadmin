using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsLifecycle.Invoke, "AxBuild")]
    public class Invoke_AxBuild : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Number of AxBuild worker processes", Mandatory = false)]
        public int Workers;

        [Parameter(HelpMessage = "Folder path to alternate binaries", Mandatory = false)]
        public string AlternateBinaryFolder;

        protected override void ProcessRecord()
        {
            AxBuildTask task = new AxBuildTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, Workers, AlternateBinaryFolder);
            task.RunInAppDomain();
        }
    }
}
