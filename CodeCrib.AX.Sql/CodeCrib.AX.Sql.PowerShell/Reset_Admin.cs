using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CodeCrib.AX.Sql.PowerShell
{
    [Cmdlet(VerbsCommon.Reset, "Admin")]
    public class Reset_Admin : PSCmdlet
    {
        [Parameter(HelpMessage = "Name of the SQL server instance", Mandatory = true)]
        public string serverName;

        [Parameter(HelpMessage = "Name of the database", Mandatory = true)]
        public string databaseName;

        protected override void ProcessRecord()
        {
            DbManagement.ResetAdminUser(serverName, databaseName);
        }
    }
}
