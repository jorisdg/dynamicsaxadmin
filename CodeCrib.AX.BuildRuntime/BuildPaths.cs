using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCrib.AX.BuildRuntime
{
    public static class BuildPaths
    {
        private static bool _initialized;
        private static string _temp;

        public static string Temp
        {
            get
            {
                if (!_initialized)
                {
                    string buildWorkingPath = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");
                    if (!string.IsNullOrEmpty(buildWorkingPath))
                    {
                        _temp = Path.Combine(buildWorkingPath, string.Format("temp_{0}", Guid.NewGuid()));
                        Directory.CreateDirectory(_temp);
                    }
                    else
                    {
                        _temp = Path.GetTempPath();
                    }
                    _initialized = true;
                }

                return _temp;
            }
        }
    }
}
