using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.TFS
{
    public static class ContextExtensions
    {
        internal static TFSBuildLogger DefaultLogger(this CodeActivityContext context)
        {
            return TFSBuildLogger.NewStandard(context);
        }
    }
}
