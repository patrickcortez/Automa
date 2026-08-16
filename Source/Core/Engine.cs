using System.Collections;
using System.Text;
using static Automa.Source.Utility.Utils;

namespace Automa.Source.Core
{
    internal class Engine(string path,bool isdebug=false) // Lexer
    {


        /*
         * 
         * Line 1: Write("Hello World")
         * Line 2: var = Read("Tacticus")
         * Line 3: Write("Hello $var")
         *  
         * List Structure:
         * [0] -> TokenInstruction , TokenLParen, TokenString, TokenRParen
         * [1] -> TokenIdentifier , TokenInstruction,RParen,LParen,TokenString
         * [] ... And so on....
         */

        // TODO: Add Arithmetic Engine
        // Plus,Parenthesis Depth and minus
        // Multiplication and Division is up-to the users to create using upcoming while-loop and functions.


        private List<List<LexerToken>> _Tokenize() // To be continued...
        {
            List<List<LexerToken>> Tokens = new(); // TODO: Finisher Lexer

            using StreamReader Reader = new(path);

            string line = "";

            while((line = Reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (line.StartsWith('#'))
                {
                    continue;
                }

                StringBuilder Token = new();
                bool isInQoutes = false;

                foreach (char current in line) // Tokenize our Line
                {
                    if (current == '#' && !isInQoutes) // Comments
                    {
                        break;
                    }

                    if(current == ' ' && !isInQoutes) // whitespace
                    {
                        continue;
                    }

                    Token.Append(current);
                }
            }

            return Tokens;
        }

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

                if (line.StartsWith('#')) // Comments '#' 
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

                Parser parse = new(Tokenize()); // TODO: Update Parser to recieve LexerTokens.
               

                return  parse.Start();
            }
            catch(Exception ex)
            {
                Print("Error while Tokenizing File", ex,new(PrintOptions.Error,true));
                return 1;
            }
        }
    }
}
