using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    public class AOSStopTask : BuildTask
    {
        public AOSStopTask(IBuildLogger buildLogger, int timeoutMinutes, string configurationFile) : base(buildLogger, timeoutMinutes, configurationFile)
        {
        }

        public override void Run()
        {
            var aosNumber = Deploy.Configs.GetServerNumber(ConfigurationFile);

            Manage.AOS aos = new Manage.AOS(aosNumber);
            aos.Stop(TimeoutMinutes);
        }
    }
}
