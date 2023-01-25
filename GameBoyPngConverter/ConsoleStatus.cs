using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameBoyPngConverter
{
    internal static class ConsoleStatus
    {
        internal static void Errored()
        {
            var oldConsoleFGColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Error: ");
            Console.ForegroundColor = oldConsoleFGColor;
        }

        internal static void Warning()
        {
            var oldConsoleFGColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Warning: ");
            Console.ForegroundColor = oldConsoleFGColor;
        }

        internal static void Completed()
        {
            var oldConsoleFGColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Completed");
            Console.ForegroundColor = oldConsoleFGColor;
        }
    }
}
