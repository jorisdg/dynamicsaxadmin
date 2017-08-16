//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Dynamics.AX.Framework.Tools.ModelManagement;
using Microsoft.Win32;

namespace CodeCrib.AX.Manage
{
    public class Model
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Publisher { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }

    public class ModelStore
    {
        string dbServer;
        string dbName;
        string axutilBinaryFolder;

        public static void ExtractModelInfo(string modelManifest, out string publisher, out string modelName, out string layer)
        {
            ModelManifest manifest = ModelManifest.Read(modelManifest);

            publisher = manifest.Publisher;
            modelName = manifest.Name;
            layer = manifest.Layer.ToString();
        }

        protected static string GetAxutilBinaryFolder()
        {
            RegistryKey AXInstall = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Dynamics\6.0\Setup");
            string path = AXInstall.GetValue("InstallDir") + @"\ManagementUtilities\";

            return path;
        }

        public ModelStore(string databaseServer, string modelDatabaseName)
            : this(databaseServer, modelDatabaseName, GetAxutilBinaryFolder())
        {
        }

        public ModelStore(string databaseServer, string modelDatabaseName, string axutilFolder)
        {
            dbServer = databaseServer;
            dbName = modelDatabaseName;
            if (!string.IsNullOrEmpty(axutilFolder))
                axutilBinaryFolder = axutilFolder;
            else
                axutilBinaryFolder = GetAxutilBinaryFolder();
        }

        public bool ModelExist(string modelName, string modelPublisher, string layer = "")
        {
            AxUtilContext utilContext = new AxUtilContext();
            AxUtilConfiguration config = new AxUtilConfiguration() { Server = dbServer, Database = dbName, ModelArgument = new ModelArgument(modelName, modelPublisher) };

            bool modelFound = false;

            AxUtil util = new AxUtil();
            IList<ModelManifest> list = util.List(utilContext, config);

            foreach (ModelManifest manifest in list)
            {
                if (manifest.Name == modelName && manifest.Publisher == modelPublisher && (string.IsNullOrEmpty(layer) || layer.ToLower() == manifest.Layer.ToString().ToLower()))
                    modelFound = true;
            }

            return modelFound;
        }

        public void ExportModel(string modelName, string modelPublisher, string modelFile, string strongNameKeyFile)
        {
            ExportModelShell(modelName, modelPublisher, modelFile, strongNameKeyFile);

            /*
             * There is a reported bug where exported model is .NET 4.0 instead of 2.0
             *  which then results in a "invalid format" error when trying to import the model
             *  from the command line tools
             * 
             * Only current workaround is to use the command line tools.
             * 
             * Below code works, but model is .NET 4.0
             
            
            AxUtilContext utilContext = new AxUtilContext();
            AxUtilConfiguration config = new AxUtilConfiguration() { Server = dbServer, Database = dbName, ExportFile = modelFile, ModelArgument = new ModelArgument(modelName, modelPublisher) };

            if (!String.IsNullOrEmpty(strongNameKeyFile))
            {
                config.StrongNameKeyFile = strongNameKeyFile;
            }
            
            AxUtil util = new AxUtil();
            util.Export(utilContext, config);

            if (utilContext.ExecutionStatus == ExecutionStatus.Error)
            {
                throw new Exception(string.Format("Model export failed: {0}", GetUtilContextErrorStr(utilContext)));
            }
            */
        }

        protected void ExportModelShell(string modelName, string modelPublisher, string modelFile, string strongNameKeyFile)
        {
            string parameters = String.Format("export /s:{0} /db:{1} \"/model:({2},{3})\" \"/file:{4}\"", dbServer, dbName, modelName, modelPublisher, modelFile);

            if (!String.IsNullOrEmpty(strongNameKeyFile))
            {
                parameters = String.Format("{0} \"/key:{1}\"", parameters, strongNameKeyFile);
            }

            ProcessStartInfo processStartInfo = new ProcessStartInfo(axutilBinaryFolder + @"\axutil.exe", parameters);
            processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            processStartInfo.WorkingDirectory = axutilBinaryFolder;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            Process process = Process.Start(processStartInfo);
            string error = process.StandardError.ReadToEnd();
            string info = process.StandardOutput.ReadToEnd();

            try
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception();
            }
            catch
            {
                throw new Exception(string.Format("Error exporting model: {0}", string.IsNullOrEmpty(error) ? info : error));
            }
        }

