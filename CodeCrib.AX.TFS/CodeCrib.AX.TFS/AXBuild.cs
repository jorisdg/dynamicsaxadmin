//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System;
using System.Activities;
using System.IO;
using System.Diagnostics;
using System.Management;
using CodeCrib.AX.BuildTasks;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class AXBuildCompile : AsyncCodeActivity
    {
        public InArgument<int> TimeOutMinutes { get; set; }
        public InArgument<int> Workers { get; set; }
        public InArgument<string> ConfigurationFile { get; set; }
        public InArgument<string> AlternateBinaryFolder { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);
            string altBin = AlternateBinaryFolder.Get(context);

            int workers = Workers.Get(context);
            AxBuildTask axBuildTask = new AxBuildTask(new NullBuildLogger(), timeOutMinutes, configurationFile, workers, altBin);
            Process process = axBuildTask.Start();

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, CancelableBuildTask = axBuildTask };
            return processWaitDelegate.BeginInvoke(process.Id, timeOutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {

            if (context.UserState is CommandContext userState)
            {
                CancelableBuildTask task = userState.RetrieveBuildTask(context.DefaultLogger());
                if (task != null)
                {
                    task.Cleanup(userState.Process);
                }
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {
            if (context.UserState is CommandContext userState)
            {
                CancelableBuildTask task = userState.RetrieveBuildTask(context.DefaultLogger());
                if (task != null)
                {
                    task.End();
                    task.Cleanup(userState.Process);
                }
            }
        }
    }    
}
