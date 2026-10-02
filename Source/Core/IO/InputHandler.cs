using Automa.Source.Core.Shell;
using System;
using System.Collections.Generic;
using System.Text;

namespace Automa.Source.Core.IO
{

    // Automa Input handler for CLI
    internal static class InputHandler
    {

        public static string ReadLine()
        {
            StringBuilder input = new();
            int cursor = 0,scroll=History.prev.Count-1;

            while (true)
            {
                ConsoleKeyInfo current = Console.ReadKey(true);

                switch (current.Key)
                {
                    case ConsoleKey.Enter:  // add a newline and return input
                        Console.WriteLine();
                        return input.ToString();

                    case ConsoleKey.LeftArrow:
                        if (cursor > 0)
                        {
                            cursor--;
                            Console.SetCursorPosition( // naviggate left of the string input
                                Console.CursorLeft - 1,
                                Console.CursorTop
                            );
                        }
                        break;

                    case ConsoleKey.RightArrow:   // navigate right of the string input
                        if (cursor < input.Length)
                        {
                            cursor++;
                            Console.SetCursorPosition(
                                Console.CursorLeft + 1,
                                Console.CursorTop
                            );
                        }
                        break;

                    case ConsoleKey.UpArrow: // move up the hisyory

                        if(scroll < History.prev.Count)
                        {
                            Console.SetCursorPosition( // set cursor to original position
                             Console.CursorLeft - cursor,
                             Console.CursorTop
                             );
                            input.Clear();
                            input.Append(History.prev[scroll]);

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

                            input.Clear();
                            input.Append(History.prev[scroll]);

                            Console.Write(input.ToString());

                            Console.SetCursorPosition(
                                Console.CursorLeft - (input.Length-cursor),
                                Console.CursorTop
                                );

                            scroll--;

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
                            input.Remove(cursor - 1, 1);
                            cursor--;

                            // move left
                            Console.SetCursorPosition(
                                Console.CursorLeft - 1,
                                Console.CursorTop
                            );

                            // rewrite the curr line
                            Console.Write(input.ToString(cursor, input.Length - cursor));

                            // remove remaining old chars
                            Console.Write(' ');

                            // add 1 to left since we added as
                            Console.SetCursorPosition(
                                Console.CursorLeft - (input.Length - cursor + 1),
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




                        // Only insert printable characters
                        if (!char.IsControl(current.KeyChar))
                        {
                            input.Insert(cursor, (current.Modifiers is ConsoleModifiers.Shift)? char.ToUpper(current.KeyChar):current.KeyChar);
                            cursor++;

                            // Redraw from insertion point
                            Console.Write(input.ToString(cursor - 1, input.Length - cursor + 1));

                            // Move cursor back to insertion point
                            Console.SetCursorPosition(
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
