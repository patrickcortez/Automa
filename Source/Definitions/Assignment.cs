using Automa.Source.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Automa.Source.Definitions
{
    internal abstract record AssignType;

    internal record VariableAssign(Variable variable) : AssignType; //for exit code

    internal record ReadAssign(string target, string Prompt) : AssignType; // input

    internal record UnaryAssign(string target, UnaryKind kind) : AssignType
    {
        public void UpdateScope(List<Variable> Scope)
        {
            int at = Scope.FindIndex(ex => ex.name == target);

            if (at < 0)
            {
                return;
            }

            if (Scope[at].value is AutomaInteger integ)
            {
                int val = (kind is UnaryKind.Increment) ? integ.value + 1 : integ.value - 1;

                
                Scope[at] = Scope[at] with { value = new AutomaInteger(val), type = VariableType.Int };
                return;
            }

            throw new Exception("Cannot increment a non integer");
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
                Scope.Add(new Variable(target,new AutomaInteger(val), VariableType.Int));
                return;
            }

            int idx = Scope.FindIndex(e => e.name == target);
            Scope[idx] = Result with { value = new AutomaInteger(val), type = VariableType.Int };



            return;
        }
    }

    internal record FunctionCall(string name, string? target = null, List<Parameter>? Params = null) : AssignType
    {
        public int Invoke(List<Variable>? Scope,bool isdebug = false)
        {

            if (isdebug)
            {
                Console.WriteLine("[DEBUG] Current Variables in scope for function call {0}:",name);
                if(Scope is not null)
                {
                    Scope.ForEach(item => Console.WriteLine(item));
                }

            }

            if (target is null) // expression
            {

                if (isdebug)
                {
                    Console.WriteLine("[DEBUG] Executing targetless func call with: ");

                    if(Params is not null)
                    {
                        if(Params.Count is 0)
                        {

                            Console.WriteLine("[DEBUG] There is no parameters in the function");
                        }

                        Params.ForEach(e => Console.WriteLine(e));
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] Parameters are null");
                    }


                }

                FunctionTable.RunFunc(name, Params, Scope,isdebug);
                return 0;
            }

            if(Scope is null)
            {
                FunctionTable.RunFunc(name, Params, Scope);
                return 0;
            }

            Variable? Target = Scope.FirstOrDefault(ex => ex.name == target);
            Return? result = FunctionTable.RunFunc(name, Params, Scope);

            if (result is null)
            {
                return 0;
            }

            if (Target is null)
            {
                Scope.Add(new(target, result.value));
                return 0;
            }
            Target = Target with { value = result.value };
            int index = Scope.IndexOf(Target);
            Scope[index] = Target;

            return 1;
        }
    }

    //Processes

    internal record RunAssignment((string Target, string Cmd) Properties,bool ReturnOutput=false) : AssignType
    {
        public int Run()
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
                RedirectStandardOutput = true,
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
                return proc.ExitCode;
            }

            return proc.ExitCode;
        }
    }
}
