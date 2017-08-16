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
    /// Opens a Microsoft Dynamics AX Server configuration from file or registry and returns the Server object
    /// This will require administrator privileges
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "ServerConfiguration")]
    public class Get_ServerConfiguration : PSCmdlet
    {
        [Parameter(HelpMessage = "Get list of names of configurations", Mandatory = true, ParameterSetName = "registrylist")]
        [Parameter(Mandatory = true, ParameterSetName = "registrylistname")]
        public SwitchParameter listnames;

        [Parameter(HelpMessage = "Path to a configuration file", Mandatory = true, ParameterSetName = "filebased")]
        public string filename;

        [Parameter(HelpMessage = "Name of a configuration to be retrieved", Mandatory = true, ParameterSetName = "registrybased")]
        [Parameter(Mandatory = true, ParameterSetName = "registrybasedname")]
        public string config;

        [Parameter(HelpMessage = "Indicate configuration to be retrieved is the active one", Mandatory = true, ParameterSetName = "registrybasedactive")]
        [Parameter(Mandatory = true, ParameterSetName = "registrybasedactivename")]
        public SwitchParameter active;

        [Parameter(HelpMessage = "Identify AOS by its ID", Mandatory = true, ParameterSetName = "registrybasedactive")]
        [Parameter(Mandatory = true, ParameterSetName = "registrybased")]
        [Parameter(Mandatory = true, ParameterSetName = "registrylist")]
        public uint aosnumber;

        [Parameter(HelpMessage = "Identify AOS by its name", Mandatory = true, ParameterSetName = "registrybasedactivename")]
        [Parameter(Mandatory = true, ParameterSetName = "registrybasedname")]
        [Parameter(Mandatory = true, ParameterSetName = "registrylistname")]
        public string aosname;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "filebased":
                    this.WriteObject(CodeCrib.AX.Config.Server.GetConfigFromFile(filename));
                    break;
                case "registrylist":
                    this.WriteObject(CodeCrib.AX.Config.Server.GetConfigNameListFromRegistry(aosnumber));
                    break;
                case "registrylistname":
                    this.WriteObject(CodeCrib.AX.Config.Server.GetConfigNameListFromRegistry(aosname));
                    break;
                case "registrybased":
                    this.WriteObject(CodeCrib.AX.Config.Server.GetConfigFromRegistry(aosnumber, config));
                    break;
                case "registrybasedname":
                    this.WriteObject(CodeCrib.AX.Config.Server.GetConfigFromRegistry(aosname, config));
                    break;
                case "registrybasedactive":
                    this.WriteObject(CodeCrib.AX.Config.Server.GetConfigFromRegistry(aosnumber));
                    break;
                case "registrybasedactivename":
                    this.WriteObject(CodeCrib.AX.Config.Server.GetConfigFromRegistry(aosname));
                    break;
            }
        }
    }

    /// <summary>
    /// Gets a string list of installed AOS instance names
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AOSNames")]
    public class Get_AOSNames : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            this.WriteObject(CodeCrib.AX.Config.Server.GetAOSInstances());
        }
    }

    /// <summary>
    /// Gets a uint list of installed AOS IDs
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "AOSIDs")]
    public class Get_AOSIDs : PSCmdlet
    {
        protected override void ProcessRecord()
        {
            this.WriteObject(CodeCrib.AX.Config.Server.GetAOSes());
        }
    }

    /// <summary>
    /// Accepts a Server object over pipeline and writes the configuration to a Microsoft Dynamics AX configuration file
    /// This will require administrator privileges
    /// </summary>
    [Cmdlet(VerbsData.Save, "ServerConfiguration", DefaultParameterSetName = "registrybased")]
    public class Save_ServerConfiguration : PSCmdlet
    {
        [Parameter(ValueFromPipeline = true, Mandatory = true, HelpMessage = "Server configuration object")]
        public Server ServerConfiguration;

        [Parameter(HelpMessage = "Path to a (new) configuration file", Mandatory = true, ParameterSetName = "filebased")]
        public string filename;

        [Parameter(HelpMessage = "Name of a (new) configuration in registry", Mandatory = false, ParameterSetName = "registrybased")]
        [Parameter(ParameterSetName = "registrybasedname")]
        public string config;

        [Parameter(HelpMessage = "Identify AOS by its ID", Mandatory = true, ParameterSetName = "registrybased")]
        public uint aosnumber;

        [Parameter(HelpMessage = "Identify AOS by its name", Mandatory = true, ParameterSetName = "registrybasedname")]
        public string aosname;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "filebased":
                    CodeCrib.AX.Config.Server.SaveConfigToFile(ServerConfiguration, filename);
                    this.WriteVerbose(string.Format("Configuration written to file {0}", filename));
                    break;
                case "registrybased":
                    if (!string.IsNullOrEmpty(config))
                    {
                        CodeCrib.AX.Config.Server.SaveConfigToRegistry(aosnumber, ServerConfiguration, config);
                        this.WriteVerbose(string.Format("Configuration written to registry configuration {0} for AOS {1}", config, aosnumber));
                    }
                    else
                    {
                        CodeCrib.AX.Config.Server.SaveConfigToRegistry(aosnumber, ServerConfiguration, ServerConfiguration.Configuration);
                        this.WriteVerbose(string.Format("Configuration written to registry configuration {0} for AOS {1}", ServerConfiguration.Configuration, aosnumber));
                    }
                    break;
                case "registrybasedname":
                    if (!string.IsNullOrEmpty(config))
                    {
                        CodeCrib.AX.Config.Server.SaveConfigToRegistry(aosname, ServerConfiguration, config);
                        this.WriteVerbose(string.Format("Configuration written to registry configuration {0} for AOS {1}", config, aosname));
                    }
                    else
                    {
                        CodeCrib.AX.Config.Server.SaveConfigToRegistry(aosname, ServerConfiguration, ServerConfiguration.Configuration);
                        this.WriteVerbose(string.Format("Configuration written to registry configuration {0} for AOS {1}", ServerConfiguration.Configuration, aosname));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Sets the default configuration in the registry
    /// This will require administrator privileges
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "ServerConfiguration")]
    public class Set_ServerConfiguration : PSCmdlet
    {
        [Parameter(HelpMessage = "Name of a configuration in registry to make default", Mandatory = true, ParameterSetName = "aosid")]
        [Parameter(Mandatory = true, ParameterSetName = "aosname")]
        public string config;

        [Parameter(HelpMessage = "Identify AOS by its ID", Mandatory = true, ParameterSetName = "aosid")]
        public uint aosnumber;

        [Parameter(HelpMessage = "Identify AOS by its name", Mandatory = true, ParameterSetName = "aosname")]
        public string aosname;

        protected override void ProcessRecord()
        {
            switch (this.ParameterSetName)
            {
                case "aosid":
                    CodeCrib.AX.Config.Server.SetDefaultConfiguration(aosnumber, config);
                    this.WriteVerbose(string.Format("Default configuration for AOS {0} set to '{1}'", aosnumber, config));
                    break;
                case "aosname":
                    CodeCrib.AX.Config.Server.SetDefaultConfiguration(aosname, config);
                    this.WriteVerbose(string.Format("Default configuration for AOS {0} set to '{1}'", aosname, config));
                    break;
            }
        }
    }
}
