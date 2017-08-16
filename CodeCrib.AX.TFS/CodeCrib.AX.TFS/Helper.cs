using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.TFS
{
    public class RemoteServerException : System.Exception
    {
        public RemoteServerException(string message) : base(message) { }
    }

    class Helper
    {
        public static CodeCrib.AX.Config.Client GetClientConfig(string clientConfigFile)
        {
            CodeCrib.AX.Config.Client clientConfig = null;

            if (!string.IsNullOrEmpty(clientConfigFile))
                clientConfig = CodeCrib.AX.Config.Client.GetConfigFromFile(clientConfigFile);
            else
                clientConfig = CodeCrib.AX.Config.Client.GetConfigFromRegistry();

            return clientConfig;
        }

        public static CodeCrib.AX.Config.Server GetServerConfig(string clientConfigFile)
        {
            CodeCrib.AX.Config.Client clientConfig = GetClientConfig(clientConfigFile);

            if (clientConfig.Connections[0].ServerName != System.Environment.MachineName)
                throw new RemoteServerException(string.Format("Build does not support remote servers, client config server name ({0}) differs from current server ({1})", clientConfig.Connections[0].ServerName, System.Environment.MachineName));

            var servers = CodeCrib.AX.Config.Server.GetAOSInstances();
            var serverConfig = (from c in
                                    (from s in servers select CodeCrib.AX.Config.Server.GetConfigFromRegistry(s))
                                where c.TCPIPPort == clientConfig.Connections[0].TCPIPPort
                                && c.WSDLPort == clientConfig.Connections[0].WSDLPort
                                select c).FirstOrDefault();

            return serverConfig;
        }

        public static uint GetServerNumber(string clientConfigFile)
        {
            CodeCrib.AX.Config.Client clientConfig = GetClientConfig(clientConfigFile);

            if (clientConfig.Connections[0].ServerName != System.Environment.MachineName)
                throw new RemoteServerException(string.Format("Build does not support remote servers, client config server name ({0}) differs from current server ({1})", clientConfig.Connections[0].ServerName, System.Environment.MachineName));

            uint aosNumber = CodeCrib.AX.Config.Server.GetAOSNumber((uint)clientConfig.Connections[0].TCPIPPort);

            return aosNumber;
        }

        /// <summary>
        /// Populates the out parameters according to the provided manifest, configuration file, and layer codes list.  Typically used to launch
        /// a client for a specific layer and/or model.
        /// </summary>
        /// <param name="configurationFile">An AX configuration file, used to resolve server configuration and therefore model store location.</param>
        /// <param name="layerCodes">A list of layer access codes.</param>
        /// <param name="modelManifest">A manifest from which layer, model, and publisher information is retrieved.</param>
        /// <param name="modelName">Returns the model name referenced by the manifest.</param>
        /// <param name="publisher">Returns the publisher name referenced by the manifest.</param>
        /// <param name="layer">Returns the layer ID referenced by the manifest.</param>
        /// <param name="layerCode">Returns the layer code for the layer ID referenced by the manifest.</param>
        public static void ExtractClientLayerModelInfo(string configurationFile, StringList layerCodes, string modelManifest, out string modelName, out string publisher, out string layer, out string layerCode)
        {
            CodeCrib.AX.Manage.ModelStore.ExtractModelInfo(modelManifest, out publisher, out modelName, out layer);

            string layerInternal = layer;

            Config.Server serverConfig = Helper.GetServerConfig(configurationFile);
            CodeCrib.AX.Manage.ModelStore modelStore = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3) == "6.0")
            {
                modelStore = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database));
            }
            else
            {
                modelStore = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            // Validate if model exists unless it's the default layer model
            if (!modelStore.ModelExist(modelName, publisher, layer) && !string.Equals(string.Format("{0} model", layer), modelName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new Exception(string.Format("Model {0} ({1}) does not exist in layer {2}", modelName, publisher, layer));
            }

            // Supports:
            // var:CODE
            // var : CODE
            // varCODE
            // var CODE
            layerCode = (from c in layerCodes where c.Substring(0, 3).ToLower() == layerInternal.ToLower() select c.Substring(3).Trim()).FirstOrDefault();
            if (!string.IsNullOrEmpty(layerCode) && layerCode[0] == ':')
            {
                layerCode = layerCode.Substring(1).Trim();
            }

            // An empty layer code is only allowed when either not specifying a layer, or when explicitly specifying the USR or USP layer.
            if (string.IsNullOrEmpty(layerCode) && !string.IsNullOrEmpty(layer) && String.Compare(layer, "USR", true) != 0 && String.Compare(layer, "USP", true) != 0)
            {
                throw new Exception(string.Format("Layer '{0}' requires an access code which couldn't be found in the Layer Codes argument", layer));
            }
        }

        /// <summary>
        /// Compresses a file, creating a new file with a .gz extension.
        /// </summary>
        /// <param name="fileInfo">A FileInfo object identifying the file to compress.</param>
        /// <param name="deleteOriginal">Should the original file be deleted?  Defaults to yes.</param>
        public static void CompressFile(FileInfo fileInfo, bool deleteOriginal = true)
        {
            using (BufferedStream fileReader = new BufferedStream(new FileStream(fileInfo.FullName, FileMode.Open)))
            {
                using (GZipStream fileWriter = new GZipStream(new BufferedStream(new FileStream(fileInfo.FullName + ".gz", FileMode.Create)), CompressionMode.Compress))
                {
                    fileReader.CopyTo(fileWriter);

                    fileWriter.Flush();
                    fileWriter.Close();
                }

                fileReader.Close();
            }

            if (deleteOriginal)
            {
                File.Delete(fileInfo.FullName);
            }
        }

        public static void ReportCompileMessages(CodeActivityContext context, CodeCrib.AX.Client.CompileOutput output)
        {
            bool hasErrors = false;
            foreach (var item in output.Output)
            {
                string compileMessage = String.Format("{0}, line {1}, column {2} : {3}", item.TreeNodePath, item.LineNumber, item.ColumnNumber, item.Message);
                switch (item.Severity)
                {
                    // Compile Errors
                    case 0:
                        context.TrackBuildError(compileMessage);
                        hasErrors = true;
                        break;
                    // Compile Warnings
                    case 1:
                    case 2:
                    case 3:
                        context.TrackBuildWarning(compileMessage);
                        break;
                    // Best practices
                    case 4:
                        context.TrackBuildMessage(string.Format("BP: {0}", compileMessage), BuildMessageImportance.Low);
                        break;
                    // TODOs
                    case 254:
                    case 255:
                        context.TrackBuildWarning(string.Format("TODO: {0}", compileMessage));
                        break;
                    // "Other"
                    default:
                        context.TrackBuildMessage(compileMessage);
                        break;
                }
            }
            if (hasErrors)
            {
                throw new Exception("Compile error(s) found");
            }
        }

    }
}
