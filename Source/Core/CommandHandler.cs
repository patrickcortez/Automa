using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class CommandHandler(string _cmd, string[]? _args = null)
    {
        private Dictionary<string, Func<string[], int>> Commands = new() // Dict of string and Func Delegates
        {
            { "help",  (string[] args) => { // Help of user convenience and a cli guide

                Print("Automa SubCommands:");
                Print("Flags:");
                Print("[-d : enables debug]");
                Print("version          - Displays current version of Automa");
                Print("run <.auto>      - Runs a Automa script");
                Print("help             - Displays this message");
                return 0;

                }
            },
            {
                "run", (string[] args) => { // Run script;
                    string flag = (args.Length > 1)? args[1] : ""; 
                    Engine engine = new(args[0],(flag=="-d")? true : false); 
                    return engine.Start(); 
                }
            },
            {

                "version", (string[] args) => { Print("Automa 0.1.0"); return 0;  } // Current Version of Automa
            }

        };


        private async Task<int> Exitc(int code)
        {
            await Task.Delay(1000);
            return code;
        }

        public int Start()
        {
            try
            {

                if (!Commands.ContainsKey(_cmd)) // Guard clause
                {
                    Console.Error.WriteLine("Command {0} is not a command", _cmd);
                    return 1;
                }

                if(_args is null)
                {
                    _args = [ "" ]; // populate with atleast one, \(0_0)/
                }

                Commands[_cmd](_args);

                return 0;
            }catch(Exception ex)
            {
                Console.Error.WriteLine("Command {0} threw an exeption of {1}", _cmd, ex);
                return 1;
            }
        }
    }
}
