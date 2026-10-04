using Automa.Source.Core.IO;

namespace Automa.Source.Core.Shell
{
    internal class Shell
    {


        private void Print()
        {
            OutputHandler.Out("\n\e[0;33m>> \e[0m");
        }

        private void PrintBanner()
        {
            string banner = "\e[0;96mAutoma\e[0m CLI v0.9.0\nType \"\e[1;36mhelp\e[0m\" to get acquainted";
            Console.WriteLine(banner + Environment.NewLine);
        }

        public async Task<int> Start(bool isVerbose = false)
        {
            PrintBanner();
            while (true)
            {

                Print();
                string? input = ((input = InputHandler.ReadLine()) is not null) ? input : null;

                if (input is null)
                {
                    continue;
                }

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                Commands comms = new(input);

                int exit = await comms.Start();
                FunctionTable.Purge(); // Erase existing funtions after input is parsed

                if (exit != 0)
                {
                    Console.WriteLine("Command: {0} , failed");
                }

                History.AddHistory(input);

            }

            return 0;

        }

    }
}
