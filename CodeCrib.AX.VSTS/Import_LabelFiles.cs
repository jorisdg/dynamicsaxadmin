using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Import, "LabelFiles")]
    public class Import_LabelFiles : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Array of layer access codes, e.g. @('cus:abc','var:def')", Mandatory = true)]
        public string[] LayerCodes;

        [Parameter(HelpMessage = "Path to the model manifest", Mandatory = true)]
        public string ModelManifest;

        [Parameter(HelpMessage = "Folder path containing the label files to import", Mandatory = true)]
        public string LabelFilesFolder;

        protected override void ProcessRecord()
        {
            using (var task = AppDomainWrapper<ClientImportLabelsTask>.Create())
            {
                ClientImportLabelsTask remoteTask = task.Facade.Task;

                remoteTask.BuildLogger = VSTSBuildLogger.CreateDefault();
                remoteTask.LayerCodes = LayerCodes.ToList();
                remoteTask.ModelManifest = ModelManifest;
                remoteTask.LabelFilesFolder = LabelFilesFolder;

                task.Facade.Task = remoteTask;

                task.Facade.Run();
            }

            //ClientImportLabelsTask task = new ClientImportLabelsTask(VSTSBuildLogger.CreateDefault(), ConfigurationFile, LayerCodes.ToList(), ModelManifest, LabelFilesFolder);
            //task.Run();
        }
    }
}
