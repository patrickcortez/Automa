using Automa.Source;
using System;
using System.Collections.Generic;
using System.Text;

// Automa Arithmetic Handler
// Addition and divisiion (for now)

// TODO:
// Implement Brace Depth Handling. (Recursive Descent)
// Implement Multiplication and Division

namespace Automa.Source.Core
{
    internal class Arithmetic(IEnumerable<LexerToken> Tokens,bool isdebug=false)
    {

        public ArithmeticNode? ParseArithmetic(out int TokensConsumed,IEnumerable<LexerToken>? Starting = null)
        {
            ArithmeticNode? left = null;
            char pendingop = '+';

            void Push(ArithmeticNode operand) =>
                left = left is null ? operand : new BinaryOpNode(left, pendingop, operand);

            bool inParen = false;
            int pd = 0;

            LexerToken[] tokens = (Starting is not null)? Starting.ToArray() : Tokens.ToArray();

            int tc = 0;

            if (isdebug)
            {
                Console.WriteLine("[DEBUG] Parsing Arithmetic");
            }

            for(int i = 0; i < tokens.Count(); i++)
            {

                

                LexerToken token = tokens[i];
                LexerToken? next = null;
                LexerType tokentype = token.TokenType;
                string content = token.GetContent();

                int Peek = i + 1;
                tc++;

                if(Peek < tokens.Count())
                {
                    next = tokens[Peek];
                }

                if (isdebug)
                {
                    Console.WriteLine($"[DEBUG] Current Type: {tokentype} , Value: {content}");
                }

                if (tokentype is LexerType.TokenInt) //123
                {
                    int val = int.Parse(content);

                    if(left is null)
                    {
                        Push(new NumberNode(val));
                    }
                    else
                    {
                        if(next is not null) // parse the next operations
                        {
                            int skip = i + 1;
                            Push(ParseArithmetic(out int tskips, tokens.Skip(skip)));
                            i += tskips;
                            continue;
                        }

                        Push(new NumberNode(val));
                    }


                    continue;
                }else if(tokentype is LexerType.Token_Add or LexerType.Token_Minus) // +,-,* or /
                {

                    if(left is null)
                    {
                        throw new Exception("Cannot start the arithmetic expression with a operator!");
                    }

                    if (tokentype is LexerType.Token_Minus)
                    {
                        pendingop = '-';
                    }
                    else if(tokentype is LexerType.Token_Multiply)
                    {

                        pendingop = '*';
                    }
                    else if(tokentype is LexerType.Token_Divide)
                    {
                        pendingop = '/';
                    }
                    else if(tokentype is LexerType.Token_Add)
                    {
                        pendingop = '+';
                    }

                    continue;
                }else if(tokentype is LexerType.Token_LParen) // (
                {

                    if (!inParen && pd ==0)
                    {
                        inParen = true;
                    }

                    int skip = i + 1;
                    Push(ParseArithmetic(out int tskip, tokens.Skip(skip)));
                    i += tskip;

                    continue;
                }else if(tokentype is LexerType.Token_RParen) // )
                {
                    if (inParen)
                    {
                        inParen = false;
                        break;
                    }
                    else
                    {
                        throw new InvalidOperationException("Missing L brace!");
                    }
                }else if(tokentype is LexerType.Token_Identifier) // variable handling (scope based)
                {

                    if(left is null)
                    {
                        Push(new VariableNode(content));
                    }
                    else
                    {

                        if (next is not null) // parse the next operations
                        {
                            int skip = i + 1;
                            Push(ParseArithmetic(out int tskip, tokens.Skip(skip)));

                            i += tskip;
                            continue;
                        }

                        Push(new VariableNode(content));
                    }
                }
            }

            TokensConsumed = tc;

            return left;
        }

    }
}
