using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Backup, "Database")]
    public class Backup_Database : PSCmdlet
    {
        [Parameter(HelpMessage = "Name of the SQL Server instance", Mandatory = true)]
        public string ServerName;

        [Parameter(HelpMessage = "Name of the SQL Server database", Mandatory = true)]
        public string DatabaseName;

        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Path to the database backup file", Mandatory = true)]
        public string BackupFilePath;

        [Parameter(HelpMessage = "Overwrite existing backup sets", Mandatory = false)]
        public SwitchParameter OverwriteBackupSets;

        [Parameter(HelpMessage = "Always enable backup compression regardless of the server setting", Mandatory = false)]
        public SwitchParameter ForceCompressionOn;

        protected override void ProcessRecord()
        {
            BackupDatabaseTask task = new BackupDatabaseTask(VSTSBuildLogger.CreateDefault(), ServerName, DatabaseName, ConfigurationFile, BackupFilePath, OverwriteBackupSets, ForceCompressionOn);
            task.Run();
        }
    }
}
