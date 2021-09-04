using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using TechLink.Core;

namespace TechLink.Core.Interfacing
{
    public enum InterfaceColor { Unknown, Default, Error, Unimportant, Instruction, Emphasis }
    public static class Interface
    {
        public static List<IPage> CurrentPages { get; set; } = new List<IPage>();

        static InterfaceColor _currentColor;

        public static void Initialize() => SetColor(InterfaceColor.Default);

        public static void VisitPage(IPage page)
        {
            try
            {
                CurrentPages.Add(page);
                page.Open();
            }
            catch (NavigatedBackException)
            {
                if (CurrentPages.Last() != page) throw new NavigatedBackException();
            }

            CurrentPages.RemoveAt(CurrentPages.Count - 1);
        }

        public static void WriteFormatted(string str)
        {
            int prevPos = 0, currentPos = 0;

            while (true)
            {
                // See if there's another "$" format to handle.
                currentPos = str.IndexOf('$', prevPos);
                if (currentPos == -1) goto Finish;
                
                // Write previous and handle escape character.
                if (currentPos > 0)
                {
                    WritePreviousText();
                    if (HasEscape()) goto Fail;
                }

                if (currentPos + 4 >= str.Length) goto Fail;

                // Get format.
                var colorCode = GetFormat();
                if (colorCode == InterfaceColor.Unknown) goto Fail;

                // Write inner text.
                if (!WriteInnerText(colorCode)) goto Fail;

                prevPos = currentPos + 1;
            }
        Finish:
            if (prevPos != str.Length) Write(str[prevPos..^0]);
            return;

        Fail:
            throw new Exception("An invalid formatted string was given for writing.");

            void WritePreviousText() => Write(str[prevPos..currentPos]);
            bool HasEscape() => str[currentPos - 1] == '\\';

            InterfaceColor GetFormat()
            {
                return str[currentPos + 1] switch
                {
                    '!' => InterfaceColor.Error,
                    'E' => InterfaceColor.Emphasis,
                    'I' => InterfaceColor.Instruction,
                    'S' => InterfaceColor.Unimportant,
                    _ => InterfaceColor.Unknown
                };
            }

            bool WriteInnerText(InterfaceColor colorCode)
            {
                prevPos = currentPos + 3;
                currentPos = str.IndexOf('}', prevPos);

                if (currentPos == -1) return false;

                // Write the text inside.
                Write(str[prevPos..currentPos], colorCode);
                return true;
            }
        }

        public static void Write(string str) => Console.Write(str);
        public static void Write(string str, InterfaceColor color)
        {
            var prev = _currentColor;

            SetColor(color);
            Console.Write(str);
            SetColor(prev);
        }

        public static void WriteLine() => Console.WriteLine();
        public static void WriteLine(string str) => Console.WriteLine(str);
        public static void WriteLine(string str, InterfaceColor color)
        {
            var prev = _currentColor;

            SetColor(color);
            Console.WriteLine(str);
            SetColor(prev);
        }
        public static void WriteLineFormatted(string str)
        {
            WriteFormatted(str);
            Console.Write('\n');
        }

        public static string ReadLine()
        {
            string res = Console.ReadLine();
            if (res == "BACK") throw new NavigatedBackException();
#if DEBUG
            if (res == "BR") Debugger.Break();
#endif

            return res;
        }

        public static void ShowErrorScreen(string error)
        {
            Clear();
            WriteLine(error, InterfaceColor.Error);
            ReadLine();
        }

        public static void SetColor(InterfaceColor color)
        {
            _currentColor = color;

            Console.ForegroundColor = color switch
            {
                InterfaceColor.Default => ConsoleColor.Cyan,
                InterfaceColor.Error => ConsoleColor.Red,
                InterfaceColor.Unimportant => ConsoleColor.DarkGray,
                InterfaceColor.Instruction => ConsoleColor.Green,
                InterfaceColor.Emphasis => ConsoleColor.Yellow,
                _ => throw new Exception("Invalid color chosen"),
            };
        }

        public static void Clear() => Console.Clear();
    }

    public struct MenuEntry<T>
    {
        public string Name;
        public Action<T> Activity;

        public MenuEntry(string name, Action<T> activity) => (Name, Activity) = (name, activity);
    }
}
