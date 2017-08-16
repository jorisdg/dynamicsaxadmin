using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CodeCrib.AX.Deploy.PowerShell
{
    [Cmdlet(VerbsCommon.Clear, "ClientCaches")]
    public class Clear_ClientCaches : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            Clean.ClientCaches();
        }
    }

    [Cmdlet(VerbsCommon.Clear, "ServerCaches")]
    public class Clear_ServerCaches : PSCmdlet
    {
        [Parameter(HelpMessage = "Client configuration object", Mandatory = true, ParameterSetName = "clientobjects")]
        public CodeCrib.AX.Config.Client clientConfig;

        [Parameter(HelpMessage = "Client configuration object", Mandatory = true, ParameterSetName = "serverobjects")]
        public CodeCrib.AX.Config.Server serverConfig;

        [Parameter(HelpMessage = "Client configuration file", Mandatory = true, ParameterSetName = "configfile")]
        public string clientConfigFile;

        [Parameter(HelpMessage = "Indicate if XPPIL files should be left", Mandatory = false, ParameterSetName = "clientobjects")]
        [Parameter(ParameterSetName = "configfile")]
        [Parameter(ParameterSetName = "serverobjects")]
        public SwitchParameter leaveXPPIL;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "clientobjects":
                    Clean.ServerCaches(clientConfig, leaveXPPIL);
                    break;
                case "serverobjects":
                    Clean.ServerCaches(serverConfig, leaveXPPIL);
                    break;
                case "configfile":
                    Clean.ServerCaches(clientConfigFile, leaveXPPIL);
                    break;
            }
        }
    }
}
