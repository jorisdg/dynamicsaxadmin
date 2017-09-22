using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Merge, "XpoFiles")]
    public class Merge_XpoFiles : PSCmdlet
    {
        [Parameter(HelpMessage = "Path to the folder containing XPO files to merge", Mandatory = true)]
        public string Folder;

        [Parameter(HelpMessage = "Process folder contents recursively", Mandatory = false)]
        public SwitchParameter Recursive;

        [Parameter(HelpMessage = "Path to the resulting merged XPO file", Mandatory = true)]
        public string OutputFilePath;

        [Parameter(HelpMessage = "Include system objects in the merged XPO file", Mandatory = false)]
        public SwitchParameter IncludeSystemObjects;

        [Parameter(HelpMessage = "Include non-system objects in the merged XPO file", Mandatory = false)]
        public SwitchParameter IncludeNonSystemObjects;

        protected override void ProcessRecord()
        {
            CombineXPOsTask task = new CombineXPOsTask(VSTSBuildLogger.CreateDefault(), Folder, Recursive, OutputFilePath, IncludeSystemObjects, IncludeNonSystemObjects);
            task.Run();
        }
    }
}
