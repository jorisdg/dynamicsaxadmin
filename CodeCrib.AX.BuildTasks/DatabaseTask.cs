using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    abstract public class DatabaseTask : BuildTask
    {
        protected string ServerName;
        protected string DatabaseName;

        public DatabaseTask(IBuildLogger buildLogger, string configurationFile, string serverName, string databaseName) : base(buildLogger, 0, configurationFile)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
        }
    }
}
