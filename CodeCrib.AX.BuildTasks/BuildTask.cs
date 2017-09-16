using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    abstract public class BuildTask
    {
        protected IBuildLogger BuildLogger;
        protected int TimeoutMinutes;
        protected string ConfigurationFile;

        protected BuildTask(IBuildLogger buildLogger, int timeoutMinutes, string configurationFile)
        {
            BuildLogger = buildLogger ?? throw new ArgumentNullException(nameof(buildLogger));
            TimeoutMinutes = timeoutMinutes;
            ConfigurationFile = configurationFile;
        }

        protected BuildTask(IBuildLogger buildLogger)
        {
            BuildLogger = buildLogger;
        }

        protected BuildTask()
        {
        }

        abstract public void Run();
    }
}
