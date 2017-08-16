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
using System.Xml.Serialization;

namespace CodeCrib.AX.Client
{
    namespace AutoRun
    {
        public enum RunType
        {
            @class,
            @job,
            @actionMenuItem,
            @displayMenuItem,
            @outputMenuItem
        }

        [Serializable]
        public class AxaptaAutoRun
        {
            [XmlAttribute("exitWhenDone")]
            public bool ExitWhenDone { get; set; }

            [XmlAttribute("version")]
            public string Version { get; set; }

            [XmlAttribute("logFile")]
            public string LogFile { get; set; }

            [XmlIgnore]
            public List<KeyValuePair<string, string>> Log { get; set; }
            [XmlIgnore]
            public string InfoLog { get; set; }

            private List<AutoRunElement> steps = new List<AutoRunElement>();
            [
            XmlElement("Run", typeof(Run)),
            XmlElement("CompileApplication", typeof(CompileApplication)),
            XmlElement("XpoImport", typeof(XpoImport)),
            XmlElement("Synchronize", typeof(Synchronize)),
            XmlElement("CompileIL", typeof(CompileIL))
            ]
            public List<AutoRunElement> Steps { get { return steps; } set { steps = value; } }

            public void ParseLog()
            {
                System.Xml.XmlDocument log = new System.Xml.XmlDocument();
                log.Load(LogFile);

                this.Log = new List<KeyValuePair<string, string>>();
                var entries = log.SelectNodes("/AxaptaAutoRun/Info | /AxaptaAutoRun/Error | /AxaptaAutoRun/Warning");
                foreach (System.Xml.XmlNode node in entries)
                {
                    this.Log.Add(new KeyValuePair<string, string>(node.Name, node.InnerText));
                }
                var infolog = log.SelectSingleNode("/AxaptaAutoRun/Infolog");
                if (infolog != null)
                    this.InfoLog = infolog.InnerText;

                Dictionary<string, int> stepTypeCount = new Dictionary<string, int>();
                foreach (AutoRunElement step in steps)
                {
                    string typeName = step.GetType().Name;

                    int occurrence = 0;
                    if (stepTypeCount.ContainsKey(typeName))
                    {
                        occurrence = ++stepTypeCount[typeName];
                    }
                    else
                    {
                        stepTypeCount.Add(typeName, 1);
                        occurrence = 1;
                    }
                    
                    var xPath = string.Format("/AxaptaAutoRun/{0}[{1}]/Info | /AxaptaAutoRun/{0}[{1}]/Error | /AxaptaAutoRun/{0}[{1}]/Warning", typeName, occurrence);

                    entries = log.SelectNodes(xPath);

                    step.Log = new List<KeyValuePair<string, string>>();
                    foreach (System.Xml.XmlNode node in entries)
                    {
                        step.Log.Add(new KeyValuePair<string, string>(node.Name, node.InnerText));
                    }
                }
            }

            public static AxaptaAutoRun DeserializeAutoRun(string filename)
            {
                AxaptaAutoRun autoRun = null;

                XmlSerializer serializer = new XmlSerializer(typeof(AxaptaAutoRun));

                StreamReader reader = new StreamReader(filename);
                autoRun = (AxaptaAutoRun)serializer.Deserialize(reader);
                reader.Close();

                return autoRun;
            }

            public static void SerializeAutoRun(AxaptaAutoRun autoRun, string filename)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(AxaptaAutoRun));
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                // Set to blank so no namespace is used in output XML
                ns.Add(String.Empty, String.Empty);
                TextWriter writer = new StreamWriter(filename);
                serializer.Serialize(writer, autoRun, ns);
                writer.Close();
            }

            public static AxaptaAutoRun FindOrCreate(string filename)
            {
                AxaptaAutoRun autoRun = null;

                if (System.IO.File.Exists(filename))
                {
                    autoRun = AxaptaAutoRun.DeserializeAutoRun(filename);
                }
                else
                {
                    autoRun = new AxaptaAutoRun();
                }

                return autoRun;
            }
        }

        public abstract class AutoRunElement
        {
            [XmlIgnore]
            public List<KeyValuePair<string, string>> Log { get; set; }
        }

        [Serializable]
        public class Run : AutoRunElement
        {
            [XmlAttribute("type")]
            public RunType Type { get; set; }

            [XmlAttribute("name")]
            public string Name { get; set; }

            [XmlAttribute("method")]
            public string Method { get; set; }

            [XmlAttribute("parameters")]
            public string Parameters { get; set; }
        }

        [Serializable]
        public class CompileApplication : AutoRunElement
        {
            [XmlAttribute("node")]
            public string Node { get; set; }

            [XmlAttribute("crossReference")]
            public bool UpdateCrossReference { get; set; }
        }

        [Serializable]
        public class XpoImport : AutoRunElement
        {
            [XmlAttribute("file")]
            public string File { get; set; }
        }

        [Serializable]
        public class Synchronize : AutoRunElement
        {
            //[XmlAttribute("syncTasks")]
            //public bool SyncTasks { get; set; }

            [XmlAttribute("syncRoles")]
            public bool SyncRoles { get; set; }

            [XmlAttribute("syncDb")]
            public bool SyncDB { get; set; }

            //[XmlAttribute("syncRuntimeModel")]
            //public bool SyncRunTimeModel { get; set; }
        }

        [Serializable]
        public class CompileIL : AutoRunElement
        {
            [XmlAttribute("incremental")]
            public bool Incremental { get; set; }

            [XmlAttribute("createPackage")]
            public bool CreatePackage { get; set; }
        }
    }
}
