//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeCrib.AX.Sql
{
    static class DbServerExtensions
    {
        public static string DataPath(this Server server)
        {
            // If the default data path has never been modified from the original
            // value, the DefaultFile property may be blank.
            if (String.IsNullOrEmpty(server.Settings.DefaultFile))
            {
                return server.Information.MasterDBPath;
            }

            return server.Settings.DefaultFile;
        }

        public static string LogPath(this Server server)
        {
            // If the default log path has never been modified from the original
            // value, the DefaultFile property may be blank.
            if (String.IsNullOrEmpty(server.Settings.DefaultLog))
            {
                return server.Information.MasterDBLogPath;
            }

            return server.Settings.DefaultLog;
        }
    }
}
