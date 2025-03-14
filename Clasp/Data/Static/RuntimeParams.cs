using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clasp.Data.Static
{
    public static class RuntimeParams
    {
        public const string FILE_EXT = "clsp";

        public static string LIBRARY_REPO_DIR = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\source\repos\Clasp\ClaspDev");
    }
}
