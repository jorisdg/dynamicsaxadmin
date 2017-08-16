using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeCrib.AX.Client
{
    public class XpoParser
    {
        public const string ObjectType_Table = "DBT";
        public const string ObjectType_Class = "CLS";
        protected Regex _originIdRegex;
        public Regex OriginIdRegex
        {
            get
            {
                if (_originIdRegex == null)
                    _originIdRegex = new Regex(@"^\s*origin\s+#(\{\w+-.+\})\s*$", RegexOptions.IgnoreCase);
                return _originIdRegex;
            }
            protected set { _originIdRegex = value; }
        }

        protected Regex _elementTypeRegex;
        public Regex ElementTypeRegex
        {
            get
            {
                if (_elementTypeRegex == null)
                {
                    _elementTypeRegex = new Regex(@"^\*\*\*element\:\s+(\w\w\w)", RegexOptions.IgnoreCase);
                }
                    return _elementTypeRegex;
            }
            protected set { _elementTypeRegex = value; }
        }
        protected Regex _tempLabelRegex;
        public Regex TempLabelRegex
        {
            get
            {
                if (_tempLabelRegex == null)
                {
                    _tempLabelRegex = new Regex(@"@\$[A-Z]{2}\d+", RegexOptions.IgnoreCase);
                }
                return _tempLabelRegex;
            }
            set { _tempLabelRegex = value; }
        }
        public Boolean doesLineContainTempLabel(string line)
        {
            Match match = TempLabelRegex.Match(line);

            return match.Success;
        }
        public Boolean isOriginIdLine(string line)
        {
            string searchText = line.ToLower().TrimStart().TrimEnd();
            if (!this.getPropertyLineName(searchText).Equals("origin"))
                return false;

            Match match = OriginIdRegex.Match(searchText);
            if (match.Success)
            {
                return true;
            }

            return false;
        }
        public string getOriginIdValue(string line)
        {
            if (!isOriginIdLine(line))
                return "";

            string searchText = line.ToLower().TrimStart().TrimEnd();
            Match match = OriginIdRegex.Match(searchText);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            return "";
        }

        public string getElementType(string line)
        {
            string searchText = line.ToLower().TrimStart().TrimEnd();
            Match elementTypeMatch = ElementTypeRegex.Match(searchText);

            if (elementTypeMatch.Success)
                return elementTypeMatch.Groups[1].Value.ToUpper();

            return "";
        }
        public string getPropertyLineName(string line)
        {
            if (line.Contains("#"))
            {
                return line.Split('#')[0].Trim();
            }
            return "";
        }

        internal bool isTableDefinitionLine(string line)
        {
            bool isTableDefinitionLine = (line.Trim().ToLower().StartsWith("table #"));
            return isTableDefinitionLine;
        }

        internal bool isFieldDefinitionLine(string line)
        {
            bool isDefinitionLine = (line.Trim().ToLower().StartsWith("field #"));
            return isDefinitionLine;
        }

        internal string getFieldDefinitionName(string line)
        {
            if (this.isFieldDefinitionLine(line))
            {
                return line.Split('#')[1].Trim();
            }
            return "";
        }

        internal object getMethodDefinitionName(string line)
        {
            if (this.isMethodDefinitionLine(line))
            {
                return line.Split('#')[1].Trim();
            }
            return "";
        }

        internal bool isMethodDefinitionLine(string line)
        {
            bool isDefinitionLine = (line.Trim().ToLower().StartsWith("source #"));
            return isDefinitionLine;
        }

        internal string getTableDefinitionName(string line)
        {
            if (this.isTableDefinitionLine(line))
            {
                return line.Split('#')[1].Trim();
            }
            return "";
        }

        public string getPropertyLineValue(string line)
        {
            if (line.Contains("#"))
            {
                return line.Split('#')[1];
            }
            return "";
        }
        public Boolean isElementTypeLine(string line)
        {
            string searchText = line.ToLower().TrimStart().TrimEnd();
            return searchText.StartsWith("***element");
        }
        public Boolean isPropertiesEndLine(string line)
        {
            string searchText = line.ToLower().TrimStart().TrimEnd();
            return (searchText.Equals("endproperties"));
        }
        public Boolean isPropertiesStartLine(string line)
        {
            string searchText = line.ToLower().TrimStart().TrimEnd();

            return (searchText.Equals("properties"));
        }

        public string objectCodeToAotPath(string objectCode)
        {
            switch (objectCode.ToUpper())
            {
                case ObjectType_Class: return "Classes";
                case "DBE": return "Data dictionary\\Base enums";
                case "UTS": // string
                case "UTR": // real
                case "UTE": // enum
                case "UTW": // int64
                case "UTI": // integer
                case "UTD": // date
                case "UTT": // time
                    return "Data dictionary\\Extended data types";

                case "MAP": return "Data dictionary\\Maps";
                case ObjectType_Table: return "Data dictionary\\Tables";
                case "VIE": return "Data dictionary\\Views";
                case "FRM": return "Forms";
                case "JOB": return "Jobs";
                case "FTM": return "Menu items";
                case "MNU": return "Menus";
                case "FPA": return "Parts\\Form parts";

                case "IPA": return "Parts\\Info part";
                case "PRN": return "Projects";
                case "QUE": return "Queries";
                case "SDT": return "Security\\Duties";
                case "SPV": return "Security\\Privileges";
                case "SRP": return "SSRS Reports";
                    // END: End(of file)                
            }
            return "";
        }
    }
}
