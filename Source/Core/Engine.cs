using System.Collections;
using System.Text;
using System.Transactions;
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
         * [] ... And so on ...
         */

        // TODO: Add Arithmetic Engine
        // Plus,Parenthesis Depth and minus
        // Multiplication and Division is up-to the users to create using upcoming while-loop and functions.

        private LexerToken[]? _Tokenize() // Lexer & Tokenizer
        {
            try
            {
                List<LexerToken> Tokens = new();

                using StreamReader Reader = new(path);
                string line = "";
                int LineNo = 0;

                while ((line = Reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        LineNo++;
                        continue;
                    }

                    if (line.StartsWith('#'))
                    {
                        LineNo++;
                        continue;
                    }

                    StringBuilder Value = new(),identifier = new();
                    bool isInQoutes = false;

                    foreach (char c in line)
                    {
                        // Qoute Checking
                        if (c is '"')
                        {
                            isInQoutes = !isInQoutes;
                            continue;
                        }

                        // integer handling
                        if (char.IsDigit(c) && !isInQoutes && identifier.Length == 0) // if its a integer
                        {
                            string val = Value.ToString() ?? string.Empty;

                            if (val != string.Empty && !char.IsNumber(val[val.Length - 1]))
                            {
                                throw new Exception("Invalid assignment: Cannot assignment integer next to a string literal");
                            }

                            Value.Append(c);
                            continue;
                        }

                        // Identifier Handling
                        if ((char.IsLetterOrDigit(c) || c == '_') && !isInQoutes) // store all character literals 
                        {
                            if(Value.Length > 0) // if the previous letters are integers then just append the previous to current,
                            {
                                identifier.Append(Value);
                                Value.Clear(); // erase previous since its a identifier
                            }
                            identifier.Append(c);
                            continue;
                        }else if(!(char.IsLetterOrDigit(c) || c == '_') && !isInQoutes) // store at '_', '|' , '&' and etc....
                        {
                            if(identifier.Length > 0) // always check if ident is not empty
                            {
                                Tokens.Add(new(LexerType.Token_Identifier, LineNo, identifier.ToString()));
                                identifier.Clear();
                            }
                        }

                        // Comments, Tab and Spaces handling
                        if (!isInQoutes)
                        {
                            if (c is ' ' or '\t')
                            {
                                continue;
                            }

                            if (c is '#')
                            {
                                break;
                            }
                        }



                        // Special-Characters
                        if (!isInQoutes)
                        {
                            if (c is ';')
                            {

                                if (Value.Length > 0)
                                {
                                    string Val = Value.ToString();
                                    if(int.TryParse(Val,out int value)) // if value is int
                                    {
                                        Tokens.Add(new(LexerType.TokenInt, LineNo, Val));
                                        Value.Clear();
                                    }
                                    else // string
                                    {
                                        Tokens.Add(new(LexerType.TokenString, LineNo, Value.ToString()));
                                        Value.Clear();
                                    }


                                }

                                Tokens.Add(new(LexerType.Token_SemiColon, LineNo));
                                continue;
                            }
                            else if (c is '(')
                            {
                                Tokens.Add(new(LexerType.Token_LParen, LineNo));
                                continue;
                            }
                            else if (c is ')')
                            {
                                if(Value.Length > 0)
                                {
                                    string val = Value.ToString();

                                    if(int.TryParse(val,out int num))
                                    {
                                        Tokens.Add(new(LexerType.TokenInt, LineNo,val));
                                    }
                                    else
                                    {
                                        Tokens.Add(new(LexerType.TokenString, LineNo, val));
                                    }
                                    Value.Clear();
                                }

                                Tokens.Add(new(LexerType.Token_RParen, LineNo));
                                continue;
                            }
                            else if (c is '{')
                            {
                                Tokens.Add(new(LexerType.Token_LBrace, LineNo));
                                continue;
                            }
                            else if (c is '}')
                            {
                                Tokens.Add(new(LexerType.Token_RBrace, LineNo));
                                continue;
                            }
                            else if (c is '=')
                            {
                                if(Value.Length > 0)
                                {
                                    if (int.TryParse(Value.ToString(),out int val)) // make sure the left side is not a string or int literal
                                    {
                                        throw new Exception("Cannot assign a value to a integer literal");
                                    }

                                    throw new Exception("Cannot assign a value to a string literal");
                                }

                                Tokens.Add(new(LexerType.Token_Equal, LineNo));
                                continue;
                            }
                            else if (c is '+')
                            {
                                Tokens.Add(new(LexerType.Token_Add, LineNo));
                                continue;
                            }
                            else if (c is '-')
                            {
                                Tokens.Add(new(LexerType.Token_Minus, LineNo));
                                continue;
                            }else if(c is '!')
                            {
                                Tokens.Add(new(LexerType.Token_Not, LineNo));
                                continue;
                            }
                        }
                        else if (isInQoutes) // String Literal
                        {
                            Value.Append(c);
                        }


                    }

                    if (Tokens.Count > 0)
                    {
                        var lasttoken = Tokens[Tokens.Count - 1];

                        if (lasttoken.Line == LineNo)
                        {
                            if (lasttoken.TokenType != LexerType.Token_SemiColon && lasttoken.TokenType != LexerType.Token_LBrace && lasttoken.TokenType != LexerType.Token_RBrace)
                            {
                                throw new Exception($"Missing ';' in line: {LineNo}");
                            }
                        }
                    }

                    LineNo++;
                }

                return Tokens.ToArray();
            }catch(Exception ex)
            {
                Console.WriteLine("Lexer Error: {0}", ex);
                return null;
            }
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

                LexerToken[] toks = _Tokenize() ?? [];
                
                if(isdebug && toks.Length > 0)
                {
                    Console.WriteLine("[Debug] Token Count: {0}", toks.Count());
                }

                Parser parse = new(toks ?? throw new Exception("Lexer Error: Empty Tokens!"),isdebug);
               

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
