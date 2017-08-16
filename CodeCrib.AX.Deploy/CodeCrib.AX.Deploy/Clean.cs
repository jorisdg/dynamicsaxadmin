using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CodeCrib.AX.Deploy
{
    public class Clean
    {
        private static void CleanFolder(string path, string filePattern)
        {
            if (System.IO.Directory.Exists(path))
            {
                var files = System.IO.Directory.GetFiles(path, filePattern);
                foreach (string file in files)
                {
                    System.IO.File.SetAttributes(file, FileAttributes.Normal);
                    System.IO.File.Delete(file);
                }
            }
        }

        private static void CleanFolders(string path, string pathPattern, string filePattern)
        {
            if (System.IO.Directory.Exists(path))
            {
                var folders = System.IO.Directory.GetDirectories(path, pathPattern);
                foreach (string folder in folders)
                {
                    CleanFolder(folder, filePattern);
                }
            }
        }

        public static void ClientCaches()
        {
            //context.TrackBuildMessage("Cleaning client cache artifacts");
            CleanFolder(System.Environment.GetEnvironmentVariable("localappdata"), "ax_*.auc");
            CleanFolder(System.Environment.GetEnvironmentVariable("localappdata"), "ax*.kti");

            //context.TrackBuildMessage("Cleaning client VSAssemblies artifacts");
            CleanFolders(string.Format(@"{0}\{1}", System.Environment.GetEnvironmentVariable("localappdata"), @"Microsoft\Dynamics Ax"), "VSAssemblies*", "*");
        }

        public static void ServerCaches(string configurationFile = null, bool leaveXppIL = false)
        {
            var serverConfig = CodeCrib.AX.Deploy.Configs.GetServerConfig(configurationFile);

            ServerCaches(serverConfig, leaveXppIL);
        }

        public static void ServerCaches(CodeCrib.AX.Config.Client clientConfig, bool leaveXppIL = false)
        {
            var serverConfig = CodeCrib.AX.Deploy.Configs.GetServerConfig(clientConfig);

            ServerCaches(serverConfig, leaveXppIL);
        }

        public static void ServerCaches(CodeCrib.AX.Config.Server serverConfig, bool leaveXppIL = false)
        {
            //context.TrackBuildMessage("Cleaning server label artifacts");
            CleanFolder(string.Format(@"{0}\Application\Appl\Standard", serverConfig.AlternateBinDirectory), "ax*.al?");

            if (!leaveXppIL)
            {
                //context.TrackBuildMessage("Cleaning server XppIL artifacts");
                CleanFolder(string.Format(@"{0}\XppIL", serverConfig.AlternateBinDirectory), "*");
            }

            //context.TrackBuildMessage("Cleaning server VSAssemblies artifacts");
            CleanFolder(string.Format(@"{0}\VSAssemblies", serverConfig.AlternateBinDirectory), "*");
        }
    }
}
