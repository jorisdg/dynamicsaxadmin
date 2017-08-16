//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System.IO;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class Clean : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<bool> LeaveXppIL { get; set; }

        private void CleanFolder(string path, string filePattern)
        {
            if (System.IO.Directory.Exists(path))
            {
                IEnumerable<string> files = System.IO.Directory.EnumerateFiles(path, filePattern);
                foreach (string file in files)
                {
                    System.IO.File.SetAttributes(file, FileAttributes.Normal);
                    System.IO.File.Delete(file);
                }
            }
        }

        private void CleanFolders(string path, string pathPattern, string filePattern)
        {
            if (System.IO.Directory.Exists(path))
            {
                IEnumerable<string> folders = System.IO.Directory.EnumerateDirectories(path, pathPattern);
                foreach (string folder in folders)
                {
                    CleanFolder(folder, filePattern);
                }
            }
        }

        protected override void Execute(CodeActivityContext context)
        {
            bool leaveXppIL = LeaveXppIL.Get(context);
            string configurationFile = ConfigurationFile.Get(context);
            var serverConfig = Helper.GetServerConfig(configurationFile);

            context.TrackBuildMessage("Cleaning server label artifacts");
            CleanFolder(string.Format(@"{0}\Application\Appl\Standard", serverConfig.AlternateBinDirectory), "ax*.al?");

            if (!leaveXppIL)
            {
                context.TrackBuildMessage("Cleaning server XppIL artifacts");
                CleanFolder(string.Format(@"{0}\XppIL", serverConfig.AlternateBinDirectory), "*");
            }

            context.TrackBuildMessage("Cleaning server VSAssemblies artifacts");
            CleanFolder(string.Format(@"{0}\VSAssemblies", serverConfig.AlternateBinDirectory), "*");

            context.TrackBuildMessage("Cleaning client cache artifacts");
            CleanFolder(System.Environment.GetEnvironmentVariable("localappdata"), "ax_*.auc");
            CleanFolder(System.Environment.GetEnvironmentVariable("localappdata"), "ax*.kti");

            context.TrackBuildMessage("Cleaning client VSAssemblies artifacts");
            CleanFolders(string.Format(@"{0}\{1}", System.Environment.GetEnvironmentVariable("localappdata"), @"Microsoft\Dynamics Ax"), "VSAssemblies*", "*");
        }
    }
}
