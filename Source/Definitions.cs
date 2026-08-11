using Automa.Source.Core;
using Automa.Source.Utility;
using System.Data;
using System.Diagnostics;

namespace Automa.Source
{
    internal enum PrintOptions
    {
        Normal,
        Warning,
        Error
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

