//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Management.Smo;
using System.Data;
using System.IO;
using System.Data.Common;
using System.Data.SqlClient;

namespace CodeCrib.AX.Sql
{
    public class DbManagement
    {
        private const int DB_TIMEOUT = 600;
        private const string DB_FIELD_LOGICALNAME = "LogicalName";
        private const string DB_FIELD_TYPE = "Type";
        private const string DB_FILETYPE_LOG = "L";
        private const string DB_FILETYPE_DATA = "D";

        private const string DB_RESET_GLOBALGUID = @"UPDATE [dbo].[SYSSQMSETTINGS] SET [GLOBALGUID] = '{00000000-0000-0000-0000-000000000000}'";
        private const string DB_RESET_ADMIN = @"UPDATE [dbo].[USERINFO] SET [NETWORKDOMAIN] = '{0}', [NETWORKALIAS] = '{1}', [SID] = '{2}' WHERE [ID] = 'Admin'";
        private const string DB_CUSTEXP_OFF = @"UPDATE [dbo].[SYSUSERINFO] SET [SQMENABLED] = 1 WHERE [ID] = (SELECT [ID] FROM [dbo].[USERINFO] WHERE [dbo].[USERINFO].[SID] = '{0}')";

        // Instantiate a Server object and set all shared options.
        private static Server CreateServer(string serverName)
        {
            Server server = new Server(serverName);
            server.ConnectionContext.StatementTimeout = DB_TIMEOUT;
            return server;
        }

        public static void ResetGlobalGuid(string serverName, string dbName)
        {
            Server server = CreateServer(serverName);

            if (!server.Databases.Contains(dbName))
            {
                throw new Exception(String.Format("Database {0} does not exist", dbName));
            }

            Database database = new Database(server, dbName);

            // Not using database.ExecuteNonQuery here to avoid legacy .NET v2 runtime activation workaround.
            // Use the database name from the SMO database object rather than passed in value.
            using (DbConnection dbConnection = CreateConnection(serverName, database.Name))
            {
                dbConnection.Open();

                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    dbCommand.CommandText = DB_RESET_GLOBALGUID;
                    dbCommand.ExecuteNonQuery();
                }
            }
        }

