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
    public class StopAOSService : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }
        public InArgument<int> TimeOutMinutes { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);

            AOSStopTask task = new AOSStopTask(context.DefaultLogger(), timeOutMinutes, configurationFile);
            task.Run();
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class StartAOSService : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }
        public InArgument<int> TimeOutMinutes { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);

            AOSStartTask task = new AOSStartTask(context.DefaultLogger(), timeOutMinutes, configurationFile);
            task.Run();
        }
    }
}
