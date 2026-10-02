using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Automa.Source.Core.IO
{
    // Automa Output handler
    internal static class OutputHandler
    {

        public static void Out(string msg,byte ErrorLevel=0)    // Simple output with no newline
        {
            Stream Output = Console.OpenStandardOutput(msg.Length);
            string Error = "";

            if(ErrorLevel is 1)  //Warning
            {
                Error = "\e[0;33m";
            }else if(ErrorLevel is 2)
            {
                Error = "\e[0;31m";
            }

            string formatted = Regex.Unescape(Error+msg); // format all the escape codes

            

            byte[] Data = Encoding.UTF8.GetBytes(formatted);
            Output.Write(Data, 0, Data.Length);
        }

    }
}
