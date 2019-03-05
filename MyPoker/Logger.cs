using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPoker
{
    static class Logger
    {
        private static readonly StringBuilder StringLogs = new StringBuilder();

        public static void Write(string logs)
        {
            StringLogs.AppendLine(logs);
        }
        public static string Read() =>
            StringLogs.ToString();

    }
}
