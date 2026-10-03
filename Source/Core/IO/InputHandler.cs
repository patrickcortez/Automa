using Automa.Source.Core.Shell;
using System;
using System.Collections.Generic;
using TextCopy;
using System.Text;

namespace Automa.Source.Core.IO
{
 

    // Automa Input handler for CLI
    internal static class InputHandler
    {
        private const string Tab = "    ";  // 4 spaces
        public static string ReadLine()
        {
            
            StringBuilder input = new();
            int cursor = 0,scroll=0;
            bool selected = false;


            void Redraw(bool erase = false)
            {

                Console.SetCursorPosition(
                    Math.Max(0, Console.CursorLeft - cursor),
                    Console.CursorTop
                );


                if (erase) // erase all in the buffer and reset cursor to start
                {
                    Console.Write(new string(' ', input.Length));
                    int orig = input.Length; // save original size
                    input.Clear();  // clear buffer
                    cursor = 0;   // reset cursor/pointer
                    selected = false;

                    Console.SetCursorPosition(Console.CursorLeft - orig, Console.CursorTop); // return to start.
                }
                else
                {
                    if (selected) // highlight text in cyan and magenta on select
                    {
                        Console.BackgroundColor = ConsoleColor.Cyan;
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    }
                    Console.Write(input.ToString());
                    Console.ResetColor();
                    Console.SetCursorPosition(
                    Math.Max(0, Console.CursorLeft - (input.Length - cursor)),
                    Console.CursorTop
                    );
                }

            }

            while (true)
            {
                ConsoleKeyInfo current = Console.ReadKey(true);

                

                switch (current.Key)
                {
                    case ConsoleKey.Enter:  // add a newline and return input

                        if (selected)
                        {
                            selected = false;
                            Redraw();
                        }

                        Console.WriteLine();
                        return input.ToString();

                    case ConsoleKey.Tab: // \t handling: 4 spacing
                        if (selected)
                        {
                            selected = false;
                            Redraw();
                        }

                        input.Insert(cursor, Tab); // store tab in buffer
                        cursor += Tab.Length;   //  + 4

                        
                        Console.Write(input.ToString(cursor - Tab.Length, input.Length - (cursor - Tab.Length)));

                        
                        Console.SetCursorPosition(
                            Console.CursorLeft - (input.Length - cursor),
                            Console.CursorTop
                        );
                        break;

                    case ConsoleKey.LeftArrow:
                        if (cursor > 0)
                        {
                            cursor--;
                            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                        }
                        break;

                    case ConsoleKey.RightArrow:   // navigate right of the string input
                        if (cursor < input.Length)
                        {
                            cursor++;
                            Console.SetCursorPosition(Console.CursorLeft + 1, Console.CursorTop);
                        }
                        break;

                    case ConsoleKey.UpArrow: // move up the hisyory

                        if(scroll < History.prev.Count)
                        {
                            Console.SetCursorPosition( // set cursor to original position
                             Console.CursorLeft - cursor,
                             Console.CursorTop
                             );

                            int pad = input.Length;
                            input.Clear();
                            input.Append(History.prev[scroll]);

                            Console.Write(new string(' ', pad));
                            
                            Console.SetCursorPosition(Console.CursorLeft - pad, Console.CursorTop);
                            

                            Console.Write(input.ToString());

                            Console.SetCursorPosition(
                                Console.CursorLeft - (input.Length - cursor),
                                Console.CursorTop
                                );

                            scroll++;
                        }

                        break;
                    case ConsoleKey.DownArrow:

                        if(scroll > 0)
                        {
                            Console.SetCursorPosition(
                                Console.CursorLeft -  cursor,
                                Console.CursorTop
                                );
                            scroll--;

                            int pad = input.Length;
                            input.Clear();
                            input.Append(History.prev[scroll]);

                            Console.Write(new string(' ', pad));

                            Console.SetCursorPosition(Console.CursorLeft - pad, Console.CursorTop);

                            Console.Write(input.ToString());

                            Console.SetCursorPosition(
                                Console.CursorLeft - (input.Length-cursor),
                                Console.CursorTop
                                );

                            

                        }
                        break;

                    case ConsoleKey.Home:
                        if (cursor > 0)
                        {
                            Console.SetCursorPosition(
                                Console.CursorLeft - cursor,
                                Console.CursorTop
                            );

                            cursor = 0;
                        }
                        break;

                    case ConsoleKey.End:
                        if (cursor < input.Length)
                        {
                            Console.SetCursorPosition(
                                Console.CursorLeft + (input.Length - cursor),
                                Console.CursorTop
                            );

                            cursor = input.Length;
                        }
                        break;

                    case ConsoleKey.Backspace: // backspace key
                        if (cursor > 0) // if cursor is gt 0 move back by 1 per key press
                        {

                            if (selected)
                            {
                                selected = false;
                                Redraw(true);
                                break;

                            }

                            // determine if tab or space
                            int count = (cursor >= Tab.Length &&
                                         input.ToString(cursor - Tab.Length, Tab.Length) == Tab)
                                        ? Tab.Length : 1;

                            input.Remove(cursor - count, count);
                            cursor -= count;



                            Console.SetCursorPosition(Console.CursorLeft - count, Console.CursorTop);
                            Console.Write(input.ToString(cursor, input.Length - cursor));
                            Console.Write(new string(' ', count));

                           
                            Console.SetCursorPosition(
                                Console.CursorLeft - (input.Length - cursor + count),
                                Console.CursorTop
                            );
                        }
                        break;

                    case ConsoleKey.Delete:
                        if (cursor < input.Length)
                        {
                            input.Remove(cursor, 1);

                            // Rewrite everything after cursor
                            Console.Write(input.ToString(cursor, input.Length - cursor));
                            Console.Write(' ');

                            // Return cursor
                            Console.SetCursorPosition(
                                Console.CursorLeft - (input.Length - cursor + 1),
                                Console.CursorTop
                            );
                        }
                        break;

                   

                    default:
                        // ignore ALL controls..
                        //if (current.Modifiers == ConsoleModifiers.Control)
                        //    break;

                         if(current.Key is ConsoleKey.A && current.Modifiers.HasFlag(ConsoleModifiers.Control))
                        {
                                
                                selected = true;
                               // cursor = input.Length;
                                Redraw();
                            break;


                        }


                         if(current.Key is ConsoleKey.C && current.Modifiers.HasFlag(ConsoleModifiers.Control)) // copy to clipboard
                        {
                            ClipboardService.SetText(input.ToString());

                            if (selected)
                            {
                                selected = false;
                                Redraw();
                            }
                           
                        }
                         

                         if(current.Key is ConsoleKey.V && current.Modifiers.HasFlag(ConsoleModifiers.Control)) // paste to buffer
                        {
                            int orig = input.Length;
                            input.Clear();
                            input.Append(ClipboardService.GetText()); // append whole text

                            Console.SetCursorPosition(
                                Math.Max(0, Console.CursorLeft - orig),
                                Console.CursorTop
                                );

                            Console.Write(input.ToString()); // print pasted text
                        }


                        // Only insert printable characters
                        if (!char.IsControl(current.KeyChar))
                        {
                            if (selected)
                            {
                                selected = false;                             
                                Redraw();
                            }

                            input.Insert(cursor, (current.Modifiers is ConsoleModifiers.Shift)? char.ToUpper(current.KeyChar):current.KeyChar);
                            cursor++;

                            // Redraw from insertion point
                            Console.Write(input.ToString(cursor - 1, input.Length - cursor + 1));

                            Console.SetCursorPosition( // reset to original pos by calc diff between in len and curse
                            Console.CursorLeft - (input.Length - cursor),
                            Console.CursorTop
                        );

                        }

                        break;
                }
            }
        }

    }
}
