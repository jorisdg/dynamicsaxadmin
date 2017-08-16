//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCrib.AX.AXBuild.Commands
{
    public class Command
    {
        public string AltBinDir { get; set; }
        public string LogPath { get; set; }
        public string AOSInstance { get; set; }
        public string DbServer { get; set; }
        public string DbName { get; set; }
        public bool NoCleanup { get; set; }
        public int Workers { get; set; }

        virtual public List<string> Parameters()
        {
            List<string> parameters = new List<string>();

            if (!String.IsNullOrEmpty(AltBinDir))
            {
                parameters.Add(String.Format("/altbin=\"{0}\"", AltBinDir));
            }

            if (!String.IsNullOrEmpty(LogPath))
            {
                parameters.Add(String.Format("/log=\"{0}\"", LogPath));
            }

            if (!String.IsNullOrEmpty(AOSInstance))
            {
                parameters.Add(String.Format("/aos=\"{0}\"", AOSInstance));
            }

            if (!String.IsNullOrEmpty(DbServer))
            {
                parameters.Add(String.Format("/dbserver=\"{0}\"", DbServer));
            }

            if (!String.IsNullOrEmpty(DbName))
            {
                parameters.Add(String.Format("/modelstore=\"{0}\"", DbName));
            }

            if (NoCleanup)
            {
                parameters.Add("/nocleanup");
            }

            if (Workers > 0)
            {
                parameters.Add(String.Format("/workers={0}", Workers));
            }

            return parameters;
        }
    }

    public class Compile : Command
    {
        public string Compiler { get; set; }

        public override List<string> Parameters()
        {
            List<string> parameters = base.Parameters();

            parameters.Insert(0, "xppcompileall");

            if (!String.IsNullOrEmpty(Compiler))
            {
                parameters.Add(String.Format("/compiler=\"{0}\"", Compiler));
            }

            return parameters;
        }
    }
}
