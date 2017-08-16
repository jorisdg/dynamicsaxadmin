//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;

namespace CodeCrib.AX.Client
{
    public class XPOLabelCheck
    {
        public static List<string> FindTempLabels(string folder, bool recursive)
        {
            XPOLabelCheck combine = new XPOLabelCheck();
            List<string> files = new List<string>();
            List<string> filesWithTempLabels = new List<string>();
            combine.GetFiles(folder, recursive, files);

            if (files.Count < 1)
            {
                throw new Exception(String.Format("No XPO files found in {0}", folder));
            }
            else
            {
                foreach (string filename in files)
                {
                    using (StreamReader streamReader = new StreamReader(File.OpenRead(filename)))
                    {
                        while (!streamReader.EndOfStream)
                        {
                            string line = streamReader.ReadLine();

                            if (Regex.IsMatch(line, @"@\$[A-Z]{2}\d+"))
                            {
                                filesWithTempLabels.Add(filename);
                                break;
                            }
                        }
                    }
                }
            }

            return filesWithTempLabels;
        }

        protected void GetFiles(string directory, bool recursive, List<string> files)
        {
            foreach (string filename in Directory.GetFiles(directory, "*.xpo"))
            {
                files.Add(filename);
            }

            if (recursive)
            {
                foreach (string subdirectory in Directory.GetDirectories(directory))
                {
                    GetFiles(subdirectory, recursive, files);
                }
            }
        }
    }
}
