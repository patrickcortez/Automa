using Automa.Source.Core;
using Automa.Source.Core.Shell;

namespace Automa.Source;

// A simple Automation Language made by Tezzz =D


public static class Automa
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            Console.TreatControlCAsInput = true;

            if (args.Length < 1)
            {
                Shell shell = new();
                await shell.Start();
                return 0;
            }
            
            string cmd = args[0].ToLower();

            string[] _args = args.Skip(1).ToArray();

            CommandHandler command = new(_cmd: cmd,_args: _args);
            


            return command.Start();

        }catch(Exception ex)
        {
            Console.Error.WriteLine("Encountered an Error while running: {0}", ex);
            return 1;
        }
    }
}