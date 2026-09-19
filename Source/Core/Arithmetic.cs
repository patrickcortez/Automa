using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Core
{
    internal class Arithmetic(IEnumerable<LexerToken> Tokens)
    {

        public ArithmeticNode? ParseArithmetic(IEnumerable<Variable>? Variables = null)
        {
            BinaryOpNode? Start = null;

            foreach (LexerToken token in Tokens)
            {
                LexerType tokentype = token.TokenType;
                string content = token.GetContent();
                bool inParen = false;
                int pd = 0;

                if(tokentype is LexerType.TokenInt) //123
                {
                    int val = int.Parse(content);
                    if(Start is null)
                    {
                        Start = new BinaryOpNode(new NumberNode(val), '+', new NumberNode(0));
                        continue;
                    }
                    else
                    {
                        Start = Start with { right = new NumberNode(val) };
                    }


                    continue;
                }else if(tokentype is LexerType.Token_Add or LexerType.Token_Minus) // + or -
                {
                    if(tokentype is LexerType.Token_Minus)
                    {
                        if(Start is not null)
                        {
                            Start = Start with { Op = '-' };
                        }


                        throw new Exception("Cannot assign + or - as first arithmetic token");
                    }

                    continue;
                }else if(tokentype is LexerType.Token_LParen) // (
                {

                    if (!inParen && pd ==0)
                    {
                        inParen = true;
                    }
                    else
                    {
                        pd++;
                    }

                    continue;
                }else if(tokentype is LexerType.Token_RParen) // )
                {
                    if (inParen)
                    {
                        pd--;
                    }
                }
            }

            return Start;
        }

    }
}
