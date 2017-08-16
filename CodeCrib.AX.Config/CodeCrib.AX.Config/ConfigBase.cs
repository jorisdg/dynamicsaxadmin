using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Xml.Serialization;

namespace CodeCrib.AX.Config
{
    [Serializable]
    public class ConfigBase
    {
        public ConfigBase()
        {
            Properties = new List<ConfigProperty>();
            FormatVersion = "1";
        }

        public string FormatVersion
        {
            get;
            set;
        }

        public string Configuration
        {
            get;
            set;
        }

        public List<ConfigProperty> Properties
        {
            get;
            set;
        }

        public string GetPropertyValue(string name)
        {
            var propertyValue = (from p in Properties where p.Name.ToLower() == name.ToLower() select p.Value).FirstOrDefault();

            return propertyValue;
        }

        public void SetOrCreateProperty(string name, string dataType, string value)
        {
            var property = (from p in Properties where p.Name.ToLower() == name.ToLower() select p).FirstOrDefault();

            if (property == null)
            {
                property = new ConfigProperty();
                property.Name = name; // name.ToLower(); // some new properties have a capital somewhere, so don't assume ToLower()
                property.DataType = dataType.Substring(0, 1).ToUpper() + dataType.Substring(1).ToLower();

                Properties.Add(property);
            }

            property.Value = value;
        }

        public List<string> GetPropertyValueList(string name)
        {
            return this.GetPropertyValue(name).Split(';').ToList();
        }

        public void SetOrCreatePropertyList(string name, string dataType, List<string> values)
        {
            this.SetOrCreateProperty(name, dataType, string.Join(";", values.ToArray()));
        }

        protected static List<string> GetConfigNameListFromRegistry(RegistryKey configKey)
        {
            return configKey.GetSubKeyNames().ToList();
        }

        protected static void GetConfigFromFile(ConfigBase config, string filename)
        {
            using (StreamReader streamReader = new StreamReader(File.OpenRead(filename)))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine().Trim();

                    if (line.StartsWith("Formatversion:"))
                    {
                        config.FormatVersion = line.Substring(15);
                        continue;
                    }
                    if (line.StartsWith("Configuration:"))
                    {
                        config.Configuration = line.Substring(15);
                        continue;
                    }

                    if (line.IndexOf(',') < 0)
                        continue;

                    ConfigProperty property = new ConfigProperty();
                    property.Name = line.Substring(0, line.IndexOf(','));
                    line = line.Substring(line.IndexOf(',') + 1);
                    property.DataType = line.Substring(0, line.IndexOf(','));
                    line = line.Substring(line.IndexOf(',') + 1);
                    property.Value = line;

                    config.Properties.Add(property);
                }
            }
        }

        protected static void GetConfigFromRegistry(ConfigBase config, RegistryKey configKey)
        {
            foreach (string propertyName in configKey.GetValueNames())
            {
                ConfigProperty property = new ConfigProperty();

                property.Name = propertyName;

                switch (configKey.GetValueKind(propertyName))
                {
                    case RegistryValueKind.DWord:
                        property.DataType = "Int";
                        break;
                    case RegistryValueKind.String:
                        property.DataType = "Text";
                        break;
                    // Failsafe, make it Text
                    default:
                        property.DataType = "Text";
                        break;
                }

                property.Value = configKey.GetValue(propertyName).ToString();

                config.Properties.Add(property);
            }
        }

        protected static void SaveConfigToFile(ConfigBase config, string filename)
        {
            using (StreamWriter streamWriter = new StreamWriter(File.Open(filename, FileMode.Create), Encoding.Unicode))
            {
                streamWriter.WriteLine("Configuration export file for Dynamics");
                streamWriter.WriteLine(string.Format("Formatversion: {0}", config.FormatVersion));
                streamWriter.WriteLine(string.Format("Configuration: {0}", config.Configuration));

                foreach (ConfigProperty property in config.Properties)
                {
                    streamWriter.WriteLine(string.Format("    {0},{1},{2}", property.Name, property.DataType, property.Value));
                }

                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        protected static void SaveConfigToRegistry(ConfigBase config, RegistryKey configKey)
        {
            SaveConfigToRegistry(config, config.Configuration, configKey);
        }

        protected static void SaveConfigToRegistry(ConfigBase config, string configurationName, RegistryKey configKey)
        {
            configKey = configKey.CreateSubKey(configurationName);

            foreach (ConfigProperty property in config.Properties)
            {
                switch (property.DataType)
                {
                    case "Int":
                        configKey.SetValue(property.Name, UInt16.Parse(property.Value), RegistryValueKind.DWord);
                        break;
                    case "Text":
                        configKey.SetValue(property.Name, property.Value, RegistryValueKind.String);
                        break;
                    default:
                        configKey.SetValue(property.Name, property.Value, RegistryValueKind.String);
                        break;
                }
            }

            configKey.Close();
        }

        protected static void SetDefaultConfiguration(string configurationName, RegistryKey configKey)
        {
            var configs = configKey.GetSubKeyNames().ToList();

            if (configs.Exists(x => x == configurationName))
            {
                configKey.SetValue("Current", configurationName, RegistryValueKind.String);
            }
            else
            {
                throw new Exception(string.Format("Configuraton '{0}' does not exist", configurationName));
            }
        }
    }
}
