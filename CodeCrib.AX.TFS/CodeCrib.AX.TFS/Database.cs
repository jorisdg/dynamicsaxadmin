//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using CodeCrib.AX.BuildTasks;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class RestoreDb : CodeActivity
    {
        public InArgument<string> ServerName { get; set; }
        [RequiredArgument]
        public InArgument<string> DatabaseName { get; set; }
        [RequiredArgument]
        public InArgument<string> BackupFilePath { get; set; }
        public InArgument<string> ConfigurationFile { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string serverName = ServerName.Get(context);
            string databaseName = DatabaseName.Get(context);
            string backupFilePath = BackupFilePath.Get(context);
            string configurationFile = ConfigurationFile.Get(context);

            RestoreDatabaseTask task = new RestoreDatabaseTask(context.DefaultLogger(), serverName, databaseName, configurationFile, backupFilePath);
            task.Run();
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class BackupDb : CodeActivity
    {
        public InArgument<string> ServerName { get; set; }
        [RequiredArgument]
        public InArgument<string> DatabaseName { get; set; }
        [RequiredArgument]
        public InArgument<string> BackupFilePath { get; set; }
        public InArgument<bool> OverwriteBackupSets { get; set; }
        public InArgument<bool> ForceCompressionOn { get; set; }
        public InArgument<string> ConfigurationFile { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string serverName = ServerName.Get(context);
            string databaseName = DatabaseName.Get(context);
            string backupFilePath = BackupFilePath.Get(context);
            bool overwriteBackupSets = OverwriteBackupSets.Get(context);
            bool forceCompressionOn = ForceCompressionOn.Get(context);
            string configurationFile = ConfigurationFile.Get(context);

            BackupDatabaseTask task = new BackupDatabaseTask(context.DefaultLogger(), serverName, databaseName, configurationFile, backupFilePath, overwriteBackupSets, forceCompressionOn);
            task.Run();
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class ResetAdmin : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            var serverConfig = CodeCrib.AX.Deploy.Configs.GetServerConfig(configurationFile);
            string serverName = serverConfig.DatabaseServer;
            string databaseName = serverConfig.Database;

            context.TrackBuildMessage(String.Format("Resetting admin on database {0} on server {1}", databaseName, serverName));

            Sql.DbManagement.ResetAdminUser(serverName, databaseName);
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class DisableCustomerExperience : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            var serverConfig = CodeCrib.AX.Deploy.Configs.GetServerConfig(configurationFile);
            string serverName = serverConfig.DatabaseServer;
            string databaseName = serverConfig.Database;

            context.TrackBuildMessage(String.Format("Disable customer experience dialog on database {0} on server {1}", databaseName, serverName));

            Sql.DbManagement.DisableCustomerExperienceDialog(serverName, databaseName);
        }
    }
}
