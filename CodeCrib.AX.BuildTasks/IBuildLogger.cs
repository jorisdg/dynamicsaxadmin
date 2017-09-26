using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    public interface IBuildLogger
    {
        void LogInformation(string message);
        void LogWarning(string message);
        void LogError(string message);

        void StoreLogFile(string logFilePath);
    }
}
