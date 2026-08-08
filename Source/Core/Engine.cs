using System;
using System.Collections.Generic;
using System.Text;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Engine(string path,bool isdebug=false)
    {
        private string[] Tokenize()
        {
            List<string> Tokens = new();

             using StreamReader Reader = new(path);

            string line = "";
            while ((line = Reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                Tokens.Add(line);
            }

            return Tokens.ToArray();
        }

        public int Start()
        {
            try
            {
                if (!Path.Exists(path))
                {
                    Print($"File {path} does not exist", new(PrintOptions.Error, true));
                    return 1;
                }

                if(Path.GetExtension(path) != ".auto")
                {
                    Print($"File {path} is not a Automa Script", new(PrintOptions.Error, true));
                    return 1;
                }

                Parser parse = new(Tokenize());
               

                return parse.Start();
            }
            catch(Exception ex)
            {
                Print("Error while Tokenizing File", ex,new(PrintOptions.Error,true));
                return 1;
            }
        }
    }
}
