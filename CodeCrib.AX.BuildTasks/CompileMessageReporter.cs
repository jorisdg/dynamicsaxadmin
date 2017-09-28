using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildTasks
{
    public class CompileMessageReporter
    {
        public static void ReportCompileMessages(IBuildLogger buildLogger, Client.CompileOutput output)
        {
            bool hasErrors = false;
            foreach (var item in output.Output)
            {
                string compileMessage = String.Format("{0}, line {1}, column {2} : {3}", item.TreeNodePath, item.LineNumber, item.ColumnNumber, item.Message);
                switch (item.Severity)
                {
                    // Compile Errors
                    case 0:
                        buildLogger.LogError(compileMessage);
                        hasErrors = true;
                        break;
                    // Compile Warnings
                    case 1:
                    case 2:
                    case 3:
                        buildLogger.LogWarning(compileMessage);
                        break;
                    // Best practices
                    case 4:
                        buildLogger.LogInformation(string.Format("BP: {0}", compileMessage));
                        break;
                    // TODOs
                    case 254:
                    case 255:
                        buildLogger.LogWarning(string.Format("TODO: {0}", compileMessage));
                        break;
                    // "Other"
                    default:
                        buildLogger.LogInformation(compileMessage);
                        break;
                }
            }
            if (hasErrors)
            {
                throw new Exception("Compile error(s) found");
            }
        }
    }
}
