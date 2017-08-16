//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CodeCrib.AX.Client
{
    public class CompileOutputItem
    {
        public Int32 ElementType { get; set; }
        public string ElementName { get; set; }
        public string MethodName { get; set; }
        public string PropertyName { get; set; }
        public string TreeNodePath { get; set; }
        public Int32 LineNumber { get; set; }
        public Int32 ColumnNumber { get; set; }
        public Int16 Severity { get; set; }
        public string Message { get; set; }
        public Int32 ErrorCode { get; set; }
        public DateTime Date { get; set; }
    }

    public class CompileOutput
    {
        protected List<CompileOutputItem> output = new List<CompileOutputItem>();
        public List<CompileOutputItem> Output
        {
            get
            {
                return output;
            }
            set
            {
                if (value == null)
                    output = new List<CompileOutputItem>();
                else
                    output = value;
            }
        }

        protected XmlDocument ExtractXML(string filename)
        {
            string xml = "";

            bool xmlBlock = false;
            using (StreamReader streamReader = new StreamReader(File.OpenRead(filename)))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine().Trim();

                    if (line == "<AxaptaCompilerOutput>")
                        xmlBlock = true;

                    if (!xmlBlock)
                        continue;

                    xml = String.Format("{0}{1}{2}", xml, line, System.Environment.NewLine);

                    if (line == "</AxaptaCompilerOutput>")
                        xmlBlock = false;
                }
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc;
        }

        public void Parse(string filename)
        {
            output = new List<CompileOutputItem>();

            XmlDocument doc = ExtractXML(filename);

            XmlNodeList nodes;
            System.Collections.IEnumerator nodeEnumerator;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("Table", "urn:www.microsoft.com/Formats/Table");
            nodes = doc.SelectNodes("//Table:Record", nsmgr);
            if (nodes != null)
            {
                nodeEnumerator = nodes.GetEnumerator();
                while (nodeEnumerator.MoveNext())
                {
                    XmlNode node = nodeEnumerator.Current as XmlNode;

                    CompileOutputItem item = new CompileOutputItem();
                    item.Severity = Int16.Parse(node.SelectSingleNode("Table:Field[@name='SysCompilerSeverity']", nsmgr).InnerText);
                    item.TreeNodePath = node.SelectSingleNode("Table:Field[@name='TreeNodePath']", nsmgr).InnerText;
                    item.LineNumber = Int32.Parse(node.SelectSingleNode("Table:Field[@name='Line']", nsmgr).InnerText);
                    item.ColumnNumber = Int32.Parse(node.SelectSingleNode("Table:Field[@name='Column']", nsmgr).InnerText);
                    item.Message = node.SelectSingleNode("Table:Field[@name='SysCompileErrorMessage']", nsmgr).InnerText;
                    item.PropertyName = node.SelectSingleNode("Table:Field[@name='SysPropertyName']", nsmgr).InnerText;
                    item.MethodName = node.SelectSingleNode("Table:Field[@name='SysAotMethodName']", nsmgr).InnerText;
                    item.ErrorCode = Int32.Parse(node.SelectSingleNode("Table:Field[@name='CompileErrorCode']", nsmgr).InnerText);
                    // UtilElementType not present in R2 CU7 log.
                    XmlNode typeNode = node.SelectSingleNode("Table:Field[@name='UtilElementType']", nsmgr);
                    if (typeNode != null)
                    {
                        item.ElementType = Int32.Parse(typeNode.InnerText);
                    }
                    item.ElementName = node.SelectSingleNode("Table:Field[@name='SysUtilElementName']", nsmgr).InnerText;
                    item.Date = DateTime.Parse(node.SelectSingleNode("Table:Field[@name='createdDateTime']", nsmgr).InnerText);

                    output.Add(item);
                }
            }
        }

        public static CompileOutput CreateFromFile(string filename)
        {
            CompileOutput output = new CompileOutput();
            output.Parse(filename);

            return output;
        }
    }
}
