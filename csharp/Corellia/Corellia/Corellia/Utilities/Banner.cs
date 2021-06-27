using System;

namespace Corellia.Utilities
{
    internal class Banner
    {
        public static void Show()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"    __________________________________");
            Console.WriteLine(@"   |:                           ``::%H|");
            Console.WriteLine(@"   |%:.        Corellia            `:%|");
            Console.WriteLine(@"   |H%::..___________________________:|");
            Console.ResetColor();
            Console.WriteLine();
        }
    }
}