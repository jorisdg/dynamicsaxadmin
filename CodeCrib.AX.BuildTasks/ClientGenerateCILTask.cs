using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class ClientGenerateCILTask : ClientBuildTask
    {
        protected string ClientExecutablePath;

        protected string LogFile;

        public ClientGenerateCILTask(IBuildLogger buildLogger, string configurationFile) : base(buildLogger, 0, configurationFile)
        {
        }

        public ClientGenerateCILTask(IBuildLogger buildLogger, int timeoutMinutes, string configurationFile) : base(buildLogger, timeoutMinutes, configurationFile)
        {
        }

        public ClientGenerateCILTask(IBuildLogger buildLogger, int timeoutMinutes, string configurationFile, string clientExecutablePath) : base(buildLogger, timeoutMinutes, configurationFile)
        {
            ClientExecutablePath = clientExecutablePath;
        }

        public ClientGenerateCILTask()
        {
        }

        public override Process Start()
        {
            BuildLogger.LogInformation("Generating CIL");

            Client.Commands.GenerateCIL compile = new Client.Commands.GenerateCIL()
            {
                Minimize = true,
                LazyClassLoading = true,
                LazyTableLoading = true,
            };

            if (!string.IsNullOrEmpty(ConfigurationFile))
            {
                compile.ConfigurationFile = ConfigurationFile;
            }

            Process process = null;
            if (string.IsNullOrEmpty(ClientExecutablePath))
            {
                process = Client.Client.StartCommand(compile);
            }
            else
            {
                process = Client.Client.StartCommand(ClientExecutablePath, compile);
            }

            LogFile = Path.Combine(Environment.ExpandEnvironmentVariables(Deploy.Configs.GetServerConfig(ConfigurationFile).AlternateBinDirectory), @"XppIL\Dynamics.Ax.Application.dll.log");

            return process;
        }

        public override void End()
        {
            BuildLogger.StoreLogFile(LogFile);

            Client.CILGenerationOutput output = null;
            try
            {
                output = Client.CILGenerationOutput.CreateFromFile(LogFile);
            }
            catch (FileNotFoundException ex)
            {
                throw new Exception("CIL generation log could not be found", ex);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error parsing CIL generation log: {0}", ex.Message), ex);
            }

            bool hasErrors = false;
            foreach (var item in output.Output)
            {
                string compileMessage;

                if (item.LineNumber > 0)
                    compileMessage = string.Format("Object {0} method {1}, line {2} : {3}", item.ElementName, item.MethodName, item.LineNumber, item.Message);
                else
                    compileMessage = string.Format("Object {0} method {1} : {2}", item.ElementName, item.MethodName, item.Message);

                switch (item.Severity)
                {
                    // Compile Errors
                    case 0:
                        BuildLogger.LogError(compileMessage);
                        hasErrors = true;
                        break;
                    // Compile Warnings
                    case 1:
                        BuildLogger.LogWarning(compileMessage);
                        break;
                    // "Other"
                    case 4:
                    default:
                        BuildLogger.LogInformation(item.Message);
                        break;
                }
            }
            if (hasErrors)
            {
                throw new Exception("CIL error(s) found");
            }
        }

    }
}