        public void ExportModelStore(string modelStoreFile)
        {
            ExportModelStoreShell(modelStoreFile);

            /*
             * There is a reported bug where exported model is .NET 4.0 instead of 2.0
             *  which then results in a "invalid format" error when trying to import the model
             *  from the command line tools
             * 
             * Only current workaround is to use the command line tools.
             * 
             * Below code works, but model is .NET 4.0
            
            AxUtilContext utilContext = new AxUtilContext();
            AxUtilConfiguration config = new AxUtilConfiguration() { Server = dbServer, Database = dbName, ExportFile = modelStoreFile };

            AxUtil util = new AxUtil();
            util.ExportStore(utilContext, config, false);

            if (utilContext.ExecutionStatus == ExecutionStatus.Error)
            {
                throw new Exception(string.Format("Model store export failed: {0}", GetUtilContextErrorStr(utilContext)));
            }
            */
        }

        protected void ExportModelStoreShell(string modelStoreFile)
        {
            string parameters = String.Format("exportstore /s:{0} /db:{1} \"/file:{2}\"", dbServer, dbName, modelStoreFile);

            ProcessStartInfo processStartInfo = new ProcessStartInfo(axutilBinaryFolder + @"\axutil.exe", parameters);
            processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            processStartInfo.WorkingDirectory = axutilBinaryFolder;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            Process process = Process.Start(processStartInfo);
            string error = process.StandardError.ReadToEnd();
            string info = process.StandardOutput.ReadToEnd();

            try
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception();
            }
            catch
            {
                throw new Exception(string.Format("Error exporting model store: {0}", string.IsNullOrEmpty(error) ? info : error));
            }
        }

        public List<string> GetModelElements(string manifestFile)
        {
            ModelManifest manifest = ModelManifest.Read(manifestFile);
            return GetModelElements(manifest.Name, manifest.Publisher);
        }

        public List<string> GetModelElements(string modelName, string modelPublisher)
        {
            List<string> modelElements = new List<string>();
            AxUtilContext utilContext = new AxUtilContext();
            var modelArgument = new ModelArgument(modelName, modelPublisher);

            AxUtil util = new AxUtil();
            util.Context = new AxUtilContext();
            util.Config = new AxUtilConfiguration();
            var contents = util.View(modelArgument, true);

            foreach (var c in contents.Elements)
            {
                modelElements.Add(c.Path);
            }

            return modelElements;
        }

        public void CreateModel(string manifestFile)
        {
            ModelManifest manifest = ModelManifest.Read(manifestFile);
            CreateModelFromManifest(manifest);
        }

        public void CreateModel(string manifestFile, string version, string description)
        {
            ModelManifest manifest = ModelManifest.Read(manifestFile);

            if (!string.IsNullOrEmpty(version))
                manifest.Version = version;

            if (!string.IsNullOrEmpty(description))
                manifest.Description = description;

            CreateModelFromManifest(manifest);
        }

        public void CreateModel(string modelName, string modelPublisher, string layer, string displayName, string description, string version)
        {
            ModelManifest manifest = new ModelManifest();
            manifest.Name = modelName;
            manifest.Publisher = modelPublisher;
            manifest.Version = version;
            manifest.DisplayName = displayName;
            manifest.Description = description;
            manifest.Layer = (Layer)System.Enum.Parse(typeof(Layer), layer, true);

            CreateModelFromManifest(manifest);
        }

        protected void CreateModelFromManifest(ModelManifest manifest)
        {
            AxUtilContext utilContext = new AxUtilContext();
            AxUtilConfiguration config = new AxUtilConfiguration() { Server = dbServer, Database = dbName, ModelArgument = new ModelArgument(manifest), Layer = manifest.Layer.ToString() };

            AxUtil util = new AxUtil();
            bool created = util.Create(utilContext, config, manifest);

            if (utilContext.ExecutionStatus == ExecutionStatus.Error)
            {
                throw new Exception(string.Format("Model creation failed: {0}", GetUtilContextErrorStr(utilContext)));
            }

            if (!created)
            {
                throw new Exception("Model creation failed.");
            }
        }

        protected void InstallModel(string modelFile, ConflictModelResolverType resolverType)
        {
            // Due to issues with appdomain assembly loading getting messed up by calling the import method,
            // currently we've disabled the use of the DLL and are using shell command instead.

            //AxUtilContext utilContext = new AxUtilContext();
            //AxUtilConfiguration config = new AxUtilConfiguration() { Server = dbServer, Database = dbName };
            //config.ImportFiles.Add(modelFile);

            //config.ConflictResolver = resolverType;

            //AxUtil util = new AxUtil();
            //IList<ModelContents> importedModels = util.Import(utilContext, config);

            //if (utilContext.ExecutionStatus == ExecutionStatus.Error)
            //{
            //    throw new Exception(string.Format("Model install failed: {0}", GetUtilContextErrorStr(utilContext)));
            //}

            //if (importedModels == null || importedModels.Count != 1)
            //{
            //    throw new Exception("Model install failed.");
            //}

            this.InstallModelShell(modelFile, resolverType);
        }

        protected void InstallModelShell(string modelFile, ConflictModelResolverType resolverType)
        {
            string parameters = String.Format("import /s:{0} /db:{1} \"/file:{2}\" /conflict:{3} /noPrompt", dbServer, dbName, modelFile, resolverType.ToString());

            ProcessStartInfo processStartInfo = new ProcessStartInfo(axutilBinaryFolder + @"\axutil.exe", parameters);
            processStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            processStartInfo.WorkingDirectory = axutilBinaryFolder;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;

            Process process = Process.Start(processStartInfo);
            string error = process.StandardError.ReadToEnd();
            string info = process.StandardOutput.ReadToEnd();

            try
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                    throw new Exception();
            }
            catch
            {
                throw new Exception(string.Format("Error installing model: {0}", string.IsNullOrEmpty(error) ? info : error));
            }
        }

        public void InstallModel(string modelFile)
        {
            InstallModel(modelFile, ConflictModelResolverType.Reject);
        }

        public void InstallModelPush(string modelFile)
        {
            InstallModel(modelFile, ConflictModelResolverType.Push);
        }

        public void InstallModelOverwrite(string modelFile)
        {
            InstallModel(modelFile, ConflictModelResolverType.Overwrite);
        }

        public void SetNoInstallMode()
        {
            AxUtilContext utilContext = new AxUtilContext();
            AxUtilConfiguration config = new AxUtilConfiguration() { Server = dbServer, Database = dbName };

            AxUtil util = new AxUtil(utilContext, config);
            util.ApplyInstallModeState(InstallModeState.NoInstallMode);

            if (utilContext.ExecutionStatus == ExecutionStatus.Error)
            {
                throw new Exception(string.Format("Setting no install mode failed: {0}", GetUtilContextErrorStr(utilContext)));
            }
        }

        public void UninstallModel(string modelName, string modelPublisher)
        {
            AxUtilContext utilContext = new AxUtilContext();
            AxUtilConfiguration config = new AxUtilConfiguration() { Server = dbServer, Database = dbName, ModelArgument = new ModelArgument(modelName, modelPublisher) };

            AxUtil util = new AxUtil();
            util.Delete(utilContext, config);

            if (utilContext.ExecutionStatus == ExecutionStatus.Error)
            {
                throw new Exception(string.Format("Model uninstall failed: {0}", GetUtilContextErrorStr(utilContext)));
            }
        }

        public void UninstallAllLayerModels(string layer)
        {
            AxUtilContext utilContext = new AxUtilContext();
            AxUtilConfiguration config = new AxUtilConfiguration() { Server = dbServer, Database = dbName, Layer = layer };

            AxUtil util = new AxUtil();
            util.Delete(utilContext, config);

            if (utilContext.ExecutionStatus == ExecutionStatus.Error)
            {
                throw new Exception(string.Format("Uninstall all models in layer failed: {0}", GetUtilContextErrorStr(utilContext)));
            }
        }

        static ModelStore()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += new ResolveEventHandler(MyResolveEventHandler);
        }

        protected static string GetUtilContextErrorStr(AxUtilContext utilContext)
        {
            string errorStr = "";
            foreach (string error in utilContext.Errors)
            {
                if (!string.IsNullOrEmpty(error))
                    errorStr = string.Format("\n{0}", error);
                else
                    errorStr = string.Format("{0}", error);
            }

            return errorStr;
        }

        static public Assembly MyResolveEventHandler(object sender, ResolveEventArgs args)
        {
            //This handler is called only when the common language runtime tries to bind to the assembly and fails.

            //Retrieve the list of referenced assemblies in an array of AssemblyName.
            Assembly MyAssembly = null, objExecutingAssemblies;
            string strTempAssmbPath = "";

            objExecutingAssemblies = Assembly.GetExecutingAssembly();
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();

            //Loop through the array of referenced assembly names.
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                string name = args.Name.Substring(0, args.Name.IndexOf(","));
                //Check for the assembly names that have raised the "AssemblyResolve" event.
                if (strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == name)
                {
                    if (name.ToLower() == "axutillib")
                    {
                        //Build the path of the assembly from where it has to be loaded.				
                        RegistryKey AXInstall = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Dynamics\6.0\Setup");
                        strTempAssmbPath = string.Format(@"{0}\ManagementUtilities\{1}.dll", AXInstall.GetValue("InstallDir"), name);
                        break;
                    }
                }
            }

            if (!String.IsNullOrEmpty(strTempAssmbPath))
            {
                //Load the assembly from the specified path. 					
                MyAssembly = Assembly.LoadFrom(strTempAssmbPath);
            }

            //Return the loaded assembly.
            return MyAssembly;
        }
    }
}
