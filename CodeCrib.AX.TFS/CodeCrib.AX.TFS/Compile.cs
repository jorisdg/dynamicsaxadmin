//This C# code file was released under the Ms-PL license
//http://www.opensource.org/licenses/ms-pl.html
//This script was originally intended for use with Microsoft Dynamics AX
//and maintained and distributed as a project on CodePlex
//http://dynamicsaxadmin.codeplex.com

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Activities;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System.IO;
using System.Diagnostics;
using CodeCrib.AX.BuildTasks;

namespace CodeCrib.AX.TFS
{
    [BuildActivity(HostEnvironmentOption.Agent)]
    public class Compile : AsyncCodeActivity
    {
        public InArgument<string> ClientExecutablePath { get; set; }

        public InArgument<bool> UpdateCrossReference { get; set; }
        public InArgument<int> TimeOutMinutes { get; set; }

        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<StringList> LayerCodes { get; set; }
        public InArgument<string> ModelManifestFile { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            bool updateXRef = UpdateCrossReference.Get(context);
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string clientExePath = ClientExecutablePath.Get(context);
            StringList layerCodes = LayerCodes.Get(context);
            string configurationFile = ConfigurationFile.Get(context);
            string modelManifest = ModelManifestFile.Get(context);

            ClientCompileTask task = new ClientCompileTask(context.DefaultLogger(), timeOutMinutes, configurationFile, layerCodes, modelManifest, clientExePath, updateXRef);
            Process process = task.Start();

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, CancelableBuildTask = task };
            return processWaitDelegate.BeginInvoke(process.Id, timeOutMinutes, callback, state);
        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {

            if (context.UserState is CommandContext userState && 
                userState.CancelableBuildTask != null)
            {
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {

            if (context.UserState is CommandContext userState && 
                userState.CancelableBuildTask != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                {
                    throw processWaitException;
                }

                userState.CancelableBuildTask.End();
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }
        }
    }

    [BuildActivity(HostEnvironmentOption.Agent)]
    public class CompileNodes : AsyncCodeActivity
    {
        public InArgument<string> ClientExecutablePath { get; set; }

        public InArgument<bool> UpdateCrossReference { get; set; }
        public InArgument<int> TimeOutMinutes { get; set; }

        public InArgument<string> ConfigurationFile { get; set; }

        public InArgument<StringList> LayerCodes { get; set; }
        public InArgument<string> ModelManifestFile { get; set; }

        [RequiredArgument]
        public InArgument<StringList> AOTCompilePaths { get; set; }

        protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
        {
            bool updateXRef = UpdateCrossReference.Get(context);
            int timeOutMinutes = TimeOutMinutes.Get(context);
            string clientExePath = ClientExecutablePath.Get(context);
            string configurationFile = ConfigurationFile.Get(context);
            StringList aotCompilePaths = AOTCompilePaths.Get(context);
            StringList layerCodes = LayerCodes.Get(context);
            string modelManifest = ModelManifestFile.Get(context);

            ClientCompileNodesTask task = new ClientCompileNodesTask(context.DefaultLogger(), timeOutMinutes, configurationFile, layerCodes, modelManifest, clientExePath, aotCompilePaths);
            Process process = task.Start();

            Func<int, int, Exception> processWaitDelegate = new Func<int, int, Exception>(CommandContext.WaitForProcess);
            context.UserState = new CommandContext { Delegate = processWaitDelegate, Process = process, CancelableBuildTask = task };
            return processWaitDelegate.BeginInvoke(process.Id, timeOutMinutes, callback, state);

        }

        protected override void Cancel(AsyncCodeActivityContext context)
        {

            if (context.UserState is CommandContext userState &&
                userState.CancelableBuildTask != null)
            {
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }

            if (context.IsCancellationRequested)
            {
                context.MarkCanceled();
            }
        }

        protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
        {

            if (context.UserState is CommandContext userState &&
                userState.CancelableBuildTask != null)
            {
                Func<int, int, Exception> processWaitDelegate = userState.Delegate;
                Exception processWaitException = processWaitDelegate.EndInvoke(result);

                if (processWaitException != null)
                {
                    throw processWaitException;
                }

                userState.CancelableBuildTask.End();
                userState.CancelableBuildTask.Cleanup(userState.Process);
            }
        }

    }
}
