using CodeCrib.AX.Deploy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class DeployReferencesTask : BuildTask
    {
        protected string ReferencesFolderPath;

        public DeployReferencesTask(IBuildLogger buildLogger, string configurationFile, string referencesFolderPath) : base(buildLogger, 0, configurationFile)
        {
            ReferencesFolderPath = referencesFolderPath;
        }

        public DeployReferencesTask()
        {
        }

        public override void Run()
        {
            if (string.IsNullOrEmpty(ReferencesFolderPath))
            {
                BuildLogger.LogInformation("References folder path not specified");
                return;
            }

            BuildLogger.LogInformation("Deploying references to client and server");

            if (Directory.Exists(ReferencesFolderPath))
            {
                string clientBasePath = Path.Combine(Environment.GetEnvironmentVariable("localappdata"), @"Microsoft\Dynamics Ax");
                IEnumerable<string> folders = Directory.EnumerateDirectories(clientBasePath, "VSAssemblies*");

                Config.Server serverConfig = Configs.GetServerConfig(ConfigurationFile);
                string serverPath = Path.Combine(serverConfig.AlternateBinDirectory, "VSAssemblies");

                if (!Directory.Exists(serverPath))
                {
                    BuildLogger.LogInformation(string.Format("Creating directory '{0}' because it does not already exist", serverPath));
                    Directory.CreateDirectory(serverPath);
                }

                IEnumerable<string> files = Directory.EnumerateFiles(ReferencesFolderPath, "*");
                foreach (string file in files)
                {
                    // Copy to client destinations
                    foreach (string folder in folders)
                    {
                        string destination = Path.Combine(folder, Path.GetFileName(file));
                        BuildLogger.LogInformation(string.Format("Copying '{0}' to '{1}'", file, destination));
                        File.Copy(file, destination);
                    }

                    // Copy to server destination
                    string serverDestination = Path.Combine(serverPath, Path.GetFileName(file));
                    BuildLogger.LogInformation(string.Format("Copying '{0}' to '{1}'", file, serverDestination));
                    File.Copy(file, serverDestination);
                    // Set the readonly flag on the references copied to the server location, to prevent their
                    // deletion during the build.
                    File.SetAttributes(serverDestination, FileAttributes.ReadOnly | File.GetAttributes(serverDestination));
                }
            }
            else
            {
                BuildLogger.LogWarning(string.Format("Folder '{0}' containing reference files does not exist", ReferencesFolderPath));
            }
        }
    }
}
