using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace CodeCrib.AX.Client.PowerShell
{
    [Cmdlet(VerbsLifecycle.Start, "AxClient")]
    public class Start_AxClient : PSCmdlet
    {
        [Parameter(HelpMessage = "Command to execute", Mandatory = true, ParameterSetName = "objects")]
        public Commands.Command command;

        [Parameter(HelpMessage = "Client object", Mandatory = true, ParameterSetName = "objects")]
        public Client client;

        [Parameter(HelpMessage = "Time-out (in minutes) for execution of command", Mandatory = false, ParameterSetName = "objects")]
        public int timeout;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "objects":
                    client.Execute(command, timeout);
                    break;
            }
        }
    }
}
