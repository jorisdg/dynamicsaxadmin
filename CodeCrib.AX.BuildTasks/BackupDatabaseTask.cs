using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    public class BackupDatabaseTask : DatabaseTask
    {
        protected string BackupFilePath;
        protected bool OverwriteBackupSets;
        protected bool ForceCompressionOn;

        public BackupDatabaseTask(
            IBuildLogger buildLogger,
            string serverName,
            string databaseName,
            string configurationFile,
            string backupFilePath, 
            bool overwriteBackupSets, 
            bool forceCompressionOn) : base(buildLogger, configurationFile, serverName, databaseName)
        {
            BackupFilePath = backupFilePath;
            OverwriteBackupSets = overwriteBackupSets;
            ForceCompressionOn = forceCompressionOn;
        }

        public override void Run()
        {
            if (string.IsNullOrEmpty(ServerName))
            {
                var serverConfig = Deploy.Configs.GetServerConfig(ConfigurationFile);
                ServerName = serverConfig.DatabaseServer;
            }

            BuildLogger.LogInformation(String.Format("Backing up database {0} from server {1} to file {2}", DatabaseName, ServerName, BackupFilePath));

            Sql.DbManagement.BackupDbToFile(ServerName, DatabaseName, BackupFilePath, true, OverwriteBackupSets, ForceCompressionOn);

            BuildLogger.LogInformation("Database backup complete");
        }
    }
}
