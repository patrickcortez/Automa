using Automa.Source.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Automa.Source.Definitions
{
    internal abstract record AssignType;

    internal record VariableAssign(Variable variable) : AssignType; //for exit code

    internal record ReadAssign(string target, string Prompt) : AssignType; //

    internal record UnaryAssign(string target, UnaryKind kind) : AssignType
    {
        public void UpdateScope(List<Variable> Scope)
        {
            Variable? Result = Scope.FirstOrDefault();

            if (Result is null)
            {
                return;
            }

            if (Result.type is not VariableType.Int)
            {
                return;
            }

            int res = int.Parse(Result.value);

            if (kind is UnaryKind.Increment)
            {
                res++;
            }
            else
            {
                res--;
            }

            Result = Result with { value = res.ToString() };
            return;
        }
    }

    internal record AssignInstruction(AssignType type) : Instruction;

    internal record ArithmeticAssign(ArithmeticNode node, string target) : AssignType
    {
        public void UpdateScope(List<Variable> Scope)
        {
            Variable? Result = Scope.FirstOrDefault(e => e.name == target);

            int val = node switch
            {
                NumberNode n => n.value,
                VariableNode v => v.Eval(Scope),
                BinaryOpNode b => b.Eval(Scope),
                _ => 0
            };

            if (Result is null)
            {
                Scope.Add(new Variable(target, val.ToString(), VariableType.Int));
                return;
            }

            Result = Result with { value = val.ToString() };
            Result = Result with { type = VariableType.Int };

            return;
        }
    }

    internal record FunctionCall(string name, string? target = null, List<Parameter>? Params = null) : AssignType
    {
        public int Invoke(List<Variable> Scope)
        {

            if (target is null) // expression
            {
                FunctionTable.RunFunc(name, Params, Scope);
            }

            Variable? Target = Scope.FirstOrDefault(ex => ex.name == target);

            if (Target is null)
            {

            }

            FunctionTable.RunFunc(name, Params, Scope);

            return 1;
        }
    }

    //Processes

    internal record RunAssignment((string Target, string Cmd) Properties) : AssignType
    {
        public string Run()
        {
            string[] cmdPart = Properties.Cmd.Split(' ', 2);
            string name = cmdPart[0];
            string args = cmdPart.Length > 1 ? cmdPart[1] : "";

            Process proc = new();
            proc.StartInfo = new()
            {
                FileName = name,
                Arguments = args,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };


            if (proc.Start())
            {
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                proc.OutputDataReceived += (_, e) =>
                {
                    if (e.Data != null)
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
