using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Cmdlet(VerbsData.Restore, "Database")]
    public class Restore_Database : PSCmdlet
    {
        [Parameter(HelpMessage = "Name of the SQL Server instance", Mandatory = true)]
        public string ServerName;

        [Parameter(HelpMessage = "Name of the SQL Server database", Mandatory = true)]
        public string DatabaseName;

        [Parameter(HelpMessage = "Path to the configuration file", Mandatory = false)]
        public string ConfigurationFile;

        [Parameter(HelpMessage = "Path to the database backup file", Mandatory = true)]
        public string BackupFilePath;

        protected override void ProcessRecord()
        {
            RestoreDatabaseTask task = new RestoreDatabaseTask(VSTSBuildLogger.CreateDefault(), ServerName, DatabaseName, ConfigurationFile, BackupFilePath);
            task.RunInAppDomain();
        }
    }
}
