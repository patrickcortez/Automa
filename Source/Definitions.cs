using Automa.Source.Core;
using Automa.Source.Utility;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Automa.Source
{
    internal enum PrintOptions // ConsoleWriteLine Wrapper
    {
        Normal,
        Warning,
        Error
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
        TokenArith, // 2 + 2 - 2
        Token_Identifier // Write,Read etc...
    }

    internal struct LexerToken // Lexer Token definition
    {
        StringBuilder Token = new(); // Content Storage (Assignment , Argument & Expression values)
        LexerType TokenType;
        int Line;

        public LexerToken(LexerType _Type,int _Line,string Content="") // Constructor
        {
            Token.Append(Content);
            TokenType = _Type;
            Line = _Line;
        }

        public void Append(string NewContent)
        {

            if(NewContent.Length == 0)
            {
                return;
            }

            Token.Append(NewContent);
        }

        public string GetToken()
        {
            return Token.ToString();
        }
    }

    internal enum TokenType
    {
        If,
        Elif,
        Else,
        Null
    }
    internal enum AssignmentType
    {
        Variable,
        Run,
        Read
    }

    // Print Configuration definition
    internal record PrintConfiguration(PrintOptions option, bool newline);

    // Assigns

    internal abstract record AssignType;

    internal record VariableAssign(Variable variable) : AssignType;
    // Instructions
    internal record WriteInstruction(string Content);

    internal record ReadAssign(string target,string Prompt) : AssignType;

    internal record AssignInstruction(AssignType type); 

    internal record Variable(string name, string value)
    {
       public string name { get; set; } = name;
       public string value { get; set; } = value;
    }

    internal record IfBlock(Expression expression, List<object> Instructions, List<Variable> Variables) // if(condition)
    {
        public List<object> Instructions { get; set; } = Instructions;

        public List<Variable> Variable { get; set; } = Variables;

        public int ExecuteBlock()
        {
           // Console.WriteLine("Amount of VAriable in IF: {0}", Variables.Count);
            //Console.WriteLine("Debug: IF Instructions: {0}", this.Instructions.Count);
            Executor executor = new(this.Instructions, this.Variable);

            return  executor.Start();
        }
    }

    internal record Elif(Expression expression, List<object> Instructions, List<Variable> Variables) // elif(<condition>)
    {
        public List<object> Instructions { get; set; } = Instructions;

        public List<Variable> Variable { get; set; } = Variables;

        public int ExecuteBlock()
        {
            //Console.WriteLine("Debug: IF Instructions: {0}", this.Instructions.Count);
            Executor executor = new(this.Instructions, this.Variable);

            return  executor.Start();
        }
    }

    internal record Else(List<object> Instructions, List<Variable> Variables)
    {
        public List<object> Instructions { get; set; } = Instructions;

        public List<Variable> Variable { get; set; } = Variables;
        public int ExecuteBlock()
        {
            Executor executor = new(Instructions, Variables);
            return executor.Start();
        }
    }

    // Expressions

    internal abstract record Expression();

    internal record VariableExpression(string VariableName) : Expression
    {
        private Variable? Value { get; set; }

        public Variable? GetVariable(List<Variable> Variables)
        {
            Value = Utils.FindVariable(VariableName, Variables);
            return Value;
        }
    }

    internal record LiteralExpression(string value) : Expression
    {
        public string Value { get; set; } = value;// well only have string data types anyways =P, so i dont have to cast later
    }

    internal record EqualTo(Expression left,Expression right) : Expression
    {
        private List<Variable> Variables = new();

        public void UpdateVariables(List<Variable> Updated)
        {
            Variables = Updated;
        }

        public bool Evaluate()
        {
            // Console.WriteLine("Debug: Left Type: {0} , Right Type: {1}", left.GetType(), right.GetType());

            if(left is LiteralExpression LitLeft)
            {
                if(right is LiteralExpression LitRight)
                {
                    Variable? FindLeft = Utils.FindVariable(LitLeft.value, Variables);
                    Variable? FindRight = Utils.FindVariable(LitRight.value, Variables);

                    if(FindLeft != null && FindRight != null)
                    {
                        return FindLeft.value == FindRight.value;
                    }else if(FindLeft != null)
                    {
                        return FindLeft.value == LitRight.value;
                    }else if(FindRight != null)
                    {
                        return LitLeft.Value == FindRight.value;
                    }
                    else
                    {
                        return  LitLeft.value == LitRight.value;
                    }
                }
            }

            throw new Exception("Unknown Expression used!");
        }
    }

    internal record NotEqualTo(Expression left, Expression right) : Expression
    {

        private List<Variable> Variables = new();

        public void UpdateVariables(List<Variable> Updated)
        {
            Variables = Updated;
        }


        public bool Evaluate()
        {
            // Console.WriteLine("Debug: Left Type: {0} , Right Type: {1}", left.GetType(), right.GetType());

            if (left is LiteralExpression LitLeft)
            {
                if (right is LiteralExpression LitRight)
                {
                    Variable? FindLeft = Utils.FindVariable(LitLeft.value, Variables);
                    Variable? FindRight = Utils.FindVariable(LitRight.value, Variables);

                    if (FindLeft != null && FindRight != null)
                    {
                        return FindLeft.value != FindRight.value;
                    }
                    else if (FindLeft != null)
                    {
                        return FindLeft.value != LitRight.value;
                    }
                    else if (FindRight != null)
                    {
                        return LitLeft.Value != FindRight.value;
                    }
                    else
                    {
                        return LitLeft.value != LitRight.value;
                    }
                }
            }

            throw new Exception("Unknown Expression used!");
        }
    

}

    //Processes

    internal record RunAssignment((string Target,string Cmd) Properties) : AssignType
    {
        public string Run()
        {
            string[] cmdPart = Properties.Cmd.Split(' ', 2);
            string name = cmdPart[0];
            string args = cmdPart.Length > 1 ? cmdPart[1] : "";

            Process proc = new();
            proc.StartInfo = new()
            {
                FileName=name,
                Arguments=args,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            

            if (proc.Start()){
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                proc.OutputDataReceived += (_, e) =>
                {
                   if(e.Data != null)
                    {
                        //Do nothing
                    }
                };

                proc.ErrorDataReceived += (_, e) =>
                {
                    if (e.Data != null)
                    {
                        //Do nothing
                    }
                };

                proc.WaitForExit();
                return $"{proc.ExitCode}";
            }

            return "1";
        }
    }

}

