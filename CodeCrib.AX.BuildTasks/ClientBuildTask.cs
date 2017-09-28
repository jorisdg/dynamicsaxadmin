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
    [Serializable]
    abstract public class ClientBuildTask : CancelableBuildTask
    {
        public List<string> LayerCodes { get; set; }
        public string ModelManifest { get; set; }

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

        public ClientBuildTask(IBuildLogger buildLogger, int timeoutMinutes, string configurationFile) : base(buildLogger, timeoutMinutes, configurationFile)
        {
        }

        public ClientBuildTask()
        {
        }

        public override void Run()
        {
            Process process = Start();

            Exception executionException = CommandContext.WaitForProcess(process.Id, TimeoutMinutes);
            if (executionException != null)
            {
                throw executionException;
            }

            End();
            Cleanup(process);
        }

        public override void Cleanup(Process process)
        {
            if (process != null && !process.HasExited)
            {
                process.Kill();
            }
        }
    }
}
