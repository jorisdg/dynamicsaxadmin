using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace CodeCrib.AX.Deploy
{
    public class Configs
    {
        public static string GetFQDN()
        {
            string domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            string hostName = Dns.GetHostName();

            if (!hostName.EndsWith(domainName))  // if hostname does not already include domain name
            {
                hostName = String.Concat(hostName, ".", domainName); // add the domain name part
            }

            return hostName; // return the fully qualified name
        }

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

            return GetServerConfig(clientConfig);
        }

        public static CodeCrib.AX.Config.Server GetServerConfig(CodeCrib.AX.Config.Client clientConfig)
        {
            if (!clientConfig.Connections[0].ServerName.Equals(System.Environment.MachineName, StringComparison.CurrentCultureIgnoreCase)
                && !clientConfig.Connections[0].ServerName.Equals(GetFQDN(), StringComparison.CurrentCultureIgnoreCase)
                && !clientConfig.Connections[0].ServerName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
                throw new Exception(string.Format("Build does not support remote servers, client config server name ({0}) differs from current server ({1})", clientConfig.Connections[0].ServerName, System.Environment.MachineName));

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

            if (!clientConfig.Connections[0].ServerName.Equals(System.Environment.MachineName, StringComparison.CurrentCultureIgnoreCase)
                && !clientConfig.Connections[0].ServerName.Equals(GetFQDN(), StringComparison.CurrentCultureIgnoreCase)
                && !clientConfig.Connections[0].ServerName.Equals("localhost", StringComparison.CurrentCultureIgnoreCase))
                throw new Exception(string.Format("Build does not support remote servers, client config server name ({0}) differs from current server ({1})", clientConfig.Connections[0].ServerName, System.Environment.MachineName));

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
        public static void ExtractClientLayerModelInfo(string configurationFile, List<string> layerCodes, string modelManifest, out string modelName, out string publisher, out string layer, out string layerCode)
        {
            CodeCrib.AX.Manage.ModelStore.ExtractModelInfo(modelManifest, out publisher, out modelName, out layer);

            string layerInternal = layer;

            Config.Server serverConfig = GetServerConfig(configurationFile);
            CodeCrib.AX.Manage.ModelStore modelStore = null;
            if (serverConfig.AOSVersionOrigin.Substring(0, 3).Equals("6.0"))
            {
                modelStore = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}", serverConfig.Database));
            }
            else
            {
                modelStore = new Manage.ModelStore(serverConfig.DatabaseServer, string.Format("{0}_model", serverConfig.Database));
            }

            // Validate if model exists unless it's the default layer model
            if (!modelStore.ModelExist(modelName, publisher, layer) && !string.Equals(string.Format("{0} model", layer), modelName, StringComparison.CurrentCultureIgnoreCase))
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

        protected static long CopyTo(Stream source, Stream destination)
        {
            byte[] buffer = new byte[2048];
            int bytesRead;
            long totalBytes = 0;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, bytesRead);
                totalBytes += bytesRead;
            }
            return totalBytes;
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
                String compressedFilename = String.Concat(fileInfo.FullName, ".gz");
                using (FileStream fileStream = new FileStream(compressedFilename, FileMode.Create))
                using (BufferedStream bufferedStream = new BufferedStream(fileStream))
                using (GZipStream fileWriter = new GZipStream(bufferedStream, CompressionMode.Compress))
                {
                    CopyTo(fileReader, fileWriter);

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
    }
}
