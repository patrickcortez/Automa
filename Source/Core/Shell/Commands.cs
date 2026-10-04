using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Core.Shell
{
    internal class Commands(string Command)
    {



        public async Task<int> Start()
        {
            string[] arg = Command.Split(' ');
            string cmd = arg[0].ToLower();
            string[] ext = arg.Skip(1).ToArray();

            CommandHandler handler = new(cmd, ext);

            int exit = handler.Start();

            if(exit is 2)
            {
                string tmpfile = "tmp.auto";

                using (StreamWriter write = new(tmpfile, false))
                {
                    await write.WriteLineAsync(Command);
                }

                Engine Lexer = new(tmpfile);

                exit =  Lexer.Start();

                File.Delete(tmpfile);
            }

            return exit;
        }


    }
}
