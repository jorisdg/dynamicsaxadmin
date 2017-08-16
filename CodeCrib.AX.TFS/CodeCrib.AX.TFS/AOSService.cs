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
    public class StopAOSService : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }
        public InArgument<int> TimeOutMinutes { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);
            var aosNumber = Helper.GetServerNumber(configurationFile);

            CodeCrib.AX.Manage.AOS aos = new Manage.AOS(aosNumber);
            aos.Stop(timeOutMinutes);
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
            var aosNumber = Helper.GetServerNumber(configurationFile);

            CodeCrib.AX.Manage.AOS aos = new Manage.AOS(aosNumber);
            aos.Start(timeOutMinutes);
        }
    }
}
