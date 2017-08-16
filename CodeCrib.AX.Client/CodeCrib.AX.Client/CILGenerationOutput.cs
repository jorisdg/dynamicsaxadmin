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
    public class CILGenerationOutputItem
    {
        public string ElementName { get; set; }
        public string MethodName { get; set; }
        public Int32 LineNumber { get; set; }
        public Int16 Severity { get; set; }
        public string Message { get; set; }
    }

    public class CILGenerationOutput
    {
        string[] knownErrorExpressions =
            { 
                @"^Warning: (?<message>CIL could not be generated for X\+\+ method (?<object>[A-Za-z_]\w*)\.(?<method>[A-Za-z_]\w*) due to X\+\+ compile errors\. This method throws an exception if run as CIL\.)$",
                @"^(?<message>Error when loading the method.) Type: (?<object>[A-Za-z_]\w*), (?<method>[A-Za-z_]\w*)$",
                @"^Warning: (?<message>A breakpoint statement was encountered during CIL generation of (?<object>[A-Za-z_]\w*)\.(?<method>[A-Za-z_]\w*)\.)$",
                @"^(?<message>The CIL generator found errors and could not save the new assembly\.)$"
            };
        string[] genericErrorExpressions =
            { 
                @"^Error: (?<message>.* method (?<object>[A-Za-z_]\w*)\.(?<method>[A-Za-z_]\w*).*)$"
                ,@"^Error: (?<message>.*)ClassName: (?<object>[A-Za-z_]\w*), MethodName: (?<method>[A-Za-z_]\w*).*$"
                ,@"^Error .* Class: (?<object>[A-Za-z_]\w*), Method: (?<method>[A-Za-z_]\w*), Exception: ([A-Za-z_][\w.]*): Line Number (?<linenum>\d+) - (?<message>.*)$"
            };

        string[] knownWarningExpressions =
            { 
            };
        string[] genericWarningExpressions =
            { 
                @"^Warning:(?<message>.* method (?<object>[A-Za-z_]\w*)\.(?<method>[A-Za-z_]\w*).*)$"
                ,@"^Warning:(?<message>.*)ClassName: (?<object>[A-Za-z_]\w*), MethodName: (?<method>[A-Za-z_]\w*).*$"
            };


        protected List<CILGenerationOutputItem> output = new List<CILGenerationOutputItem>();
        public List<CILGenerationOutputItem> Output
        {
            get
            {
                return output;
            }
            set
            {
                if (value == null)
                    output = new List<CILGenerationOutputItem>();
                else
                    output = value;
            }
        }

        protected void AddMatchedLine(System.Text.RegularExpressions.Match match, Int16 severity)
        {
            if (match.Success)
            {
                CILGenerationOutputItem item = new CILGenerationOutputItem() { Severity = severity };

                if (!string.IsNullOrEmpty(match.Groups["message"].Value))
                    item.Message = match.Groups["message"].Value;
                else
                    item.Message = match.Groups[0].Value;

                item.ElementName = match.Groups["object"].Value;

                item.MethodName = match.Groups["method"].Value;

                if (!string.IsNullOrEmpty(match.Groups["linenum"].Value))
                    item.LineNumber = Int32.Parse(match.Groups["linenum"].Value);

                output.Add(item);
            }
        }

        protected bool TestLine(string line, string[] expressions, Int16 severity)
        {
            bool matchedExpression = false;

            foreach (var expression in expressions)
            {
                var match = System.Text.RegularExpressions.Regex.Match(line, expression);

                if (match.Success)
                {
                    AddMatchedLine(match, severity);
                    matchedExpression = true;
                    break;
                }
            }

            return matchedExpression;
        }

        public void Parse(string filename)
        {
            output = new List<CILGenerationOutputItem>();

            using (StreamReader fileReader = new StreamReader(filename))
            {
                while (!fileReader.EndOfStream)
                {
                    String line = fileReader.ReadLine();

                    if (!TestLine(line, knownErrorExpressions, 0)) // Check for known errors
                        if (!TestLine(line, knownWarningExpressions, 1)) // Check for known warnings
                            if (!TestLine(line, genericErrorExpressions, 0)) // Try generic errors
                                if (!TestLine(line, genericWarningExpressions, 1)) // Try generic warnings
                                {
                                    // Otherwise add line as "other"
                                    CILGenerationOutputItem item = new CILGenerationOutputItem()
                                        {
                                            Severity = 4,
                                            Message = line
                                        };

                                    output.Add(item);
                                }
                }
            }
        }

        public static CILGenerationOutput CreateFromFile(string filename)
        {
            CILGenerationOutput output = new CILGenerationOutput();
            output.Parse(filename);

            return output;
        }
    }
}
