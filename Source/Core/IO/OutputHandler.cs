using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Automa.Source.Core.IO
{
    // Automa Output handler
    internal static class OutputHandler
    {

         private static string TolerateString(string str){

            return Regex.Replace(str, @"\\(.)", match =>
            {
                return match.Groups[1].Value switch {
                    "n" => "\n", // newline
                    "t" => "\t", // horizonal tab
                    "\\"=> "\\", // backslash
                    "\""=> "\"", // qoute
                    "'" => "'", // single character
                    "e" => "\e", // escape code
                    "0" => "\0", // null
                    "a" => "\a", // alert
                    "r" => "\r", // carriage return
                    "b" => "\b", // backspace
                    "f" => "\f", // form feed
                    "v" => "\v", // vertical tab
                    _ => match.Value
                };
            });
            
        }
        
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
            
            
            string formatted = TolerateString(Error+msg); // format all the escape codes     

            byte[] Data = Encoding.UTF8.GetBytes(formatted);
            Output.Write(Data, 0, Data.Length);
        }

    }
}
