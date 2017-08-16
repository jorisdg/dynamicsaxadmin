using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management.Automation;

namespace CodeCrib.AX.Deploy.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "ServerConfigFromClient")]
    public class Get_ServerConfigFromClient : PSCmdlet
    {
        [Parameter(HelpMessage = "Client configuration object", Mandatory = true, ParameterSetName = "objects")]
        public CodeCrib.AX.Config.Client clientConfig;

        [Parameter(HelpMessage = "Client configuration file", Mandatory = true, ParameterSetName = "configfile")]
        public string clientConfigFile;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "objects":
                    this.WriteObject(Configs.GetServerConfig(clientConfig));
                    break;
                case "configfile":
                    this.WriteObject(Configs.GetServerConfig(clientConfigFile));
                    break;
            }
        }
    }
}
