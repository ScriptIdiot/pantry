using System;

namespace Sift.Utilities
{
    internal class Banner
    {
        public static void Show()
        {
            Console.WriteLine();
            Console.WriteLine("\t    ~ Sift ~");
            Console.Write("         />_________________________________\n");
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("########");
            Console.ResetColor();
            Console.Write("[]_________________________________|\n");
            Console.Write("         \\>\n");
            Console.WriteLine("\t\t\t~ mez0 ~");
            Console.WriteLine();
        }
    }
}