using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Import, "VSProjects")]
    public class Import_VSProjects : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Array of layer access codes, e.g. @('cus:abc','var:def')", Mandatory = true)]
        public string[] LayerCodes;

        [Parameter(HelpMessage = "Path to the model manifest", Mandatory = true)]
        public string ModelManifest;

        [Parameter(HelpMessage = "Folder path containing the Visual Studio projects to import", Mandatory = true)]
        public string VSProjectsFolder;

        protected override void ProcessRecord()
        {
            ClientImportVSProjectTask task = new ClientImportVSProjectTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, LayerCodes.ToList(), ModelManifest, VSProjectsFolder);
            task.RunInAppDomain();
        }
    }
}
