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

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class XPOLabelCheck : CodeActivity
    {
        [RequiredArgument]
        public InArgument<string> Folder { get; set; }
        public InArgument<bool> Recursive { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string folder = Folder.Get(context);
            bool recursive = Recursive.Get(context);

            var xpoList = Client.XPOLabelCheck.FindTempLabels(folder, recursive);
            foreach (var xpo in xpoList)
            {
                // Tracking error without exception will cause "partially succeeded"
                context.TrackBuildError(string.Format("Temporary label(s) found in XPO {0}", xpo));
            }
        }
    }
}
