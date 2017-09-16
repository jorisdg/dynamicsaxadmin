using CodeCrib.AX.Client.AutoRun;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    abstract public class ClientBuildTask : BuildTask
    {
        protected List<string> LayerCodes;
        protected string ModelManifest;

        protected string ModelName;
        protected string Publisher;
        protected string Layer;
        protected string LayerCode;

        public AxaptaAutoRun AutoRun { get; protected set; }
        public string AutoRunFile { get; protected set; }

        public ClientBuildTask(
            IBuildLogger buildLogger,
            int timeoutMinutes,
            string configurationFile,
            List<string> layerCodes,
            string modelManifest) : base(buildLogger, timeoutMinutes, configurationFile)
        {
            LayerCodes = layerCodes;
            ModelManifest = modelManifest;
        }

        public ClientBuildTask()
        {
        }

        abstract public Process Start();
        abstract public void End(IBuildLogger buildLogger, AxaptaAutoRun autoRun);

        public void Cleanup(Process process, string logFile, string autoRunFile)
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }

            try
            {
                File.Delete(logFile);
            }
            catch (FileNotFoundException)
            {
            }

            try
            {
                File.Delete(autoRunFile);
            }
            catch (FileNotFoundException)
            {
            }
        }
    }
}
