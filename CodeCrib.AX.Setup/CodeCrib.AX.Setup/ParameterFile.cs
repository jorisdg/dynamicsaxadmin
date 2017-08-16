//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCrib.AX.Setup
{
    public class ParameterFile
    {
        /// <summary>
        /// Save parameters to a valid Dynamics AX Setup parameter file
        /// </summary>
        /// <param name="parameters">List of parameters</param>
        /// <param name="fileName">Path and filename of parameters file; will overwrite if already exists</param>
        public static void Save(List<Parameter> parameters, string fileName)
        {
            System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName);

            foreach (Parameter parameter in parameters)
            {
                if (!parameter.Enabled)
                    writer.Write("#");

                writer.WriteLine(string.Format("{0}={1}", parameter.Name, parameter.Value));
            }

            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Read a Dynamics AX Setup parameters file
        /// </summary>
        /// <param name="fileName">Path and filename of parameter file to open</param>
        /// <returns>List of parameters from the file</returns>
        public static List<Parameter> Open(string fileName)
        {
            List<Parameter> parameters = new List<Parameter>();
            System.IO.StreamReader reader = new System.IO.StreamReader(fileName);

            while (!reader.EndOfStream)
            {
                bool commentedLine = false;
                string line = reader.ReadLine().Trim();

                commentedLine = line.StartsWith("#");
                // don't split after # if the string doesn't go beyond it, the column count won't equal 2 anyway
                if (commentedLine && line.Length >= line.IndexOf("#") + 1)
                {
                    line = line.Substring(line.IndexOf("#") + 1);
                }

                string[] columns = line.Split('=');
                if (columns.Count() == 2)
                {
                    Parameter param = parameters.FirstOrDefault(x => x.Name == columns[0]);
                    if (param != null)
                    {
                        // If line is commented, don't update parameter if it appeared before
                        if (!commentedLine)
                        {
                            param.Value = columns[1];
                        }
                    }
                    else
                    {
                        parameters.Add(new Parameter() { Name = columns[0].Trim(), Value = columns[1], Enabled = !commentedLine });
                    }
                }
                else
                {
                    //Console.WriteLine(string.Format("Couldn't parse line '{0}'", line));
                }
            }

            reader.Close();

            return parameters;
        }
    }
}
