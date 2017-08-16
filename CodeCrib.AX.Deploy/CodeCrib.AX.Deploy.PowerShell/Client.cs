using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace CodeCrib.AX.Deploy.PowerShell
{
    [Cmdlet(VerbsData.Sync, "DataDictionary")]
    public class Sync_DataDictionary : PSCmdlet
    {
        [Parameter(HelpMessage = "Time-out in minutes", Mandatory = false)]
        public int timeoutMinutes;

        [Parameter(HelpMessage = "Client configuration file", Mandatory = false)]
        public string clientConfigFile;

        [Parameter(HelpMessage = "Log file", Mandatory = false)]
        public string logFile;


        protected override void ProcessRecord()
        {
            Guid guid = Guid.NewGuid();

            if (string.IsNullOrEmpty(logFile))
            {
                logFile = string.Format(@"{0}\SynchronizeLog-{1}.xml", Environment.GetEnvironmentVariable("temp"), guid);
            }

            CodeCrib.AX.Client.AutoRun.AxaptaAutoRun autoRun = new CodeCrib.AX.Client.AutoRun.AxaptaAutoRun() { ExitWhenDone = true, LogFile = logFile };
            autoRun.Steps.Add(new CodeCrib.AX.Client.AutoRun.Synchronize() { SyncDB = true, SyncRoles = true });

            string autoRunFile = string.Format(@"{0}\AutoRun-Synchronize-{1}.xml", Environment.GetEnvironmentVariable("temp"), guid);
            CodeCrib.AX.Client.AutoRun.AxaptaAutoRun.SerializeAutoRun(autoRun, autoRunFile);

            //Process process = CodeCrib.AX.Client.Client.StartCommand(new CodeCrib.AX.Client.Commands.AutoRun() { ConfigurationFile = clientConfigFile, Filename = autoRunFile });

            CodeCrib.AX.Client.Client.ExecuteCommand(new CodeCrib.AX.Client.Commands.AutoRun() { ConfigurationFile = clientConfigFile, Filename = autoRunFile }, timeoutMinutes);

            //AutoRunLogOutput.Output(context, autoRun, true);

            //if (System.IO.File.Exists(autoRun.LogFile))
            //    System.IO.File.Delete(autoRun.LogFile);

            System.IO.File.Delete(autoRunFile);
        }
    }
}
