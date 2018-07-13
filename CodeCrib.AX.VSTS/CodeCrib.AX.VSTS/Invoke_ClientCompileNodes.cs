using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsLifecycle.Invoke, "ClientCompileNodes")]
    public class Invoke_ClientCompileNodes : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Array of layer access codes, e.g. @('cus:abc','var:def')", Mandatory = true)]
        public string[] LayerCodes;

        [Parameter(HelpMessage = "Path to the model manifest", Mandatory = true)]
        public string ModelManifest;

        [Parameter(HelpMessage = "AOT paths identifying the objects to compile", Mandatory = false)]
        public string[] AOTCompilePaths;

        protected override void ProcessRecord()
        {
            ClientCompileNodesTask task = new ClientCompileNodesTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, LayerCodes.ToList(), ModelManifest, AOTCompilePaths);
            task.RunInAppDomain();
        }
    }
}
