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
    public class CombineXPOs
    {
        public enum IncludeObjects
        {
            AllObjects,
            ExcludeSystemObjects,
            SystemObjectsOnly
        }

        private static List<string> defaultSystemClasses = new List<string>() { "syssetupformrun.xpo", "info.xpo", "classfactory.xpo", "application.xpo", "session.xpo" };
        public static List<string> SystemClasses { get { return defaultSystemClasses; } set { defaultSystemClasses = value; } }

        const string XPOSTARTLINE1 = @"Exportfile for AOT version 1.0 or later";
        const string XPOSTARTLINE2 = @"Formatversion: 1";
        const string XPOENDLINE1 = @"***Element: END";

        public static void Combine(string folder, bool recursive, string combinedXPOFilename, IncludeObjects inclusion)
        {
            CombineXPOs combine = new CombineXPOs();
            List<string> files = new List<string>();
            combine.GetFiles(folder, recursive, files);

            if (files.Count < 1)
                throw new Exception(String.Format("No XPO files found in {0}", folder));

            switch (inclusion)
            {
                case IncludeObjects.AllObjects:
                    // we already have this one
                    break;
                case IncludeObjects.ExcludeSystemObjects:
                    files = (from f in files where !SystemClasses.Contains(System.IO.Path.GetFileName(f)) select f).ToList();
                    break;
                case IncludeObjects.SystemObjectsOnly:
                    files = (from f in files where SystemClasses.Contains(System.IO.Path.GetFileName(f)) select f).ToList();
                    break;
            }

            combine.Combine(files, combinedXPOFilename);
        }

        public void Combine(List<string> files, string combinedXPOFilename)
        {
            if (files.Count != 0)
            {
                using (StreamWriter writer = new StreamWriter(combinedXPOFilename, false, Encoding.Unicode))
                {
                    writer.WriteLine(XPOSTARTLINE1);
                    writer.WriteLine(XPOSTARTLINE2);

                    List<string>.Enumerator fileEnumerator = files.GetEnumerator();
                    while (fileEnumerator.MoveNext())
                    {
                        Combine(fileEnumerator.Current, writer);
                    }

                    writer.WriteLine(XPOENDLINE1);
                    writer.Flush();
                }
            }                
        }

        //protected void AddMacro(string macroName, string contents, StreamWriter combinedFile)
        //{
        //    string xpoText = "";

        //    xpoText = String.Format("***Element: MCR{0}", System.Environment.NewLine);
        //    xpoText = String.Format("{0}{1}", xpoText, System.Environment.NewLine);
        //    xpoText = String.Format("{0}; Microsoft Dynamics AX Macro: {1} unloaded{2}", xpoText, macroName, System.Environment.NewLine);
        //    xpoText = String.Format("{0}; --------------------------------------------------------------------------------{1}", xpoText, System.Environment.NewLine);
        //    xpoText = String.Format("{0}  JOBVERSION 1{1}", xpoText, System.Environment.NewLine);
        //    xpoText = String.Format("{0}  {1}", xpoText, System.Environment.NewLine);
        //    xpoText = String.Format("{0}  SOURCE #{1}{2}", xpoText, macroName, System.Environment.NewLine);

        //    string[] lines = contents.Split('\n');
        //    foreach (string line in lines)
        //    {
        //        xpoText = String.Format("{0}    #{1}{2}", xpoText, line, System.Environment.NewLine);
        //    }
        //    xpoText = String.Format("{0}  ENDSOURCE{1}", xpoText, System.Environment.NewLine);

        //    combinedFile.Write(xpoText);
        //}

        protected void Combine(string filename, StreamWriter combinedFile)
        {
            using (StreamReader streamReader = new StreamReader(File.OpenRead(filename)))
            {
                while (!streamReader.EndOfStream)
                {
                    string line = streamReader.ReadLine();

                    if (!line.Equals(XPOENDLINE1)
                        && !line.Equals(XPOSTARTLINE1)
                        && !line.Equals(XPOSTARTLINE2))
                    {
                        combinedFile.WriteLine(line);
                    }
                }
            }
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
