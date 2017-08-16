using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeCrib.AX.Client
{    
    public class XPOStaticAnalysis
    {        
        
        public Dictionary<string, List<string>> DuplicateOriginDictionary { get; protected set; }
        public Dictionary<string, int> TemporaryLabelDictionary { get; protected set; }
        public void analyzeXPO(string filename)
        {
            using (TextReader reader = File.OpenText(filename))
            {
                string currentLine;
                Dictionary<string, string> originDictionary = new Dictionary<string, string>();
                TemporaryLabelDictionary = new Dictionary<string, int>();
                DuplicateOriginDictionary = new Dictionary<string, List<string>>();

                while ((currentLine = reader.ReadLine()) != null)
                {
                    originId = "";
                    this.processXpoLine(currentLine);
                    if (!originId.Equals(""))
                    {
                        if (originDictionary.ContainsKey(originId))
                        {
                            List<string> objectNames = new List<string>();
                            string name;
                            originDictionary.TryGetValue(originId, out name);
                            objectNames.Add(name);
                            objectNames.Add(this.createObjectNameWithPath());
                            DuplicateOriginDictionary.Add(originId, objectNames);
                        }
                        else
                        {
                            originDictionary.Add(originId, this.createObjectNameWithPath());
                        }
                    }
                }
            }
        }

        protected string createObjectNameWithPath()
        {
            string path = xpoParser.objectCodeToAotPath(objectTypeCode);
            string name = "";
            if (!path.Equals(""))
                name = String.Format(@"{0}\{1}", path, objectName);
            else
                name = objectName;
            if (!subObjectName.Equals(""))
            {
                name = String.Format("{0}\\{1}", name, subObjectName);
            }

            return name;
        }
        protected string objectName = "";
        protected bool withinProperties = false;
        protected string objectTypeCode = "";
        protected string originId = "";
        protected XpoParser xpoParser = new XpoParser();
        protected string subObjectName = "";

        public void processXpoLine(string line)
        {
            if (xpoParser.isElementTypeLine(line))
            {
                objectTypeCode = xpoParser.getElementType(line);
                objectName = "";
                subObjectName = "";
            }

            if (xpoParser.isPropertiesStartLine(line))
                withinProperties = true;
            else if (xpoParser.isPropertiesEndLine(line))
                withinProperties = false;

            if (objectName.Equals("") && objectTypeCode.Equals(XpoParser.ObjectType_Table) && xpoParser.isTableDefinitionLine(line))
            {
                objectName = xpoParser.getTableDefinitionName(line);
            }
            if (objectTypeCode.Equals(XpoParser.ObjectType_Table) && xpoParser.isFieldDefinitionLine(line))
            {
                subObjectName = String.Format("Fields\\{0}", xpoParser.getFieldDefinitionName(line));
            }
            if ((objectTypeCode.Equals(XpoParser.ObjectType_Table) || objectTypeCode.Equals(XpoParser.ObjectType_Class)) && xpoParser.isMethodDefinitionLine(line))
            {
                subObjectName = String.Format("Methods\\{0}", xpoParser.getMethodDefinitionName(line));
            }
            if (objectName.Equals(""))
            {                
                if (withinProperties && xpoParser.getPropertyLineName(line).ToLower().Equals("name"))
                {
                    objectName = xpoParser.getPropertyLineValue(line);
                }
            }

            if (withinProperties)
                originId = xpoParser.getOriginIdValue(line);

            if (xpoParser.doesLineContainTempLabel(line))
            {
                int tempLabelCount;
                string key = this.createObjectNameWithPath();
                if (TemporaryLabelDictionary.TryGetValue(key, out tempLabelCount) == false)
                {
                    tempLabelCount = 1;
                    TemporaryLabelDictionary.Add(key, tempLabelCount);
                }
                else
                {
                    TemporaryLabelDictionary[key]++;
                }                                
            }
        }
    }    
}
