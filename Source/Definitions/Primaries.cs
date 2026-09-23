using Automa.Source.Core;
using Automa.Source.Utility;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;


namespace Automa.Source.Definitions
{
    internal enum PrintOptions // ConsoleWriteLine Wrapper
    {
        Normal,
        Warning,
        Error
    }

    internal enum VariableType
    {
        String, // "ABCdef..."
        Int, // 1,2,3,...
        Boolean, // true or false
        Invokable, // function call

        Identifier // var,num
    }

   
    internal enum LexerType // Lexer Types
    {
        Token_LBrace, // {
        Token_RBrace, // }
        Token_SemiColon, // ;
        Token_Equal, // =
        Token_LParen, // (
        Token_RParen, // )
        Token_Comma, // ,
        Token_Add, // +
        Token_Minus, // -
        TokenString, // Dave123
        TokenInt, // 123
        TokenBool, // true or false
        Token_Not, // !
        TokenArith, // 2 + 2 - 2
        Token_Identifier, // Write,Read etc...
        Token_Multiply, // *
        Token_Divide, // \
        Token_KeyWord,
        Token_GreaterThan, // >
        Token_LessThan, // <
        Token_Or, // ||
        Token_And, // &&
        Token_EqualTo, // ==
        Token_NotEqualTo, // !=
        Token_GTE, // >=
        Token_LTE, // <=
        Token_Pipe, // |
        Token_Ampersand, // &
        Token_Increment, // ++
        Token_Decrement, // --
        Token_None // Default value;

    }
    //---

    internal struct LexerToken // Lexer Token definition
    {
        public StringBuilder Content { get; set; } = new(); // Content Storage (Assignment , Argument & Expression values)
        public LexerType TokenType { get; set; }
        public readonly int Line { get; }

        public LexerToken(LexerType _Type, int _Line, string _Content = "") // Constructor
        {
            Content.Append(_Content);
            TokenType = _Type;
            Line = _Line;
        }

        public void Append(string NewContent)
        {

            if (NewContent.Length == 0)
            {
                return;
            }

            Content.Append(NewContent);
        }

        public string GetContent()
        {
            return Content.ToString();
        }
    }


    // Print Configuration definition
    internal record PrintConfiguration(PrintOptions option, bool newline);

    // Unary Assign type:

    enum UnaryKind
    {
        Increment,
        Decrement
    }

    // Assigns

    internal abstract record Instruction
    {
        public Instruction? Next { get; set; }
    }

    // Instructions
    internal record WriteInstruction(string Content, bool isIdent = false) : Instruction;

    // variable definition
    internal record Variable(string name, string value, VariableType type = VariableType.String, (List<Parameter>? Arguments, Return? type)? FunctionCall = null);

}

