using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.Build.Workflow.Activities;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.TFS
{
    class Helper
    {
        public static void ReportCompileMessages(CodeActivityContext context, CodeCrib.AX.Client.CompileOutput output)
        {
            bool hasErrors = false;
            foreach (var item in output.Output)
            {
                string compileMessage = String.Format("{0}, line {1}, column {2} : {3}", item.TreeNodePath, item.LineNumber, item.ColumnNumber, item.Message);
                switch (item.Severity)
                {
                    // Compile Errors
                    case 0:
                        context.TrackBuildError(compileMessage);
                        hasErrors = true;
                        break;
                    // Compile Warnings
                    case 1:
                    case 2:
                    case 3:
                        context.TrackBuildWarning(compileMessage);
                        break;
                    // Best practices
                    case 4:
                        context.TrackBuildMessage(string.Format("BP: {0}", compileMessage), BuildMessageImportance.Low);
                        break;
                    // TODOs
                    case 254:
                    case 255:
                        context.TrackBuildWarning(string.Format("TODO: {0}", compileMessage));
                        break;
                    // "Other"
                    default:
                        context.TrackBuildMessage(compileMessage);
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
