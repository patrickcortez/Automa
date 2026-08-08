using Automa.Source.Core;

namespace Automa.Source;

// A simple Automation Language made by Tezzz =D

public static class Automa
{
    public static int Main(string[] args)
    {
        try
        {
            if(args.Length < 1)
            {
                CommandHandler run = new(_cmd: "help");
                run.Start();
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