using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public abstract class CancelableBuildTask : BuildTask
    {
        protected CancelableBuildTask(
            IBuildLogger buildLogger,
            int timeoutMinutes,
            string configurationFile) : base (buildLogger, timeoutMinutes, configurationFile)
        {
        }

        protected CancelableBuildTask()
        {
        }

        public abstract Process Start();
        public abstract void End();
        public abstract void Cleanup(Process processToCleanUp);

        public void SetBuildLogger(IBuildLogger buildLogger)
        {
            BuildLogger = buildLogger;
        }
    }
}
