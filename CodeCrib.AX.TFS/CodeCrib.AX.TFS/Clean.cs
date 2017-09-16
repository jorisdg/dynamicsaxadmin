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
using System.IO;
using CodeCrib.AX.BuildTasks;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class Clean : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<bool> LeaveXppIL { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            bool leaveXppIL = LeaveXppIL.Get(context);
            string configurationFile = ConfigurationFile.Get(context);

            CleanTask task = new CleanTask(context.DefaultLogger(), leaveXppIL, configurationFile);
            task.Run();
        }
    }
}
