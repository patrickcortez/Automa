using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Core
{
    internal class Expression
    {
        public LogOp ParseUnit(List<LexerToken> Tokens)
        {

            LogOp? current = null;
            int Pos = 0,max=Tokens.Count;
            Expression expr = ParseExpression(Tokens, ref Pos);


            

            return current;
        }

        private Expression ParseExpression(List<LexerToken> Tokens,ref int Pos) // == and !=
        {
            Expression expr;


            while(Pos < Tokens.Count && Tokens[Pos].TokenType == )

            return expr;
        }

        private Operand ParseOperand(List<LexerToken> Tokens,ref int Pos) // 
        {

        }
    }
}
