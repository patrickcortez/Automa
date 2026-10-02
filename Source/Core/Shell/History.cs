using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Core.Shell
{
    internal static class History
    {
        public static List<string> prev = new();

        public static void AddHistory(string command)
        {
            prev.Add(command);
        }
    }
}
