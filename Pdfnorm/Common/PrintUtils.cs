using System;
using System.Linq;

namespace Pdfnorm.Common
{
    internal static class PrintUtils
    {
        public static void WriteLine(string line)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ResetColor();

            string[] segments = line.Split('<');
            foreach (string segment in segments)
            {
                bool printed = false;
                foreach (ConsoleColor color in Enum.GetValues<ConsoleColor>().Cast<ConsoleColor>())
                {
                    if (segment.StartsWith(color.ToString(), StringComparison.CurrentCultureIgnoreCase))
                    {
                        Console.ForegroundColor = color;
                        Console.Write(segment[(color.ToString().Length + 1)..]);
                        printed = true;
                        break;
                    }
                }

                if (!printed)
                {
                    Console.ResetColor();
                    Console.Write(segment.StartsWith('/') ? segment.Split('>')[1] : segment);
                }
            }
            Console.ResetColor();
            Console.Write("\n");
        }
    }
}
