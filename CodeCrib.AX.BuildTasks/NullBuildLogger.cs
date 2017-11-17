using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    public class NullBuildLogger : IBuildLogger
    {
        public void LogError(string message)
        {
        }

        public void LogInformation(string message)
        {
        }

        public void LogWarning(string message)
        {
        }

        public void StoreLogFile(string logFilePath)
        {
        }
    }
}
