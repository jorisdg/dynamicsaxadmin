using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class RestoreDatabaseTask : DatabaseTask
    {
        protected string BackupFilePath;

        public RestoreDatabaseTask(
            IBuildLogger buildLogger, 
            string serverName, 
            string databaseName, 
            string configurationFile, 
            string backupFilePath) : base(buildLogger, configurationFile, serverName, databaseName)
        {
            BackupFilePath = backupFilePath;
        }

        public override void Run()
        {
            if (string.IsNullOrEmpty(ServerName))
            {
                var serverConfig = Deploy.Configs.GetServerConfig(ConfigurationFile);
                ServerName = serverConfig.DatabaseServer;
            }

            BuildLogger.LogInformation(String.Format("Restoring database {0} on server {1} from file {2}", DatabaseName, ServerName, BackupFilePath));

            Sql.DbManagement.RestoreDbFromFile(ServerName, DatabaseName, BackupFilePath);

            BuildLogger.LogInformation("Database restore complete");
        }
    }
}
