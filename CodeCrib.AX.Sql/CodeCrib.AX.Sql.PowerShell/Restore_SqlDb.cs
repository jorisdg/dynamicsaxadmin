//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace CodeCrib.AX.Sql.PowerShell
{
    [Cmdlet(VerbsData.Restore, "SqlDB")]
    public class Restore_SqlDb : PSCmdlet
    {
        [Parameter(HelpMessage = "Name of the SQL server instance", Mandatory = true)]
        public string serverName;

        [Parameter(HelpMessage = "Path to the database backup file", Mandatory = true)]
        public string filename;

        [Parameter(HelpMessage = "Name of the database to restore to", Mandatory = true)]
        public string databaseName;

        protected override void ProcessRecord()
        {
            DbManagement.RestoreDbFromFile(serverName, databaseName, filename);
        }
    }
}
