using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CodeCrib.AX.Manage.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "Server")]
    public class Get_Server : PSCmdlet
    {
        [Parameter(HelpMessage = "Name of the server, if ommitted uses localhost", Mandatory = false, ParameterSetName = "configurationserverport")]
        //[Parameter(Mandatory = true, ParameterSetName = "configurationserverport")]
        public string server;

        [Parameter(HelpMessage = "WSDL port for specific AOS on the server", Mandatory = true, ParameterSetName = "configurationserverport")]
        public int wsdlport;

        [Parameter(HelpMessage = "Gets the configuration for the specific aos on server", Mandatory = true, ParameterSetName = "configurationserverport")]
        public SwitchParameter configuration;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "configurationserverport":
                    if (string.IsNullOrEmpty(server))
                    {
                        this.WriteObject(CodeCrib.AX.Manage.ServerConfiguration.Get());
                    }
                    else
                    {
                        this.WriteObject(CodeCrib.AX.Manage.ServerConfiguration.Get(server, wsdlport));
                    }
                    break;
            }
        }
    }
}
