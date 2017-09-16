using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    public class AutoRunLogOutput
    {
        protected static void OutputLog(IBuildLogger buildLogger, List<KeyValuePair<string, string>> log, bool outputAllAsInfo = false)
        {
            if (log != null)
            {
                foreach (var message in log)
                {
                    if (outputAllAsInfo)
                    {
                        buildLogger.LogInformation(message.Value);
                    }
                    else
                    {
                        switch (message.Key)
                        {
                            case "Info":
                                buildLogger.LogInformation(message.Value);
                                break;
                            case "Warning":
                                buildLogger.LogWarning(message.Value);
                                break;
                            case "Error":
                                buildLogger.LogError(message.Value);
                                break;
                        }
                    }
                }
            }
        }

        public static void Output(IBuildLogger buildLogger, Client.AutoRun.AxaptaAutoRun autoRun, bool outputAllAsInfo = false, bool skipInfoLog = false)
        {
            autoRun.ParseLog();

            //OutputLog(context, autoRun.Log, outputAllAsInfo);

            foreach (var step in autoRun.Steps)
            {
                OutputLog(buildLogger, step.Log, outputAllAsInfo);
            }

            if (!skipInfoLog && !string.IsNullOrEmpty(autoRun.InfoLog))
            {
                var lines = autoRun.InfoLog.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (string line in lines)
                {
                    buildLogger.LogInformation(line.Trim());
                }
            }
        }
    }
}
