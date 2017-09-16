using CodeCrib.AX.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    public class CommandContext
    {
        public Func<int, int, Exception> Delegate { get; set; }
        public Process Process { get; set; }
        public Client.AutoRun.AxaptaAutoRun AutoRun { get; set; }
        public string AutoRunFile { get; set; }
        public string LogFile { get; set; }

        public static Exception WaitForProcess(int processId, int timeOutMinutes)
        {
            Exception returnException = null;

            if (processId != 0)
            {
                Process process = Process.GetProcessById(processId);

                if (timeOutMinutes > 0)
                {
                    if (!process.WaitForExit((int)new TimeSpan(0, timeOutMinutes, 0).TotalMilliseconds))
                    {
                        // Process is still running after the timeout has elapsed.

                        try
                        {
                            process.Kill();
                            returnException = new TimeoutException(string.Format("Client time out of {0} minutes exceeded", timeOutMinutes));
                        }
                        catch (Exception ex)
                        {
                            // Error trying to kill the process
                            returnException = new TimeoutException(string.Format("Client time out of {0} minutes exceeded, additionally an exception was encountered while trying to kill the client process (see innerexception)", timeOutMinutes), ex);
                        }
                    }
                }
                else
                {
                    process.WaitForExit();
                }
            }

            return returnException;
        }
    }
}
