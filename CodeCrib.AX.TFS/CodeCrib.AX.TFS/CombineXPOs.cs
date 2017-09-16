//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using CodeCrib.AX.BuildTasks;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class CombineXPOs : CodeActivity
    {
        [RequiredArgument]
        public InArgument<string> Folder { get; set; }
        public InArgument<bool> Recursive { get; set; }

        [RequiredArgument]
        public InArgument<string> CombinedXPOFile { get; set; }

        public InArgument<bool> IncludeSystemObjects { get; set; }
        public InArgument<bool> IncludeNonSystemObjects { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string folder = Folder.Get(context);
            bool recursive = Recursive.Get(context);
            string combinedXPOFile = CombinedXPOFile.Get(context);
            bool includeSystemObjects = IncludeSystemObjects.Get(context);
            bool includeNonSystemObjects = IncludeNonSystemObjects.Get(context);

            CombineXPOsTask task = new CombineXPOsTask(context.DefaultLogger(), folder, recursive, combinedXPOFile, includeSystemObjects, includeNonSystemObjects);
            task.Run();
        }
    }
}
