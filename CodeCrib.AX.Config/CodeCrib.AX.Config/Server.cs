//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Xml.Serialization;

namespace CodeCrib.AX.Config
{
    [Serializable]
    public class Server : ConfigBase
    {
        protected const string aosRegistryPath = @"SYSTEM\CurrentControlSet\services\Dynamics Server\6.0";
        public static uint GetAOSNumber(string aosName)
        {
            RegistryKey configKey = Registry.LocalMachine.OpenSubKey(aosRegistryPath);
            uint aosNumber = 0;

            foreach(string aosNumberName in configKey.GetSubKeyNames())
            {
                RegistryKey aosInstance = configKey.OpenSubKey(aosNumberName);
                String instanceName = aosInstance.GetValue("InstanceName", null) as String;

                if (!String.IsNullOrEmpty(instanceName) && instanceName == aosName)
                {
                    aosNumber = UInt16.Parse(aosNumberName);
                }
            }

            return aosNumber;
        }
        public static uint GetAOSNumber(uint port)
        {
            RegistryKey configKey = Registry.LocalMachine.OpenSubKey(aosRegistryPath);
            uint aosNumber = 0;

            foreach (string aosNumberName in configKey.GetSubKeyNames())
            {
                RegistryKey aosInstance = configKey.OpenSubKey(aosNumberName);
                String currentConfig = aosInstance.GetValue("Current", null) as String;

                if (!String.IsNullOrEmpty(currentConfig))
                {
                    RegistryKey config = aosInstance.OpenSubKey(currentConfig);
                    String currentPort = config.GetValue("port", null) as String;

                    if (UInt16.Parse(currentPort) == port)
                    {
                        aosNumber = UInt16.Parse(aosNumberName);
                    }
                }
            }

            return aosNumber;
        }
        protected static RegistryKey GetConfigurationRegistryKey(uint aosNumber, bool writable = false)
        {
            RegistryKey configKey = configKey = Registry.LocalMachine.OpenSubKey(string.Format(@"{0}\{1}", aosRegistryPath, aosNumber.ToString("D2")), writable);

            return configKey;
        }

        [XmlIgnore]
        public uint AOSNumberOrigin { get; set; }
        [XmlIgnore]
        public string AOSVersionOrigin { get; set; }
        [XmlIgnore]
        public string AOSNameOrigin { get; set; }


        public Server()
            : base()
        {
        }

        #region ExposedProperties
        [XmlIgnore]
        public string AlternateBinDirectory
        {
            get
            {
                return this.GetPropertyValue("bindir");
            }
            set
            {
                this.SetOrCreateProperty("bindir", "Text", value);
            }
        }

        [XmlIgnore]
        public string ConfigCommandAtKernelStartup
        {
            get
            {
                return this.GetPropertyValue("extracmdline");
            }
            set
            {
                this.SetOrCreateProperty("extracmdline", "Text", value);
            }
        }

