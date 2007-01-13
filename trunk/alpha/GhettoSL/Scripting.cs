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
using libsecondlife.Utilities;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace ghetto
{
    static class Scripting
    {

        public class UserScript
        {
            /// <summary>
            /// Session number(s) for which this script applies
            /// </summary>
            public uint[] Sessions;
            /// <summary>
            /// Array containing each line of the script
            /// </summary>
            public string[] Lines;
            /// <summary>
            /// Current line script step, referenced after a sleep
            /// </summary>
            public int CurrentStep;
            /// <summary>
            /// Timestamp of the last "settime" command
            /// </summary>
            public uint ScriptTime;
            /// <summary>
            /// Timestamp of the last (or currently active) sleep
            /// </summary>
            public uint SleepingSince;
            /// <summary>
            /// Timer to invoke the next command after sleeping
            /// </summary>
            public System.Timers.Timer SleepTimer;
            /// <summary>
            /// Dictionary of scripted events
            /// </summary>
            public Dictionary<string, ScriptEvent> Events;
            /// <summary>
            /// Load the specified script file into the Lines array
            /// </summary>
            public bool LoadScript(string scriptFile)
            {
                if (!File.Exists(scriptFile)) return false;

                string[] script = { };
                string input;
                int error = 0;
                StreamReader read = File.OpenText(scriptFile);
                for (int i = 0; (input = read.ReadLine()) != null; i++)
                {
                    char[] splitChar = { ' ' };
                    string[] args = input.ToLower().Split(splitChar);
                    string[] commandsWithArgs = { "camp", "event", "go", "goto", "if", "label", "login", "pay", "payme", "say", "shout", "sit", "teleport", "touch", "touchid", "updates", "wait", "whisper" };
                    string[] commandsWithoutArgs = { "fly", "land", "listen", "quiet", "quit", "relog", "run", "sitg", "stand", "walk" };
                    if (Array.IndexOf(commandsWithArgs, args[0]) > -1 && args.Length < 2)
                    {
                        Console.WriteLine("Missing argument(s) for command \"{0}\" on line {1} of {2}", args[0], i + 1, scriptFile);
                        error++;
                    }
                    else if (Array.IndexOf(commandsWithArgs, args[0]) < 0 && Array.IndexOf(commandsWithoutArgs, args[0]) < 0)
                    {
                        Console.WriteLine("Unknown command \"{0}\" on line {1} of {2}", args[0], i + 1, scriptFile);
                        error++;
                    }
                    else
                    {
                        Array.Resize(ref script, i + 1);
                        script[i] = input;
                    }
                }
                read.Close();
                if (error > 0) return false;
                else
                {
                    Lines = script;
                    return true;
                }
            }

            public UserScript()
            {
                //Sessions = new uint[];
                //Lines = new string[];
                CurrentStep = 0;
                ScriptTime = Helpers.GetUnixTime();
                SleepingSince = ScriptTime;
                SleepTimer = new System.Timers.Timer();
                SleepTimer.Enabled = false;
                SleepTimer.AutoReset = false;
                Events = new Dictionary<string,ScriptEvent>();
            }
        }


        public class ScriptEvent
        {
            /// <summary>
            /// Event type, enumerated in EventTypes.*
            /// </summary>
            public EventTypes Type;
            /// <summary>
            /// Used for text-matching events, or events with messages attached
            /// </summary>
            public string Text;
            /// <summary>
            /// Used for events which contain a numerical value
            /// </summary>
            public int Number;
            /// <summary>
            /// Command to execute on the specified event
            /// </summary>
            public string Command;

            public ScriptEvent()
            {
                Type = EventTypes.NULL;
                Text = "";
                Number = 0;
                Command = "";
            }
        }


        //Used by scripted events
        public enum EventTypes
        {
            NULL = 0,
            //FIXME - add support for all of these events
            Connect = 1,
            Disconnect = 2,
            Chat = 3,
            IM = 4,
            GroupIM = 5,
            GetMoney = 6,
            GiveMoney = 7,
            TeleportFinish = 8
        }


        public static bool ParseLoginCommand(uint sessionNum, string[] cmd)
        {
            if (cmd.Length < 2) { Display.Help("login");  return false; }

            GhettoSL.UserSession Session = Interface.Sessions[Interface.CurrentSession];

            string flag = cmd[1].ToLower();

            if (flag == "-r")
            {
                if (cmd.Length != 2 && cmd.Length < 5) return false;

                if (cmd.Length > 2)
                {
                    Session.Settings.FirstName = cmd[2];
                    Session.Settings.LastName = cmd[3];
                    Session.Settings.Password = cmd[4];
                }
            }

            else if (flag == "-m")
            {
                if (cmd.Length < 5) return false;

                do Interface.CurrentSession++;
                while (Interface.Sessions.ContainsKey(Interface.CurrentSession));

                Display.InfoResponse(Interface.CurrentSession, "Creating session for " + Session.Name + "...");
                Interface.Sessions.Add(Interface.CurrentSession, Session);
                Session.SessionNumber = Interface.CurrentSession;

                Session = Interface.Sessions[Interface.CurrentSession];                
                Session.Settings.FirstName = cmd[2];
                Session.Settings.LastName = cmd[3];
                Session.Settings.Password = cmd[4];
            }

            else if (cmd.Length < 4)
            {
                return false;
            }

            else
            {
                Session.Settings.FirstName = cmd[1];
                Session.Settings.LastName = cmd[2];
                Session.Settings.Password = cmd[3];
            }

            if (Session.Client.Network.Connected)
            {
                //FIXME - Add RelogTimer to UserSession, in place of this
                Session.Client.Network.Logout();
                Thread.Sleep(1000);
            }

            Session.Login();
            return true;

        }


        public static bool ParseCommand(uint sessionNum, string commandToParse, bool parseVariables, bool fromMasterIM)
        {
            //FIXME - change display output if fromMasterIM == true

            GhettoSL.UserSession Session = Interface.Sessions[sessionNum];

            char[] splitChar = { ' ' };
            string[] cmd = commandToParse.Split(splitChar);
            string command = cmd[0].ToLower();
            int tok = 1;
            if (command == "im") tok++;
            string details = "";
            for (; tok < cmd.Length; tok++)
            {
                details += cmd[tok];
                if (tok + 1 < cmd.Length) details += " ";
            }

            if (command == "anim")
            {
                if (cmd.Length < 2) { Display.Help(command); return false; }
                Session.Client.Self.AnimationStart(new LLUUID(cmd[1]));
            }

            else if (command == "balance")
            {
                Session.Client.Self.RequestBalance();
            }

            else if (command == "camp")
            {
                uint localID = FindObjectByText(sessionNum, details.ToLower());
                if (localID > 0)
                {
                    Display.InfoResponse(sessionNum, "Match found. Camping...");
                    Session.Client.Self.RequestSit(Session.Prims[localID].ID, LLVector3.Zero);
                    Session.Client.Self.Sit();
                    //Session.Client.Self.Status.Controls.FinishAnim = false;
                    Session.Client.Self.Status.Controls.SitOnGround = false;
                    Session.Client.Self.Status.Controls.StandUp = false;
                    Session.Client.Self.Status.SendUpdate();
                    return true;
                }
                Display.InfoResponse(sessionNum, "No matching objects found.");
                return false; ;
            }

            else if (command == "clear")
            {
                Console.Clear();
            }

            else if (command == "fly")
            {
                Session.Client.Self.Status.Controls.Fly = true;
                Session.Client.Self.Status.SendUpdate();
            }

            else if (command == "go")
            {
                //FIXME - readd autopilot
                int x = 0;
                int y = 0;
                float z = 0f;

                if (cmd.Length < 3 || !int.TryParse(cmd[1], out x) || !int.TryParse(cmd[2], out y)) {
                    Display.Help(command);
                    return false;
                }

                if (cmd.Length < 4 || !float.TryParse(cmd[4], out z)) z = Session.Client.Self.Position.Z;
                
                Session.Client.Self.AutoPilotLocal(x, y, z);
            }

            else if (command == "help")
            {
                string topic = "";
                if (cmd.Length > 1) topic = cmd[1];
                Display.Help(topic);
            }

            else if (command == "im")
            {
                Session.Client.Self.InstantMessage(new LLUUID(cmd[1]), details);
            }

            else if (command == "land")
            {
                Session.Client.Self.Status.Controls.Fly = false;
                Session.Client.Self.Status.SendUpdate();
            }

            else if (command == "listen")
            {
                Session.Settings.DisplayChat = true;
            }

            else if (command == "login")
            {
                if (!ParseLoginCommand(sessionNum, cmd))
                {
                    Display.InfoResponse(sessionNum, "Invalid login parameters");
                }
            }

            else if (command == "look")
            {
                string weather = Display.RPGWeather(Session);
                Display.InfoResponse(sessionNum, weather);
            }

            else if (command == "payme")
            {
                int amount;
                if (cmd.Length < 2 || !int.TryParse(cmd[1], out amount))
                {
                    Display.Help(command);
                    return false;
                }
                else if (Session.Settings.MasterID == LLUUID.Zero)
                {
                    Display.Error(sessionNum, "MasterID not defined");
                    return false;
                }
                else
                {
                    Session.Client.Self.GiveMoney(Session.Settings.MasterID, amount, "");
                }
            }

            else if (command == "quiet")
            {
                Session.Settings.DisplayChat = false;
            }

            else if (command == "relog")
            {
                if (Session.Client.Network.Connected)
                {
                    //FIXME - Add RelogTimer to UserSession, in place of this
                    Session.Client.Network.Logout();
                    Thread.Sleep(1000);
                }

                Session.Login();
            }

            else if (command == "ride")
            {
                RideWith(sessionNum, details);
            }

            else if (command == "run")
            {
                Session.Client.Self.SetAlwaysRun(true);
            }

            else if (command == "say")
            {
                Session.Client.Self.Chat(details, 0, MainAvatar.ChatType.Normal);
            }

            else if (command == "shout")
            {
                Session.Client.Self.Chat(details, 0, MainAvatar.ChatType.Shout);
            }

            else if (command == "stopanim")
            {
                if (cmd.Length < 2) { Display.Help(command); return false; }
                Session.Client.Self.AnimationStop(new LLUUID(cmd[1]));
            }

            else if (command == "whisper")
            {
                Session.Client.Self.Chat(details, 0, MainAvatar.ChatType.Whisper);
            }

            else if (command == "s" || command == "session")
            {
                if (cmd.Length > 1)
                {
                    uint switchTo;
                    if (!uint.TryParse(cmd[1], out switchTo) || switchTo < 1)
                    {
                        Display.InfoResponse(sessionNum, "Invalid session number");
                        return false;
                    }
                    else Interface.CurrentSession = switchTo;
                }
                else Display.SessionList();
            }

            else if (command == "sit")
            {
                if (cmd.Length < 2) { Display.Help(command); return false; }
                Session.Client.Self.Status.Controls.SitOnGround = false;
                Session.Client.Self.Status.Controls.StandUp = false;
                Session.Client.Self.RequestSit(new LLUUID(cmd[1]), LLVector3.Zero);
                Session.Client.Self.Sit();
                Session.Client.Self.Status.SendUpdate();
            }

            else if (command == "sitg")
            {
                Session.Client.Self.Status.Controls.SitOnGround = true;
                Session.Client.Self.Status.SendUpdate();
                Session.Client.Self.Status.Controls.SitOnGround = false;
            }

            else if (command == "stand")
            {
                Session.Client.Self.Status.Controls.SitOnGround = false;
                Session.Client.Self.Status.Controls.StandUp = true;
                Session.Client.Self.Status.SendUpdate();
                Session.Client.Self.Status.Controls.StandUp = false;
            }

            else if (command == "stats")
            {
                Display.Stats(sessionNum);
            }

            else if (command == "touchid")
            {
                if (cmd.Length < 2) { Display.Help(command); return false; }
                uint touchid;
                if (uint.TryParse(cmd[1], out touchid)) Session.Client.Self.Touch(touchid);
            }

            else if (command == "quit")
            {
                Display.InfoResponse(sessionNum, "Closing session " + sessionNum + "...");
                Session.Client.Network.Logout();
            }

            else if (command == "teleport")
            {
                if (cmd.Length < 2) return false;
                string simName;
                LLVector3 tPos;
                if (cmd.Length >= 5)
                {
                    simName = String.Join(" ", cmd, 1, cmd.Length - 4);
                    float x = float.Parse(cmd[cmd.Length - 3]);
                    float y = float.Parse(cmd[cmd.Length - 2]);
                    float z = float.Parse(cmd[cmd.Length - 1]);
                    tPos = new LLVector3(x, y, z);
                }
                else
                {
                    simName = details;
                    tPos = new LLVector3(128, 128, 0);
                }

                Display.Teleporting(sessionNum, simName);
                Session.Client.Self.Teleport(simName, tPos);

                return true;
            }
            else if (command == "touch")
            {
                LLUUID findID = new LLUUID(cmd[1]);
                foreach (PrimObject prim in Session.Prims.Values)
                {
                    if (prim.ID != findID) continue;
                    Session.Client.Self.Touch(prim.LocalID);
                    //FIXME - notify touched
                    break;
                }
                //FIXME - notify could not find
            }

            else if (command == "updates")
            {
                if (cmd.Length < 2) { Display.Help(command); return false; }
                string toggle = cmd[1].ToLower();
                if (toggle == "on")
                {
                    Display.InfoResponse(sessionNum, "Update timer ON");
                    Session.Client.Self.Status.UpdateTimer.Start();
                }
                else if (toggle == "off")
                {
                    Display.InfoResponse(sessionNum, "Update timer OFF");
                    Session.Client.Self.Status.UpdateTimer.Stop();
                }
                else { Display.Help(command); return false; }
            }

            else if (command == "walk")
            {
                Session.Client.Self.SetAlwaysRun(false);
            }

            else if (command == "who")
            {
                Display.Who(sessionNum);
            }
            else
            {
                Display.InfoResponse(0, "Unknown command: " + command.ToUpper());
                return false;
            }

            return true;
        }


        public static uint FindObjectByText(uint sessionNum, string textValue)
        {
            uint localID = 0;
            foreach (PrimObject prim in Interface.Sessions[sessionNum].Prims.Values)
            {
                int len = textValue.Length;
                string match = prim.Text.Replace("\n", ""); //Strip newlines
                if (match.Length < len) continue; //Text is too short to be a match
                else if (match.Substring(0, len).ToLower() == textValue)
                {
                    localID = prim.LocalID;
                    break;
                }
            }
            return localID;
        }


        public static bool RideWith(uint sessionNum, string name)
        {
            string findName = name.ToLower();

            GhettoSL.UserSession Session = Interface.Sessions[sessionNum];

            foreach (Avatar av in Session.Avatars.SimLocalAvatars().Values)
            {

                if (av.Name.ToLower().Substring(0, findName.Length) == findName)
                {
                    if (av.SittingOn > 0)
                    {
                        //FIXME - request if fails
                        if (!Session.Prims.ContainsKey(av.SittingOn))
                        {
                            Display.InfoResponse(sessionNum, "Object info missing");
                            return false;
                        }
                        Display.InfoResponse(sessionNum, "Riding with " + av.Name + ".");
                        Session.Client.Self.RequestSit(Session.Prims[av.SittingOn].ID, LLVector3.Zero);
                        Session.Client.Self.Sit();
                        return true;
                    }
                    else
                    {
                        Console.WriteLine(av.Name + " is not sitting.");
                        return false;
                    }
                }
            }
            return false;
        }


    }
}
