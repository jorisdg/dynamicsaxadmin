using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Import, "XpoFile")]
    public class Import_XpoFile : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Array of layer access codes, e.g. @('cus:abc','var:def')", Mandatory = true)]
        public string[] LayerCodes;

        [Parameter(HelpMessage = "Path to the model manifest", Mandatory = true)]
        public string ModelManifest;

        [Parameter(HelpMessage = "Path to the XPO file to import", Mandatory = true)]
        public string XpoFilePath;

        protected override void ProcessRecord()
        {
            ClientImportXpoTask task = new ClientImportXpoTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, LayerCodes.ToList(), ModelManifest, XpoFilePath);
            task.Run();
        }
    }
}
