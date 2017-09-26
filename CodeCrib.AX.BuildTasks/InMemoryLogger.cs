using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    [Serializable]
    public class InMemoryLogger : IBuildLogger
    {
        public List<Tuple<BuildLogSeverity, string>> LogMessages { get; set; } = new List<Tuple<BuildLogSeverity, string>>();

        public void LogError(string message)
        {
            LogMessages.Add(new Tuple<BuildLogSeverity, string>(BuildLogSeverity.Error, message));
        }

        public void LogInformation(string message)
        {
            LogMessages.Add(new Tuple<BuildLogSeverity, string>(BuildLogSeverity.Information, message));
        }

        public void LogWarning(string message)
        {
            LogMessages.Add(new Tuple<BuildLogSeverity, string>(BuildLogSeverity.Warning, message));
        }
    }
}
