using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Automa.Source
{
    internal class NestBuilder(Instruction? Initial = null)
    {
        private Instruction? Nest = (Initial is not null)? Initial : null;

        public void AddNode(Instruction node)
        {
            if(Nest is null)
            {
                Nest = node;
                return;
            }

            Instruction current = Nest;
            while(current.Next != null)
            {
                current = current.Next;
            }

            current.Next = node;
        }

        public Instruction? Build()
        {

            return Nest ?? null;
        }
    }

    internal static class NodeBuilder
    {
        private static Instruction? AST = null;

        public static void GobbleNode() // Gobble oldest node
        {
            if(AST is null)
            {
                return;
            }

            Instruction NewAST = AST.Next;

            AST = null; // Pop last

            AST = NewAST; // insert new
        }

        public static void AddNode(Instruction node,bool inBlock = false) // Add to the main branch or Block branch
        {
            if(AST is null)
            {
                AST = node;
                return;
            }

            

            Instruction current = AST;

            while(current.Next != null)
            {
                current = current.Next;
            }

            if (inBlock) // if the current node is a block
            {
                if(current is Block block)
                {
                    Instruction? body = block.Body;

                    if(body is null)
                    {
                        block.Body = node;
                        return;
                    }

                    while(body.Next != null)
                    {
                        body = body.Next;
                    }

                    body.Next = node;
                    return;
                }
            }

            // If not an ifblock add to Next
            current.Next = node;
        }



        public static Instruction? Build()
        {
            return AST;
        }
    }
}
