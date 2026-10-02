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
                Print("clear            - clears terminal screen");
                return 0;

                }
            },
            {
                "run", (string[] args) => { // Run script;
                    string file = args[0];
                    string flag = (args.Length > 1)? args[1] : "";

                    if (!Path.Exists(file)) // check file existence
                    {
                        Console.Error.WriteLine($"{file} does not exist!");
                        return 2;
                    }

                    if (Path.GetExtension(file)!=".auto")
                    {
                        throw new Exception($"{file} is not an auto file. The file must have a '.auto' file extension!");
                    }

                    Engine engine = new(file,(flag=="-d")? true : false); // run script with debug checking
                    return engine.Start(); 
                }
            },
            {

                "version", (string[] args) => { Print("Automa 0.8.0"); return 0;  } // Current Version of Automa
            },
            {
                "clear",(string[] args) => {Console.Clear(); return 0; }
            }

        };

        public int Start()
        {
            try
            {

                if (!Commands.ContainsKey(_cmd)) // Guard clause
                {
                    //Console.Error.WriteLine("Command {0} is not a command", _cmd);
                    return 2;
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
