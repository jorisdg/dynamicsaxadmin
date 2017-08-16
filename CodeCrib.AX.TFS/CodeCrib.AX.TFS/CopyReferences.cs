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
    public class DeployReferences : CodeActivity
    {
        public InArgument<string> ConfigurationFile { get; set; }
        [RequiredArgument]
        public InArgument<string> ReferencesFolder { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            string configurationFile = ConfigurationFile.Get(context);
            var serverConfig = Helper.GetServerConfig(configurationFile);
            string sourcePath = ReferencesFolder.Get(context);

            context.TrackBuildMessage("Deploying references to client and server");

            if (System.IO.Directory.Exists(sourcePath))
            {
                string clientBasePath = string.Format(@"{0}\Microsoft\Dynamics Ax", System.Environment.GetEnvironmentVariable("localappdata"));
                IEnumerable<string> folders = System.IO.Directory.EnumerateDirectories(clientBasePath, "VSAssemblies*");
                string serverPath = string.Format(@"{0}\VSAssemblies", serverConfig.AlternateBinDirectory);

                if (!System.IO.Directory.Exists(serverPath))
                    System.IO.Directory.CreateDirectory(serverPath);

                IEnumerable<string> files = System.IO.Directory.EnumerateFiles(sourcePath, "*");
                foreach (string file in files)
                {
                    foreach (string folder in folders)
                    {
                        System.IO.File.Copy(file, string.Format(@"{0}\{1}", folder, System.IO.Path.GetFileName(file)));
                    }
                    System.IO.File.Copy(file, string.Format(@"{0}\{1}", serverPath, System.IO.Path.GetFileName(file)));
                }
            }
            else
            {
                context.TrackBuildWarning(string.Format("Folder '{0}' containing reference files does not exist", sourcePath));
            }
        }
    }
}