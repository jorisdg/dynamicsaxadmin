//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CodeCrib.AX.Config.PowerShell
{
    /// <summary>
    /// Opens a Microsoft Dynamics AX client configuration from file or registry and returns the client object
    /// This will require administrator privileges if pulling from the business connector registry
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ClientConfiguration")]
    public class Get_ClientConfiguration : PSCmdlet
    {
        [Parameter(HelpMessage = "Get list of names of configurations", Mandatory = true, ParameterSetName = "registrylist")]
        public SwitchParameter listnames;

        [Parameter(HelpMessage = "Path to a configuration file", Mandatory = true, ParameterSetName = "filebased")]
        public string filename;

        [Parameter(HelpMessage = "Name of a configuration to be retrieved", Mandatory = true, ParameterSetName = "registrybased")]
        public string config;

        [Parameter(HelpMessage = "Indicate configuration to be retrieved is the active one", Mandatory = true, ParameterSetName = "registrybasedactive")]
        public SwitchParameter active;

        [Parameter(HelpMessage = "Indicate configuration is from user or Business Connector when retrieving from registry", ParameterSetName = "registrybased")]
        [Parameter(ParameterSetName = "registrybasedactive")]
        [Parameter(ParameterSetName = "registrylist")]
        public SwitchParameter BC;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "filebased":
                    this.WriteObject(CodeCrib.AX.Config.Client.GetConfigFromFile(filename));
                    break;
                case "registrylist":
                    this.WriteObject(CodeCrib.AX.Config.Client.GetConfigNameListFromRegistry(BC));
                    break;
                case "registrybased":
                    this.WriteObject(CodeCrib.AX.Config.Client.GetConfigFromRegistry(config, BC));
                    break;
                case "registrybasedactive":
                    this.WriteObject(CodeCrib.AX.Config.Client.GetConfigFromRegistry(BC));
                    break;
            }
        }
    }

    /// <summary>
    /// Accepts a client object over pipeline and writes the configuration to a Microsoft Dynamics AX configuration file
    /// This will require administrator privileges if saving to the business connector registry
    /// </summary>
    [Cmdlet(VerbsData.Save, "ClientConfiguration", DefaultParameterSetName = "registrybased")]
    public class Save_ClientConfiguration : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true, Mandatory = true, HelpMessage = "Client configuration object")]
        public Client clientConfiguration;

        [Parameter(HelpMessage = "Path to a (new) configuration file", Mandatory = true, ParameterSetName = "filebased")]
        public string filename;

        [Parameter(HelpMessage = "Name of a (new) configuration in registry", Mandatory = false, ParameterSetName = "registrybased")]
        public string config;

        [Parameter(HelpMessage = "Indicate configuration is for user or Business Connector when saving to registry", ParameterSetName = "registrybased")]
        public SwitchParameter BC;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "filebased":
                    CodeCrib.AX.Config.Client.SaveConfigToFile(clientConfiguration, filename);
                    this.WriteVerbose(string.Format("Configuration written to file {0}", filename));
                    break;
                case "registrybased":
                    if (!string.IsNullOrEmpty(config))
                    {
                        CodeCrib.AX.Config.Client.SaveConfigToRegistry(clientConfiguration, config, BC);
                        this.WriteVerbose(string.Format("Configuration written to {0}registry configuration {1}", BC ? "business connector " : "", config));
                    }
                    else
                    {
                        CodeCrib.AX.Config.Client.SaveConfigToRegistry(clientConfiguration, clientConfiguration.Configuration, BC);
                        this.WriteVerbose(string.Format("Configuration written to {0}registry configuration {1}", BC ? "business connector " : "", clientConfiguration.Configuration));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Sets the default configuration in the registry
    /// This will require administrator privileges if setting the business connector config
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ClientConfiguration")]
    public class Set_ClientConfiguration : PSCmdlet
    {
        [Parameter(HelpMessage = "Name of a configuration in registry to make default", Mandatory = true)]
        public string config;

        [Parameter(HelpMessage = "Indicate configuration is for user or Business Connector")]
        public SwitchParameter BC;

        protected override void ProcessRecord()
        {
            CodeCrib.AX.Config.Client.SetDefaultConfiguration(config, BC);
            this.WriteVerbose(string.Format("Default configuration {0}set to '{1}'", BC ? "for business connector " : "", config));
        }
    }
}