        [XmlIgnore]
        public uint TCPIPPort
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("port"));
            }
            set
            {
                this.SetOrCreateProperty("port", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public uint WSDLPort
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("WSDLPort"));
            }
        }

        [XmlIgnore]
        public uint AllowServerPrinters
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("exposeserverprinters"));
            }
            set
            {
                this.SetOrCreateProperty("exposeserverprinters", "Int", value.ToString());
            }
        }

        [XmlIgnore]
        public uint BreakpointsOnServer
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("xppdebug"));
            }
            set
            {
                this.SetOrCreateProperty("xppdebug", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public uint GlobalBreakpoints
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("globalbreakpoints"));
            }
            set
            {
                this.SetOrCreateProperty("globalbreakpoints", "Int", value.ToString());
            }
        }

        [XmlIgnore]
        public uint HotSwapping
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("hotswapenabled"));
            }
            set
            {
                this.SetOrCreateProperty("hotswapenabled", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public string DatabaseServer
        {
            get
            {
                return this.GetPropertyValue("dbserver");
            }
            set
            {
                this.SetOrCreateProperty("dbserver", "Text", value);
                this.SetOrCreateProperty("ModelDBServer", "Text", value);
            }
        }

        [XmlIgnore]
        public string Database
        {
            get
            {
                return this.GetPropertyValue("database");
            }
            set
            {
                this.SetOrCreateProperty("database", "Text", value);
                switch (AOSVersionOrigin.Substring(0, 3))
                {
                    case "6.2":
                        this.SetOrCreateProperty("ModelDatabase", "Text", string.Format("{0}_model", value));
                        break;
                    case "6.1":
                    case "6.0":
                    default:
                        this.SetOrCreateProperty("ModelDatabase", "Text", value);
                        break;
                }
            }
        }

        [XmlIgnore]
        public byte StatementCache
        {
            get
            {
                return Byte.Parse(this.GetPropertyValue("opencursors"));
            }
            set
            {
                this.SetOrCreateProperty("opencursors", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public uint MaxBufferSize
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("sqlbuffer"));
            }
            set
            {
                this.SetOrCreateProperty("sqlbuffer", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public string ODBCLog
        {
            get
            {
                return this.GetPropertyValue("log");
            }
            set
            {
                this.SetOrCreateProperty("log", "Text", value);
            }
        }

        [XmlIgnore]
        public uint OrderByFromWhere
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("ignoredatasourceindex"));
            }
            set
            {
                this.SetOrCreateProperty("ignoredatasourceindex", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public uint NumberOfConnectionRetries
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("newconnectionretrycount"));
            }
            set
            {
                this.SetOrCreateProperty("newconnectionretrycount", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public uint ConnectionRetryInterval
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("newconnectionretrydelayms"));
            }
            set
            {
                this.SetOrCreateProperty("newconnectionretrydelayms", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public bool LimitInactiveConnections
        {
            get
            {
                return (this.GetPropertyValue("connectionidletimeout") == "-1") ? false : true;
            }
            set
            {
                this.SetOrCreateProperty("connectionidletimeout", "Text", value ? "" : "-1");
            }
        }

        [XmlIgnore]
        public uint MaxInactiveConnections
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("connectionidletimeout"));
            }
            set
            {
                this.SetOrCreateProperty("connectionidletimeout", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public uint MinPacketSizeCompression
        {
            get
            {
                UInt16 oneKB = 1024;
                UInt16 size = UInt16.Parse(this.GetPropertyValue("compressionminsize"));
                return (uint)(size / oneKB);
            }
            set
            {
                this.SetOrCreateProperty("compressionminsize", "Text", (value * 1024).ToString());
            }
        }

        protected bool GetProcessorAffinity(uint cpu)
        {
            uint affinity = 0;

            if (this.Properties.Exists(x => x.Name == "processaffinitymask"))
            {
                affinity = UInt16.Parse(this.GetPropertyValue("processaffinitymask"));
            }

            cpu = (UInt16)Math.Pow(2, cpu);

            return (affinity & cpu) == cpu;
        }

        protected void SetProcessorAffinity(uint cpu, bool value)
        {
            uint affinity = 0;

            if (this.Properties.Exists(x => x.Name == "processaffinitymask"))
            {
                affinity = UInt16.Parse(this.GetPropertyValue("processaffinitymask"));
            }

            cpu = (UInt16)Math.Pow(2, cpu);

            // If new value is true but cpu is set to false
            if (value && (affinity & cpu) != cpu)
            {
                affinity += cpu;
            }
            // else if new value is false but cpu is set to true
            else if (!value && (affinity & cpu) == cpu)
            {
                affinity -= cpu;
            }

            base.SetOrCreateProperty("processaffinitymask", "Text", affinity.ToString());
        }

        [XmlIgnore]
        public bool ProcessorAffinityCPU0
        {
            get
            {
                return GetProcessorAffinity(0);
            }
            set
            {
                this.SetProcessorAffinity(0, value);
            }
        }

        [XmlIgnore]
        public bool ProcessorAffinityCPU1
        {
            get
            {
                return GetProcessorAffinity(1);
            }
            set
            {
                this.SetProcessorAffinity(1, value);
            }
        }

        [XmlIgnore]
        public bool ProcessorAffinityCPU2
        {
            get
            {
                return GetProcessorAffinity(2);
            }
            set
            {
                this.SetProcessorAffinity(2, value);
            }
        }

        [XmlIgnore]
        public bool ProcessorAffinityCPU3
        {
            get
            {
                return GetProcessorAffinity(3);
            }
            set
            {
                this.SetProcessorAffinity(3, value);
            }
        }

        #endregion

        /// <summary>
        /// Get a server configuration object from a configuration file
        /// </summary>
        /// <param name="filename">Path and filename of configuration file to load</param>
        /// <returns>Server configuration object</returns>
        public static Server GetConfigFromFile(string filename)
        {
            Server serverConfig = new Server();

            ConfigBase.GetConfigFromFile(serverConfig, filename);

            return serverConfig;
        }

        /// <summary>
        /// Get a list of AOS instance names
        /// </summary>
        /// <returns>List of AOS instance names</returns>
        public static List<string> GetAOSInstances()
        {
            RegistryKey configKey = configKey = Registry.LocalMachine.OpenSubKey(aosRegistryPath);
            List<string> instances = new List<string>();

            foreach (string aosNumberName in configKey.GetSubKeyNames())
            {
                RegistryKey aosInstance = configKey.OpenSubKey(aosNumberName);
                String instanceName = aosInstance.GetValue("InstanceName", null) as String;

                if (!String.IsNullOrEmpty(instanceName))
                {
                    instances.Add(instanceName);
                }
            }

            return instances;
        }

        /// <summary>
        /// Get a list of AOS IDs
        /// </summary>
        /// <returns>List of AOS instance IDs</returns>
        public static List<uint> GetAOSes()
        {
            RegistryKey configKey = configKey = Registry.LocalMachine.OpenSubKey(aosRegistryPath);
            List<uint> instances = new List<uint>();

            foreach (string aosNumberName in configKey.GetSubKeyNames())
            {
                instances.Add(UInt16.Parse(aosNumberName));
            }

            return instances;
        }

        /// <summary>
        /// Get a list of configuration from the registry
        /// </summary>
        /// <param name="aosName">Name of the AOS instance to fetch configurations from</param>
        /// <returns>List of configuration names</returns>
        public static List<string> GetConfigNameListFromRegistry(string aosName)
        {
            return Server.GetConfigNameListFromRegistry(Server.GetAOSNumber(aosName));
        }

        /// <summary>
        /// Get a list of configuration from the registry
        /// </summary>
        /// <param name="aosNumber">ID of the AOS instance to fetch configurations from</param>
        /// <returns>List of configuration names</returns>
        public static List<string> GetConfigNameListFromRegistry(uint aosNumber)
        {
            RegistryKey configKey = Server.GetConfigurationRegistryKey(aosNumber);

            return ConfigBase.GetConfigNameListFromRegistry(configKey);
        }

        /// <summary>
        /// Gets the active configuration from the registry
        /// </summary>
        /// <param name="aosName">Name of the AOS instance fo fetch configurations from</param>
        /// <returns>Server configuration object</returns>
        public static Server GetConfigFromRegistry(string aosName)
        {
            return Server.GetConfigFromRegistry(Server.GetAOSNumber(aosName));
        }

        /// <summary>
        /// Gets the active configuration from the registry
        /// </summary>
        /// <param name="aosNumber">ID of the AOS instance fo fetch configurations from</param>
        /// <returns>Server configuration object</returns>
        public static Server GetConfigFromRegistry(uint aosNumber)
        {
            RegistryKey configKey = Server.GetConfigurationRegistryKey(aosNumber);
            return GetConfigFromRegistry(aosNumber, configKey.GetValue("Current").ToString());
        }

        /// <summary>
        /// Gets a configuration from the registry
        /// </summary>
        /// /// <param name="aosName">Name of the AOS instance to fetch configuration from</param>
        /// <param name="configurationName">Name of the configuration fo fetch</param>
        /// <returns>Client configuration object</returns>
        public static Server GetConfigFromRegistry(string aosName, string configurationName)
        {
            return GetConfigFromRegistry(Server.GetAOSNumber(aosName), configurationName);
        }

        /// <summary>
        /// Gets a configuration from the registry
        /// </summary>
        /// <param name="aosNumber">ID of the AOS instance to fetch configuration from</param>
        /// <param name="configurationName">Name of the configuration fo fetch</param>
        /// <returns>Client configuration object</returns>
        public static Server GetConfigFromRegistry(uint aosNumber, string configurationName)
        {
            RegistryKey configKey = Server.GetConfigurationRegistryKey(aosNumber);
            string versionOrigin = (string)configKey.GetValue("ProductVersion");
            string nameOrigin = (string)configKey.GetValue("InstanceName");
            configKey = configKey.OpenSubKey(configurationName);

            Server serverConfig = new Server() { FormatVersion = "1", Configuration = configurationName, AOSNumberOrigin = aosNumber, AOSVersionOrigin = versionOrigin, AOSNameOrigin = nameOrigin };

            ConfigBase.GetConfigFromRegistry(serverConfig, configKey);

            return serverConfig;
        }

        /// <summary>
        /// Save a server configuration object to a configuration file
        /// </summary>
        /// <param name="serverconfig">Server configuration object to save</param>
        /// <param name="filename">Path and filename of the configuration file, overwrites if it already exists</param>
        public static void SaveConfigToFile(Server serverConfig, string filename)
        {
            ConfigBase.SaveConfigToFile(serverConfig, filename);
        }

        /// <summary>
        /// Save a server configuration object to the registry in the configuration specified in the "Configuration" property of the server object
        /// If the configuration doesn't exist yet, it will be created
        /// </summary>
        /// <param name="aosName">Name of the AOS instance to save the configuration to</param>
        /// <param name="serverConfig">Server configuration object to save</param>
        public static void SaveConfigToRegistry(string aosName, Server serverConfig)
        {
            SaveConfigToRegistry(Server.GetAOSNumber(aosName), serverConfig);
        }

        /// <summary>
        /// Save a server configuration object to the registry in the configuration specified in the "Configuration" property of the server object
        /// If the configuration doesn't exist yet, it will be created
        /// </summary>
        /// <param name="aosNumber">ID of the AOS instance to save the configuration to</param>
        /// <param name="serverConfig">Server configuration object to save</param>
        public static void SaveConfigToRegistry(uint aosNumber, Server serverConfig)
        {
            SaveConfigToRegistry(aosNumber, serverConfig, serverConfig.Configuration);
        }

        /// <summary>
        /// Save a server configuration object to the registry in the configuration specified in the "Configuration" property of the server object
        /// If the configuration doesn't exist yet, it will be created
        /// </summary>
        /// <param name="aosName">Name of the AOS instance to save the configuration to</param>
        /// <param name="serverConfig">Server configuration object to save</param>
        /// <param name="configurationName">Name of the configuration to save it to</param>
        public static void SaveConfigToRegistry(string aosName, Server serverConfig, string configurationName)
        {
            SaveConfigToRegistry(Server.GetAOSNumber(aosName), serverConfig, configurationName);
        }

        /// <summary>
        /// Save a server configuration object to the registry in the configuration specified in the "Configuration" property of the server object
        /// If the configuration doesn't exist yet, it will be created
        /// </summary>
        /// <param name="aosNumber">ID of the AOS instance to save the configuration to</param>
        /// <param name="serverConfig">Server configuration object to save</param>
        /// <param name="configurationName">Name of the configuration to save it to</param>
        public static void SaveConfigToRegistry(uint aosNumber, Server serverConfig, string configurationName)
        {
            RegistryKey configKey = Server.GetConfigurationRegistryKey(aosNumber, true);

            ConfigBase.SaveConfigToRegistry(serverConfig, configurationName, configKey);

            configKey.Close();
        }

        /// <summary>
        /// Sets the configuration specified as the active configuration
        /// </summary>
        /// <param name="aosName">Name of the AOS instance to make the configuration active for</param>
        /// <param name="configurationName">Name of the configuration to make active</param>
        public static void SetDefaultConfiguration(string aosName, string configurationName)
        {
            SetDefaultConfiguration(Server.GetAOSNumber(aosName), configurationName);
        }

        /// <summary>
        /// Sets the configuration specified as the active configuration
        /// </summary>
        /// <param name="aosName">ID of the AOS instance to make the configuration active for</param>
        /// <param name="configurationName">Name of the configuration to make active</param>
        public static void SetDefaultConfiguration(uint aosNumber, string configurationName)
        {
            RegistryKey configKey = Server.GetConfigurationRegistryKey(aosNumber, true);

            ConfigBase.SetDefaultConfiguration(configurationName, configKey);
        }
    }
}
