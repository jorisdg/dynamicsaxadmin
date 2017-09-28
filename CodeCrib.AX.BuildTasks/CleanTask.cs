using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class CleanTask : BuildTask
    {
        protected bool LeaveXppIL;

        public CleanTask(IBuildLogger buildLogger, bool leaveXppIL, string configurationFile) : base(buildLogger, 0, configurationFile)
        {
            LeaveXppIL = leaveXppIL;
        }

        public CleanTask()
        {
        }

        public override void Run()
        {
            BuildLogger.LogInformation("Cleaning server artifacts");
            Deploy.Clean.ServerCaches(ConfigurationFile, LeaveXppIL);

            BuildLogger.LogInformation("Cleaning client artifacts");
            Deploy.Clean.ClientCaches();
        }
    }
}
