//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeCrib.AX.Setup
{
    public class LogParser
    {
        /// <summary>
        /// Parse a setup log file and extract setup parameters
        /// </summary>
        /// <param name="filename">Path and filename of a Dynamics Ax Setup log file</param>
        /// <returns>List of parameters extracted from the log</returns>
        public static List<Parameter> LogExtract(string filename)
        {
            List<Parameter> parameters = new List<Parameter>();
            System.IO.StreamReader reader = new System.IO.StreamReader(filename);

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();

                string[] columns = line.Split('\t');
                if (columns.Count() > 1)
                {
                    var match = Regex.Match(columns[1], @"^Property ([\w\d]+) set to: '(.*)'$");
                    if (match.Success)
                    {
                        Parameter param = parameters.FirstOrDefault(x => x.Name == match.Groups[1].Value);
                        if (param != null)
                        {
                            param.Value = match.Groups[2].Value;
                        }
                        else
                        {
                            parameters.Add(new Parameter() { Name = match.Groups[1].Value, Value = match.Groups[2].Value, Enabled = true });
                        }
                    }
                }
            }

            return parameters;
        }
    }
}
