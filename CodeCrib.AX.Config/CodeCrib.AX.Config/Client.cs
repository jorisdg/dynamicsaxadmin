//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace CodeCrib.AX.Config
{
    public class AOSConnection
    {
        public string ServerName
        {
            get;
            set;
        }
        public string InstanceName
        {
            get;
            set;
        }
        public uint TCPIPPort
        {
            get;
            set;
        }
        public uint WSDLPort
        {
            get;
            set;
        }
    }

    [Serializable]
    public class Client : ConfigBase
    {
        public const string UserConfigRegistryKey = @"Software\Microsoft\Dynamics\6.0\Configuration";
        public const string BCConfigRegistryKey = @"SOFTWARE\Microsoft\Dynamics\6.0\Configuration";
        protected static RegistryKey GetConfigurationRegistryKey(bool bc, bool writable = false)
        {
            RegistryKey configKey = null;

            if (bc)
            {
                configKey = Registry.LocalMachine.OpenSubKey(BCConfigRegistryKey, writable);
            }
            else
            {
                configKey = Registry.CurrentUser.OpenSubKey(UserConfigRegistryKey, writable);
            }

            return configKey;
        }

        public Client()
            : base()
        {
        }

        #region ExposedProperties

        [XmlIgnore]
        public List<AOSConnection> Connections
        {
            get
            {
                List<AOSConnection> connections = new List<AOSConnection>();

                var aosList = this.GetPropertyValueList("aos2");
                var wsdlPort = this.GetPropertyValueList("wsdlport");

                for(int i=0; i<aosList.Count; i++)
                {
                    AOSConnection connection = new AOSConnection();

                    Match match = Regex.Match(aosList[i], @"((?<Instance>[^@]*)@)?(?<Server>[^:]+)(:(?<Port>\d*))?");
                    if (match.Success)
                    {
                        connection.ServerName = match.Groups["Server"].Value;
                        connection.TCPIPPort = 2712;
                        connection.WSDLPort = 8101;

                        if (match.Groups["Instance"].Success && !string.IsNullOrEmpty(match.Groups["Instance"].Value))
                        {
                            connection.InstanceName = match.Groups["Instance"].Value;
                        }
                        if (match.Groups["Port"].Success && !string.IsNullOrEmpty(match.Groups["Port"].Value))
                        {
                            connection.TCPIPPort = UInt16.Parse(match.Groups["Port"].Value);
                        }

                        if (wsdlPort.Count > i && !string.IsNullOrEmpty(wsdlPort[i]))
                        {
                            connection.WSDLPort = UInt16.Parse(wsdlPort[i]);
                        }

                        connections.Add(connection);
                    }
                }

                return connections;
            }
            set
            {
                var aosList = from c in value select string.Format("{0}@{1}:{2}", c.InstanceName, c.ServerName, c.TCPIPPort);
                this.SetOrCreatePropertyList("aos2", "Text", aosList.ToList());

                var wsdlList = from c in value select c.WSDLPort.ToString();
                this.SetOrCreatePropertyList("wsdlport", "Text", wsdlList.ToList());
            }
        }

        [XmlIgnore]
        public string BinaryDirectory
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
        public string LogDirectory
        {
            get
            {
                return this.GetPropertyValue("logdir");
            }
            set
            {
                this.SetOrCreateProperty("logdir", "Text", value);
            }
        }

        [XmlIgnore]
        public string Layer
        {
            get
            {
                return this.GetPropertyValue("aol");
            }
            set
            {
                this.SetOrCreateProperty("aol", "Text", value);
            }
        }

        [XmlIgnore]
        public string LayerCode
        {
            get
            {
                return this.GetPropertyValue("aolcode");
            }
            set
            {
                this.SetOrCreateProperty("aolcode", "Text", value);
            }
        }

        [XmlIgnore]
        public string Company
        {
            get
            {
                return this.GetPropertyValue("company");
            }
            set
            {
                this.SetOrCreateProperty("company", "Text", value);
            }
        }

        [XmlIgnore]
        public string Partition
        {
            get
            {
                return this.GetPropertyValue("partition");
            }
            set
            {
                this.SetOrCreateProperty("partition", "Text", value);
            }
        }

        [XmlIgnore]
        public string StartupCommand
        {
            get
            {
                return this.GetPropertyValue("startupcmd");
            }
            set
            {
                this.SetOrCreateProperty("startupcmd", "Text", value);
            }
        }

        [XmlIgnore]
        public string KernelStartupCommand
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
        public string StartupMessage
        {
            get
            {
                return this.GetPropertyValue("startupmsg");
            }
            set
            {
                this.SetOrCreateProperty("startupmsg", "Text", value);
            }
        }

        [XmlIgnore]
        public int ConnectToServerPrinters
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("useserverprinters"));
            }
            set
            {
                this.SetOrCreateProperty("useserverprinters", "Int", value.ToString());
            }
        }

        [XmlIgnore]
        public int EncryptClientServerCommunication
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("aosencryption"));
            }
            set
            {
                // Although this property is listed as "Text", it behaves 1/0 like an int
                this.SetOrCreateProperty("aosencryption", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public int UserBreakPoints
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("xppdebug"));
            }
            set
            {
                // Although this property is listed as "Text", it behaves 1/0 like an int
                this.SetOrCreateProperty("xppdebug", "Text", value.ToString());
            }
        }

        [XmlIgnore]
        public int GlobalBreakPoints
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
        public int PerformanceCacheSettings
        {
            get
            {
                return UInt16.Parse(this.GetPropertyValue("performanceProfile"));
            }
            set
            {
                // Although this property is listed as "Text", it behaves 1/0 like an int
                // AX config tool removes this line when it's zero, but doesn't complain about a "0" in the file
                this.SetOrCreateProperty("performanceProfile", "Text", value.ToString());
            }
        }

        #endregion


        /// <summary>
        /// Get a client configuration object from a configuration file
        /// </summary>
        /// <param name="filename">Path and filename of configuration file to load</param>
        /// <returns>Client configuration object</returns>
        public static Client GetConfigFromFile(string filename)
        {
            Client clientConfig = new Client();

            ConfigBase.GetConfigFromFile(clientConfig, filename);

            return clientConfig;
        }

        /// <summary>
        /// Get a list of configuration from the registry
        /// </summary>
        /// <param name="bc">True will fetch configurations from the business connector (requires administrator elevation), otherwise fetches from current user</param>
        /// <returns>List of configuration names</returns>
        public static List<string> GetConfigNameListFromRegistry(bool bc = false)
        {
            RegistryKey configKey = Client.GetConfigurationRegistryKey(bc);

            return ConfigBase.GetConfigNameListFromRegistry(configKey);
        }

        /// <summary>
        /// Gets the active configuration from the registry
        /// </summary>
        /// <param name="bc">True will fetch configuration from the business connector (requires administrator elevation), otherwise fetch from current user</param>
        /// <returns>Client configuration object</returns>
        public static Client GetConfigFromRegistry(bool bc = false)
        {
            RegistryKey configKey = Client.GetConfigurationRegistryKey(bc);
            return GetConfigFromRegistry(configKey.GetValue("Current").ToString(), bc);
        }

        /// <summary>
        /// Gets a configuration from the registry
        /// </summary>
        /// <param name="configurationName">Name of the configuration fo fetch</param>
        /// <param name="bc">True will fetch configuration from the business connector (requires administrator elevation), otherwise fetch from current user</param>
        /// <returns>Client configuration object</returns>
        public static Client GetConfigFromRegistry(string configurationName, bool bc = false)
        {
            RegistryKey configKey = Client.GetConfigurationRegistryKey(bc);
            configKey = configKey.OpenSubKey(configurationName);

            Client clientConfig = new Client() { FormatVersion = "1", Configuration = configurationName };

            ConfigBase.GetConfigFromRegistry(clientConfig, configKey);

            return clientConfig;
        }

        /// <summary>
        /// Save a client configuration object to a configuration file
        /// </summary>
        /// <param name="clientconfig">Client configuration object to save</param>
        /// <param name="filename">Path and filename of the configuration file, overwrites if it already exists</param>
        public static void SaveConfigToFile(Client clientconfig, string filename)
        {
            ConfigBase.SaveConfigToFile(clientconfig, filename);
        }

        /// <summary>
        /// Save a client configuration object to the registry in the configuration specified in the "Configuration" property of the client object
        /// If the configuration doesn't exist yet, it will be created
        /// </summary>
        /// <param name="clientConfig">Client configuration object to save</param>
        /// <param name="bc">True will save configuration to the business connector (requires administrator elevation), otherwise saves to current user</param>
        public static void SaveConfigToRegistry(Client clientConfig, bool bc = false)
        {
            SaveConfigToRegistry(clientConfig, clientConfig.Configuration, bc);
        }

        /// <summary>
        /// Save a client configuration object to the registry in the configuration specified
        /// If the configuration doesn't exist yet, it will be created
        /// </summary>
        /// <param name="clientConfig">Client configuration object to save</param>
        /// <param name="configurationName">Name of the configuration to save it to</param>
        /// <param name="bc">True will save configuration to the business connector (requires administrator elevation), otherwise saves to current user</param>
        public static void SaveConfigToRegistry(Client clientConfig, string configurationName, bool bc = false)
        {
            RegistryKey configKey = Client.GetConfigurationRegistryKey(bc, true);

            ConfigBase.SaveConfigToRegistry(clientConfig, configurationName, configKey);

            configKey.Close();
        }

        /// <summary>
        /// Sets the configuration specified as the active configuration
        /// </summary>
        /// <param name="configurationName">Name of the configuration to make active</param>
        /// <param name="bc">True will make the configuration active for the business connector (requires administrator elevation), otherwise for the current user</param>
        public static void SetDefaultConfiguration(string configurationName, bool bc = false)
        {
            RegistryKey configKey = Client.GetConfigurationRegistryKey(bc, true);

            ConfigBase.SetDefaultConfiguration(configurationName, configKey);
        }
    }
}
