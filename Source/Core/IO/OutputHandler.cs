using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Core.IO
{
    // Automa Output handler
    internal static class OutputHandler
    {

        public static void Out(string msg,byte ErrorLevel=0)    // Simple output with no newline
        {
            Stream Output = Console.OpenStandardOutput(msg.Length);

            byte[] Data = Encoding.UTF8.GetBytes(msg);
            Output.Write(Data, 0, Data.Length);
        }

    }
}
