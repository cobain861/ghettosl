using System;
using System.Collections.Generic;
using System.Text;

namespace ghetto
{
    partial class GhettoSL
    {
        void IntroArt()
        {
            Console.ForegroundColor = System.ConsoleColor.DarkCyan;
            Console.WriteLine("\r\n════════════════════════════════════════════════\r\n");
            Console.ForegroundColor = System.ConsoleColor.Cyan;
            Console.WriteLine("          ┌──┐                                                   ");
            Console.WriteLine("          │  │                                                   ");
            Console.WriteLine(" ┌────────┴┐ │      ┌─────────────┐                              ");
            Console.WriteLine(" │         │ │   ┌──┴────┐        │┌───────┐                     ");
            Console.WriteLine(" │  ┌───┐  │ │   │       ├┐  ┌────┘│       │                     ");
            Console.WriteLine(" │  │   │  │ └───┴┐┌───┐ ││  │ ┌─┐ │   .   │                     ");
            Console.WriteLine(" │  └───┘  │ ┌──┐ │└───┘ ││  ├─┘ └─┴┐ ││   │                     ");
            Console.WriteLine(" └───────┐ │ │  │ │      ││  ├─┐ ┌─┬┘ ││   │                     ");
            Console.WriteLine("  ┌─┐    │ │ │  │ │┌─────┘│  │ │ │ │  ││   │                     ");
            Console.WriteLine("  │ └────┘ │ │  │ │└─────┐│  │ │ │ │  `    │ SL                  ");
            Console.WriteLine("  └────────┘─┘  └─┘──────┘└──┘ └─┘ └───────┘                     ");
            Console.ForegroundColor = System.ConsoleColor.DarkCyan;
            Console.WriteLine("\r\n════════════════════════════════════════════════\r\n");
            Console.ForegroundColor = System.ConsoleColor.Gray;
        }

        void HeaderHelp()
        {
            Console.ForegroundColor = System.ConsoleColor.DarkCyan; Console.Write("\r\n-=");
            Console.ForegroundColor = System.ConsoleColor.Cyan; Console.Write("[");
            Console.ForegroundColor = System.ConsoleColor.White; Console.Write(" Commands ");
            Console.ForegroundColor = System.ConsoleColor.Cyan; Console.Write("]");
            Console.ForegroundColor = System.ConsoleColor.DarkCyan; Console.Write("=-────────────────────────────────────────────--──────────--──--·\r\n");
            Console.ForegroundColor = System.ConsoleColor.Gray;
        }

        void HeaderWho()
        {
            Console.ForegroundColor = System.ConsoleColor.DarkCyan; Console.Write("\r\n-=");
            Console.ForegroundColor = System.ConsoleColor.Cyan; Console.Write("[");
            Console.ForegroundColor = System.ConsoleColor.White; Console.Write(" Nearby Avatars ");
            Console.ForegroundColor = System.ConsoleColor.Cyan; Console.Write("]");
            Console.ForegroundColor = System.ConsoleColor.DarkCyan; Console.Write("=-──────────────────────────────────────--──────────--──--·\r\n");
            Console.ForegroundColor = System.ConsoleColor.Gray;
        }
        void Footer()
        {
            Console.ForegroundColor = System.ConsoleColor.DarkCyan; //32 to 111
            Console.WriteLine("-────────────────────────────────────────────────────────────-──────────--──--·\r\n");
            Console.ForegroundColor = System.ConsoleColor.Gray;
        }
    }
}