        public static void ResetAdminUser(string serverName, string dbName)
        {
            Server server = CreateServer(serverName);

            if (!server.Databases.Contains(dbName))
            {
                throw new Exception(String.Format("Database {0} does not exist", dbName));
            }

            Database database = new Database(server, dbName);

            // Not using database.ExecuteNonQuery here to avoid legacy .NET v2 runtime activation workaround.
            // Use the database name from the SMO database object rather than passed in value.
            using (DbConnection dbConnection = CreateConnection(serverName, database.Name))
            {
                dbConnection.Open();

                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                    dbCommand.CommandText = string.Format(DB_RESET_ADMIN, Environment.UserDomainName, Environment.UserName, identity.User.Value.ToString());
                    dbCommand.ExecuteNonQuery();
                }
            }
        }

        public static void DisableCustomerExperienceDialog(string serverName, string dbName)
        {
            Server server = CreateServer(serverName);

            if (!server.Databases.Contains(dbName))
            {
                throw new Exception(String.Format("Database {0} does not exist", dbName));
            }

            Database database = new Database(server, dbName);

            // Not using database.ExecuteNonQuery here to avoid legacy .NET v2 runtime activation workaround.
            // Use the database name from the SMO database object rather than passed in value.
            using (DbConnection dbConnection = CreateConnection(serverName, database.Name))
            {
                dbConnection.Open();

                using (DbCommand dbCommand = dbConnection.CreateCommand())
                {
                    var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                    dbCommand.CommandText = string.Format(DB_CUSTEXP_OFF, identity.User.Value.ToString());
                    dbCommand.ExecuteNonQuery();
                }
            }
        }

        private static string NewFileName(Server server, string fileType)
        {
            string newFileName;

            if (fileType.Equals(DB_FILETYPE_DATA, StringComparison.InvariantCultureIgnoreCase))
            {
                newFileName = Path.Combine(server.DataPath(), String.Format("{0}.mdf", Guid.NewGuid().ToString("N")));
            }
            else if (fileType.Equals(DB_FILETYPE_LOG, StringComparison.InvariantCultureIgnoreCase))
            {
                newFileName = Path.Combine(server.LogPath(), String.Format("{0}.ldf", Guid.NewGuid().ToString("N")));
            }
            else
            {
                throw new Exception(String.Format("Unsupported logical file type in backup: {0}", fileType));
            }

            return newFileName;
        }

        public static void RestoreDbFromFile(string serverName, string dbName, string fileName)
        {
            Server server = CreateServer(serverName);
            
            Restore restore = new Restore
            {
                Database = dbName
            };

            restore.Devices.AddDevice(fileName, DeviceType.File);

            foreach (DataRow row in restore.ReadFileList(server).Rows)
            {
                RelocateFile relocateFile = new RelocateFile
                {
                    LogicalFileName = (string)row[DB_FIELD_LOGICALNAME],
                    PhysicalFileName = NewFileName(server, (string)row[DB_FIELD_TYPE])
                };

                restore.RelocateFiles.Add(relocateFile);
            }

            if (server.Databases.Contains(dbName))
            {
                // Wait until just before restoring to drop any existing database.
                server.KillDatabase(dbName);
            }

            restore.SqlRestore(server);
        }

        /// <summary>
        /// Backs up a database to a file.
        /// </summary>
        /// <param name="serverName">The database server name</param>
        /// <param name="dbName">The database to back up </param>
        /// <param name="fileName">The destination file name</param>
        /// <param name="copyOnlyBackup">Should the backup be marked as copy only?</param>
        /// <param name="overwriteBackupSets">Should any existing backup sets in the file be overwritten?</param>
        /// <param name="forceCompressionOn">Should the backup be compressed regardless of the server default setting?</param>
        public static void BackupDbToFile(string serverName, string dbName, string fileName, bool copyOnlyBackup = true, bool overwriteBackupSets = false, bool forceCompressionOn = false)
        {
            Server server = CreateServer(serverName);

            Backup backup = new Backup()
            {
                Database = dbName,
                CompressionOption = forceCompressionOn ? BackupCompressionOptions.On : BackupCompressionOptions.Default,
                CopyOnly = copyOnlyBackup,
                Incremental = false,
                Initialize = overwriteBackupSets,
                // Do not check expiration or naming of backup sets when overwriting.
                SkipTapeHeader = overwriteBackupSets,
                Action = BackupActionType.Database
            };

            backup.Devices.AddDevice(fileName, DeviceType.File);

            backup.SqlBackup(server);
        }

        public static void CopyDatabase(string serverNameSrc, string dbNameSrc, string serverNameDest, string dbNameDest, string tempFileSharedPath)
        {
            string tempFileName = Path.Combine(tempFileSharedPath, String.Format("{0}.bak", Guid.NewGuid().ToString("N")));

            try
            {
                BackupDbToFile(serverNameSrc, dbNameSrc, tempFileName);
                RestoreDbFromFile(serverNameDest, dbNameDest, tempFileName);
            }
            finally
            {
                File.Delete(tempFileName);
            }
        }

        // Creates a connection separate from SMO for compatibility.
        public static DbConnection CreateConnection(string serverName, string database = "")
        {
            SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder()
            {
                IntegratedSecurity = true,
                DataSource = serverName,
                InitialCatalog = database
            };

            DbConnection dbConnection = null;

            try
            {
                dbConnection = SqlClientFactory.Instance.CreateConnection();
                dbConnection.ConnectionString = connectionStringBuilder.ConnectionString;

                return dbConnection;
            }
            catch
            {
                if (dbConnection != null)
                {
                    dbConnection.Dispose();
                }

                throw;
            }
        }
    }
}
