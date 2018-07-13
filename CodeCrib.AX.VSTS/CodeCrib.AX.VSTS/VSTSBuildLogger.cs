using CodeCrib.AX.BuildTasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.VSTS
{
    [Serializable]
    public class VSTSBuildLogger : IBuildLogger
    {
        public void LogError(string message)
        {
            Console.WriteLine("##vso[task.logissue type=error;]{0}", message);
        }

        public void LogInformation(string message)
        {
            Console.WriteLine(message);
        }

        public void LogWarning(string message)
        {
            Console.WriteLine("##vso[task.logissue type=warning;]{0}", message);
        }
        protected VSTSBuildLogger()
        {
        }

        public static VSTSBuildLogger CreateDefault()
        {
            return new VSTSBuildLogger();
        }

        public void StoreLogFile(string logFilePath)
        {
            Console.WriteLine("##vso[task.uploadfile]{0}", logFilePath);
        }
    }
}
