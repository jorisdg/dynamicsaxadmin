using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    abstract public class BuildTask
    {
        public IBuildLogger BuildLogger { get; set; }
        public int TimeoutMinutes { get; set; }
        public string ConfigurationFile { get; set; }

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
