/*
* Copyright (c) 2006, obsoleet industries
* All rights reserved.
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
*     * Redistributions of source code must retain the above copyright
*       notice, this list of conditions and the following disclaimer.
*     * Redistributions in binary form must reproduce the above copyright
*       notice, this list of conditions and the following disclaimer in the
*       documentation and/or other materials provided with the distribution.
*     * Neither the name of obsoleet industries nor the names of its
*       contributors may be used to endorse or promote products derived from
*       this software without specific prior written permission.
*
* THIS SOFTWARE IS PROVIDED BY THE REGENTS AND CONTRIBUTORS "AS IS" AND ANY
* EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
* DISCLAIMED. IN NO EVENT SHALL THE REGENTS AND CONTRIBUTORS BE LIABLE FOR ANY
* DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
* LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using libsecondlife;
using System;
using System.Collections.Generic;
using System.Text;

namespace ghetto
{
    partial class GhettoSL
    {
        void IntroArt(int imageNumber)
        {
            switch (imageNumber)
            {
                case 1:
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
                        break;
                    }
                case 2:
                    {
                        Console.ForegroundColor = System.ConsoleColor.Cyan;
                        Console.BackgroundColor = System.ConsoleColor.Cyan;
                        Console.Write("\r\n■■■■■■■■■■");
                        Console.ForegroundColor = System.ConsoleColor.DarkCyan;
                        Console.BackgroundColor = System.ConsoleColor.Black;
                        Console.Write("┌──┐");
                        Console.ForegroundColor = System.ConsoleColor.Cyan;
                        Console.BackgroundColor = System.ConsoleColor.Cyan;
                        Console.Write("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\r\n");
                        Console.ForegroundColor = System.ConsoleColor.DarkCyan;
                        Console.BackgroundColor = System.ConsoleColor.Black;
                        Console.WriteLine("░░░░░░░░░░│  │░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░");
                        Console.WriteLine("░┌────────┴┐ │░░░░░░┌─────────────┐░░░░░░░░░░░░░░░░░░░░░░░░");
                        Console.WriteLine("▒│         │ │▒▒▒┌──┴────┐        │┌───────┐▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒");
                        Console.WriteLine("▒│  ┌───┐  │ │▒▒▒│       ├┐  ┌────┘│       │▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒");
                        Console.WriteLine("▓│  │ ☻ │  │ └───┴┐┌───┐ ││  │▓┌─┐▓│  ╒    │▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓");
                        Console.WriteLine("─│  └───┘  │ ┌──┐ │└───┘ ││  ├─┘ └─┴┐ ││   │───-─- ☼ -─-───");
                        Console.WriteLine("▓└───────┐ │ │▓▓│ │      ││  ├─┐ ┌─┬┘ ││   │▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓");
                        Console.WriteLine("▒▒┌─┐    │ │ │▒▒│ │┌─────┘│  │▒│ │▒│  ││   │▒▒▒▒▒▒▒▒▒▒▒▒▒▒▒");
                        Console.WriteLine("▒▒│ └────┘ │ │▒▒│ │└─────┐│  │▒│ │▒│   ╛   │►SL◄▒▒▒▒▒▒▒▒▒▒▒");
                        Console.WriteLine("░░└────────┘─┘░░└─┘──────┘└──┘░└─┘░└───────┘░░░░░░░░░░░░░░░");
                        Console.ForegroundColor = System.ConsoleColor.Cyan;
                        Console.BackgroundColor = System.ConsoleColor.Cyan;
                        Console.WriteLine("■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■■\r\n");
                        Console.ForegroundColor = System.ConsoleColor.Gray;
                        Console.BackgroundColor = System.ConsoleColor.Black;
                        break;
                    }
            }
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

        void ShowStats()
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("─────────-──────────--───--·\r\n");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Uptime : ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Duration(Helpers.GetUnixTime() - Session.StartTime) + "\r\n");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Earned : L$");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Session.MoneyReceived + "\r\n");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" Spent  : L$");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(Session.MoneySpent + "\r\n");

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("──────────-──────────--──--·\r\n");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

    }
}
