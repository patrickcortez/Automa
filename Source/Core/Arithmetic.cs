using Automa.Source.Definitions;
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


        // Arithmetic handler
        public ArithmeticNode? ParseArithmetic(out int TokensConsumed, IEnumerable<LexerToken>? Starting = null)
        {
            LexerToken[] tokens = (Starting is not null) ? Starting.ToArray() : Tokens.ToArray(); // pass arithmetic tokens
            int pos = 0; // tracker

            if (isdebug)
            {
                Console.WriteLine("[DEBUG] Parsing Arithmetic ({0} tokens)", tokens.Length);
            }


            ArithmeticNode node = ParseExpr(tokens, ref pos); 

            if (pos < tokens.Length)
            {
                throw new Exception($"Unexpected {tokens[pos].TokenType} in arithmetic expression, line {tokens[pos].Line}");
            }

            TokensConsumed = pos;
            return node;
        }

        // Parse + or -
        private ArithmeticNode ParseExpr(LexerToken[] t, ref int pos)
        {
            ArithmeticNode left = ParseTerm(t, ref pos);

            while (pos < t.Length && t[pos].TokenType is LexerType.Token_Add or LexerType.Token_Minus)
            {
                char op = (t[pos].TokenType == LexerType.Token_Add) ? '+' : '-';
                pos++;
                left = new BinaryOpNode(left, op, ParseTerm(t, ref pos));
            }

            return left;
        }

        // Parse * or /
        private ArithmeticNode ParseTerm(LexerToken[] t, ref int pos)
        {
            ArithmeticNode left = ParseFactor(t, ref pos);

            while (pos < t.Length && t[pos].TokenType is LexerType.Token_Multiply or LexerType.Token_Divide) // Mult and div guard clauses
            {
                char op = (t[pos].TokenType == LexerType.Token_Multiply) ? '*' : '/';
                pos++;
                left = new BinaryOpNode(left, op, ParseFactor(t, ref pos));
            }

            return left;
        }

        // Parse integer, identifier and parenthesis
        private ArithmeticNode ParseFactor(LexerToken[] t, ref int pos)
        {
            if (pos >= t.Length)
            {
                throw new Exception("Arithmetic expression ended unexpectedly");
            }

            LexerToken tok = t[pos];

            switch (tok.TokenType) // token determinant
            {
                case LexerType.TokenInt:
                    pos++;
                    return new NumberNode(int.Parse(tok.GetContent()));

                case LexerType.Token_Identifier:
                    pos++;
                    return new VariableNode(tok.GetContent());

                case LexerType.Token_LParen:
                    pos++;
                    ArithmeticNode inner = ParseExpr(t, ref pos);

                    if (pos >= t.Length || t[pos].TokenType != LexerType.Token_RParen)
                    {
                        throw new Exception($"Missing ')' in arithmetic expression, line {tok.Line}");
                    }

                    pos++;
                    return inner;

                default:
                    throw new Exception($"Unexpected {tok.TokenType} in arithmetic expression, line {tok.Line}");
            }
        }

    }
}
