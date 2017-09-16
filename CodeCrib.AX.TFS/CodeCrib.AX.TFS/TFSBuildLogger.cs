using CodeCrib.AX.BuildTasks;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.TFS
{
    internal class TFSBuildLogger : IBuildLogger
    {
        private CodeActivityContext context;

        private TFSBuildLogger(CodeActivityContext context)
        {
            this.context = context;
        }

        public static TFSBuildLogger NewStandard(CodeActivityContext context)
        {
            return new TFSBuildLogger(context);
        }
        public void LogError(string message)
        {
            context.TrackBuildError(message);
        }

        public void LogInformation(string message)
        {
            context.TrackBuildMessage(message);
        }

        public void LogWarning(string message)
        {
            context.TrackBuildWarning(message);
        }
    }
}
