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

using OpenMetaverse;
using OpenMetaverse.Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ghetto
{
    class Parse
    {

        public static bool Conditions(uint sessionNum, string conditions)
        {
            //FIXME - actually parse paren grouping instead of just stripping parens
            //FIXME - possible code injection point
            string[] splitSpace = { " " };
            string c = String.Join(" ", conditions.Replace("(", "").Replace(")", "").Trim().Split(splitSpace, StringSplitOptions.RemoveEmptyEntries));

            bool pass = true;

            string[] splitLike = { " iswm ", " ISWM " };
            string[] splitMatch = { " match ", " MATCH " };
            string[] splitAnd = { " and ", " AND ", "&&" };
            string[] splitOr = { " or ", " OR ", " || " };
            string[] splitEq = { " == ", " = " };
            string[] splitNot = { " != ", " <> " };
            string[] splitLT = { " < " };
            string[] splitGT = { " > " };
            string[] splitLE = { " <= ", " =< " };
            string[] splitGE = { " >= ", " => " };

            string[] condOr = Variables(sessionNum, c.Trim(), "").Split(splitOr, StringSplitOptions.RemoveEmptyEntries);

            foreach (string or in condOr)
            {
                pass = true;

                string[] condAnd = or.Trim().Split(splitAnd, StringSplitOptions.RemoveEmptyEntries);
                foreach (string and in condAnd)
                {
                    string[] not = and.ToLower().Split(splitNot, StringSplitOptions.None);
                    string[] eq = and.ToLower().Split(splitEq, StringSplitOptions.None);
                    string[] like = and.ToLower().Split(splitLike, StringSplitOptions.None);
                    string[] match = and.ToLower().Split(splitMatch, StringSplitOptions.None);
                    string[] less = and.ToLower().Split(splitLT, StringSplitOptions.None);
                    string[] greater = and.ToLower().Split(splitGT, StringSplitOptions.None);
                    string[] lessEq = and.ToLower().Split(splitLE, StringSplitOptions.None);
                    string[] greaterEq = and.ToLower().Split(splitGE, StringSplitOptions.None);

                    //only one term (like a number or $true or $false)
                    if (eq.Length == 1 && not.Length == 1 && less.Length == 1 && greater.Length == 1 && lessEq.Length == 1 && greaterEq.Length == 1 && like.Length == 1 && match.Length == 1)
                    {
                        string val = eq[0].Trim();
                        if (val == "$false" || val == "$null" || val == "0") { pass = false; break; }
                        continue;
                    }

                    //check "iswm" (wildcards, which are converted to regex)
                    if (like.Length > 1)
                    {
                        string v1 = like[0].Trim();
                        string v2 = like[1].Trim();
                        if (v1.Length == 0) v1 = "$null";
                        if (v2.Length == 0) v2 = "$null";
                        string regex = "^" + Regex.Escape(v2).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                        bool isMatch = Regex.IsMatch(v1, regex, RegexOptions.IgnoreCase);
                        Console.WriteLine("Comparing {0} ISWM {1} == {2}", v1, v2, isMatch); //DEBUG
                        if (!isMatch) { pass = false; break; }
                        continue;
                    }

                    //check "match" (regex)
                    if (match.Length > 1)
                    {
                        string v1 = match[0].Trim();
                        string v2 = match[1].Trim();
                        try
                        {
                            bool isMatch = Regex.IsMatch(v1, v2, RegexOptions.IgnoreCase);
                            if (!isMatch) { pass = false; break; }
                        }
                        catch
                        {
                            Display.Error(sessionNum, "/if: invalid regular expression");
                            return false;
                        }
                        //Console.WriteLine("Comparing {0} MATCH {1} == {2}", v1, v2, isMatch); //DEBUG

                        continue;
                    }

                    //check ==
                    if (eq.Length > 1)
                    {
                        string v1 = eq[0].Trim();
                        string v2 = eq[1].Trim();
                        //Console.WriteLine("comparing ==: " + v1 + " vs. " + v2); //DEBUG
                        if (v1 != v2) { pass = false; break; }
                        continue;
                    }

                    //check !=
                    if (not.Length > 1)
                    {
                        string v1 = not[0].Trim();
                        string v2 = not[1].Trim();
                        //Console.WriteLine("comparing !=: " + v1 + " vs. " + v2); //DEBUG
                        if (v1 == v2) { pass = false; break; }
                        continue;
                    }

                    float val1;
                    float val2;

                    //check <
                    if (less.Length > 1)
                    {
                        if (!float.TryParse(less[0].Trim(), out val1) || !float.TryParse(less[1].Trim(), out val2) || val1 >= val2) { pass = false; break; }
                        continue;
                    }

                    //check >
                    if (greater.Length > 1)
                    {
                        if (!float.TryParse(greater[0].Trim(), out val1) || !float.TryParse(greater[1].Trim(), out val2) || val1 <= val2) { pass = false; break; }
                        continue;
                    }

                    //check <=
                    if (lessEq.Length > 1)
                    {
                        if (!float.TryParse(lessEq[0].Trim(), out val1) || !float.TryParse(lessEq[1].Trim(), out val2) || val1 > val2) { pass = false; break; }
                        continue;
                    }

                    //check >=
                    if (greaterEq.Length > 1)
                    {
                        if (!float.TryParse(greaterEq[0].Trim(), out val1) || !float.TryParse(greaterEq[1].Trim(), out val2) || val1 < val2) { pass = false; break; }
                        continue;
                    }

                }

                if (pass) return true; //FIXME - not sure if this is right

            }

            return pass;
        }


        public static string Variables(uint sessionNum, string originalString, string scriptName)
        {
            GhettoSL.UserSession Session = Interface.Sessions[sessionNum];
            string ret = originalString;

            //parse $identifiers
            ret = ret.Replace("$script", scriptName);
            ret = ret.Replace("$nullkey", UUID.Zero.ToString());
            ret = ret.Replace("$myfirst", Session.Settings.FirstName);
            ret = ret.Replace("$mylast", Session.Settings.LastName);
            ret = ret.Replace("$myname", Session.Name);
            ret = ret.Replace("$mypos", Session.Client.Self.SimPosition.ToString());
            ret = ret.Replace("$mypos.x", Session.Client.Self.SimPosition.X.ToString());
            ret = ret.Replace("$mypos.y", Session.Client.Self.SimPosition.Y.ToString());
            ret = ret.Replace("$mypos.z", Session.Client.Self.SimPosition.Z.ToString());
            ret = ret.Replace("$session", Session.SessionNumber.ToString());
            ret = ret.Replace("$master", Session.Settings.MasterID.ToString());
            ret = ret.Replace("$balance", Session.Balance.ToString());
            ret = ret.Replace("$earned", Session.MoneyReceived.ToString());
            ret = ret.Replace("$spent", Session.MoneySpent.ToString());

            if (scriptName != "")
            {
                ScriptSystem.UserScript script = Interface.Scripts[scriptName];
                uint elapsed = Utils.GetUnixTime() - script.SetTime;
                ret = ret.Replace("$elapsed", elapsed.ToString());
                uint sittingOn = Session.Client.Self.SittingOn;
                Vector3 myPos = Session.Client.Self.SimPosition;
                ret = ret.Replace("$target", script.SetTarget.ToString());
                ret = ret.Replace("$distance", Vector3.Distance(myPos, script.SetTarget).ToString());
            }

            if (Session.Client.Network.Connected && Session.Client.Self.SittingOn > 0 && Session.Prims.ContainsKey(Session.Client.Self.SittingOn))
            {
                ret = ret.Replace("$seattext", Session.Prims[Session.Client.Self.SittingOn].Text);
                ret = ret.Replace("$seatid", Session.Prims[Session.Client.Self.SittingOn].ID.ToString());
            }
            else ret = ret.Replace("$seattext", "$null").Replace("$seatid", UUID.Zero.ToString());
            if (Session.Client.Network.Connected)
            {
                ret = ret.Replace("$myid", Session.Client.Self.AgentID.ToString());
                ret = ret.Replace("$connected", "$true");
                ret = ret.Replace("$region", Session.Client.Network.CurrentSim.Name);
            }
            else
            {
                ret = ret.Replace("$myid", UUID.Zero.ToString());
                ret = ret.Replace("$connected", "$false");
                ret = ret.Replace("$region", "$null");
                ret = ret.Replace("$distance", "$null");
            }            

            if (Session.Client.Self.Movement.AlwaysRun) ret = ret.Replace("$flying", "$true");
            else ret = ret.Replace("$flying", "$false");

            if (Session.Client.Self.Movement.Fly) ret = ret.Replace("$flying", "$true");
            else ret = ret.Replace("$flying", "$false");

            if (Session.Client.Self.SittingOn > 0) ret = ret.Replace("$sitting", "$true");
            else ret = ret.Replace("$sitting", "$false");

            //parse %vars
            if (scriptName != "")
            {
                foreach (KeyValuePair<string, string> var in Interface.Scripts[scriptName].Variables)
                {
                    ret = ret.Replace(var.Key, var.Value);
                }
            }

            ret = ret.Replace("$null", "");
            return ret;
        }


        public static string Tokens(string originalString, string message)
        {
            string[] splitChar = { " " };
            string[] orig = originalString.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);
            string[] tok = message.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);

            //FIXME - parse all $1 $2 etc

            string newString = originalString;


            for (int i = 64; i > 0; i--)
            {
                if (tok.Length >= i) newString = newString.Replace("$" + i, tok[i - 1]);
                else newString = newString.Replace("$" + i, "");
                string[] spaceChar = { " " };
                newString = String.Join(" ", newString.Split(spaceChar, StringSplitOptions.RemoveEmptyEntries));
            }

            /*
            foreach (string o in orig)
            {
                int number;
                if (o.Substring(0, 1) == "$" && int.TryParse(o.Substring(1), out number))
                {
                    if (newString != "") newString += " ";
                    if (tok.Length >= number) newString += tok[number - 1];
                }
                else
                {
                    if (newString != "") newString += " ";
                    newString += o;
                }
            }
            */

            return newString;
        }


        public static bool LoginCommand(uint sessionNum, string[] cmd)
        {
            GhettoSL.UserSession Session = Interface.Sessions[sessionNum];
            uint newSession = sessionNum;

            if (cmd.Length < 2) { Display.Help("login"); return false; }

            string flag = cmd[1].ToLower();
            int index = 0;

            if (flag == "-r")
            {
                if (cmd.Length != 2 && cmd.Length < 5) return false;

                if (cmd.Length > 2)
                {
                    Session.Settings.FirstName = cmd[2];
                    Session.Settings.LastName = cmd[3];
                    Session.Settings.Password = cmd[4];
                    index = 5;
                }
            }

            else if (flag == "-m")
            {
                if (cmd.Length < 5) return false;

                do newSession++;
                while (Interface.Sessions.ContainsKey(newSession));

                Interface.Sessions.Add(newSession, new GhettoSL.UserSession(Interface.CurrentSession));

                Session = Interface.Sessions[newSession];
                Session.SessionNumber = newSession;
                Session.Settings.FirstName = cmd[2];
                Session.Settings.LastName = cmd[3];
                Session.Settings.Password = cmd[4];
                Display.InfoResponse(newSession, "Creating session for " + Session.Name + "...");

                index = 5;
            }

            else if (cmd.Length < 4)
            {
                //no flags, but we should still have the command + 3 args
                return false;
            }

            else
            {
                Session.Settings.FirstName = cmd[1];
                Session.Settings.LastName = cmd[2];
                Session.Settings.Password = cmd[3];
                index = 4;
            }

            for (bool lastArg = false; index < cmd.Length; index++, lastArg = false)
            {
                string arg = cmd[index];
                if (index >= cmd.Length) lastArg = true;

                if (arg == "-q" || arg == "-quiet")
                    Session.Settings.DisplayChat = false;
                else if (!lastArg && (arg == "-m" || arg == "-master" || arg == "-masterid"))
                {
                    UUID master;
                    if (UUID.TryParse(cmd[index + 1], out master)) Session.Settings.MasterID = master;
                }
                else if (arg == "-n" || arg == "-noupdates")
                    Session.Settings.SendUpdates = false;
                else if (!lastArg && (arg == "-p" || arg == "-pass" || arg == "-passphrase"))
                    Session.Settings.PassPhrase = ScriptSystem.QuoteArg(cmd, index + 1);
                else if (arg == "-home")
                {
                    Session.Settings.StartLocation = "home";
                }
                else if (arg == "-here")
                {
                    GhettoSL.UserSession fromSession = Interface.Sessions[sessionNum];
                    if (fromSession.Client.Network.Connected)
                    {
                        Session.Settings.StartLocation = "uri:" + fromSession.Client.Network.CurrentSim.Name
                            + "&" + (int)fromSession.Client.Self.SimPosition.X
                            + "&" + (int)fromSession.Client.Self.SimPosition.Y
                            + "&" + (int)fromSession.Client.Self.SimPosition.Z;
                    }
                    else
                    {
                        Session.Settings.StartLocation = fromSession.Settings.StartLocation;
                    }
                }
                else if (arg.Length > 13 && arg.Substring(0, 13) == "secondlife://")
                {
                    string url = ScriptSystem.QuoteArg(cmd, index);
                    Session.Settings.StartLocation = "uri:" + url.Substring(13, arg.Length - 13).Replace("%20", " ").ToLower().Replace("/", "&");
                }
            }

            Interface.CurrentSession = newSession;

            if (Session.Client.Network.Connected)
            {
                //FIXME - Add RelogTimer to UserSession, in place of this
                if (Session != null) Session.Client.Network.Logout();
                Thread.Sleep(1000);
                //Session.Client = new SecondLife(); //FIXME - clear client data when relogging
            }

            return true;

        }


        /// <summary>
        /// /script command
        /// </summary>
        public static bool LoadScriptCommand(uint sessionNum, string[] cmd)
        {
            GhettoSL.UserSession Session = Interface.Sessions[sessionNum];

            if (cmd.Length == 1 || cmd[1] == "help")
            {
                //FIXME - add more verbose script help to Display
                Display.Help(cmd[0]);
                return false;
            }

            string arg = cmd[1].ToLower();
            string scriptFile = cmd[1];

            if (arg == "-u" || arg == "-unload" || arg == "unload")
            {
                if (cmd.Length < 3) { Display.Help(arg); return false; }
                scriptFile = cmd[2];
                if (!Interface.Scripts.ContainsKey(cmd[2])) Display.InfoResponse(sessionNum, "No such script loaded. For a list of active scripts, use /scripts.");
                else
                {
                    lock (Interface.Scripts)
                    {
                        Interface.Scripts.Remove(cmd[2]);
                        Display.InfoResponse(sessionNum, "Unloaded script: " + cmd[2]);
                    }
                }
                
                if (scriptFile == cmd[2]) return false; //script unloaded itself
                else return true;
            }

            if (!File.Exists(scriptFile))
            {
                if (!File.Exists(scriptFile + ".script"))
                {
                    Display.Error(sessionNum, "File not found: " + scriptFile);
                    return false;
                }
                else scriptFile = scriptFile + ".script";
            }

            if (Interface.Scripts.ContainsKey(scriptFile))
            {
                //scriptFile is already loaded. refresh.
                Display.InfoResponse(0, "Reloading script: " + scriptFile);
                Interface.Scripts[scriptFile] = new ScriptSystem.UserScript(0, scriptFile);
            }
            else
            {
                //add entry for scriptFile
                Display.InfoResponse(0, "Loading script: " + scriptFile);
                Interface.Scripts.Add(scriptFile, new ScriptSystem.UserScript(0, scriptFile));
            }
            int loaded = Interface.Scripts[scriptFile].Load(scriptFile);
            if (loaded < 0)
            {
                Display.Error(0, "Error opening " + scriptFile + " (invalid file name or file in use)");
                return false;
            }
            else if (loaded > 0)
            {
                Display.Error(0, "Error on line " + loaded + " of script " + scriptFile);
                return false;
            }
            else
            {
                ScriptSystem.UserScript script = Interface.Scripts[scriptFile];
                int a = script.Aliases.Count;
                int e = script.Events.Count;
                Display.InfoResponse(0, "Loaded " + a + " alias(es) and " + e + " event(s).");
                //ScriptSystem.TriggerEvents(sessionNum, ScriptSystem.EventTypes.Load, null);
                if (script.Events.ContainsKey(ScriptSystem.EventTypes.Load))
                {
                    CommandArray(sessionNum, scriptFile, script.Events[ScriptSystem.EventTypes.Load].Commands, null);
                }
                return true;
            }
        }


        public static void CommandArray(uint sessionNum, string scriptName, string[] commands, Dictionary<string,string> identifiers)
        {
            bool checkElse = false;
            foreach (string command in commands)
            {
                if (command.Substring(0, 1) == ";") continue;

                //FIXME - make sure we don't need to trim command first
                string[] splitSpace = { " " };
                string[] cmd = command.Split(splitSpace, StringSplitOptions.RemoveEmptyEntries);
                string arg = cmd[0].ToLower();

                if (!checkElse)
                {
                    if (arg == "elseif" || arg == "else") continue;
                }

                checkElse = false;

                string newCommand = command; //FIXME - parse tokens and variables

                //Gets $1 $2 $3 etc from $message
                string tokmessage = "";
                if (identifiers != null && identifiers.ContainsKey("$message")) tokmessage = identifiers["$message"];
                else tokmessage = "";
                newCommand = Parse.Tokens(newCommand, tokmessage);

                if (identifiers != null)
                {
                    foreach (KeyValuePair<string, string> pair in identifiers)
                        newCommand = newCommand.Replace(pair.Key, pair.Value);
                }
                ScriptSystem.CommandResult result = Parse.Command(sessionNum, scriptName, newCommand, true, false);
                if (arg == "if" || arg == "elseif")
                {
                    if (result == ScriptSystem.CommandResult.ConditionFailed) checkElse = true;
                    else checkElse = false;
                }
            }
        }

        public static ScriptSystem.CommandResult Command(uint sessionNum, string scriptName, string commandString, bool parseVariables, bool fromMasterIM)
        {

            //DEBUG - testing scriptName value
            //if (scriptName != "") Console.WriteLine("({0}) [{1}] SCRIPTED COMMAND: {2}", sessionNum, scriptName, commandString);
            //FIXME - change display output if fromMasterIM == true

            GhettoSL.UserSession Session = Interface.Sessions[sessionNum];

            if (scriptName != "" && !Interface.Scripts.ContainsKey(scriptName)) return ScriptSystem.CommandResult.UnexpectedError; //invalid or unloaded script

            //First we clean up the original command string, removing whitespace and command slashes
            string commandToParse = commandString.Trim();
            while (commandToParse.Length > 0 && commandToParse.Substring(0, 1) == "/")
            {
                commandToParse = commandToParse.Substring(1).Trim();
                if (commandToParse.Length == 0) return ScriptSystem.CommandResult.NoError;
            }

            //Next we save the unparsed command, split it by spaces, for commands like /set and /inc
            char[] splitChar = { ' ' };
            string[] unparsedCommand = commandToParse.Split(splitChar, StringSplitOptions.RemoveEmptyEntries);

            //Now we parse the variables
            if (parseVariables)
            {
                commandToParse = Variables(sessionNum, commandString, scriptName);
            }

            //Next we split again. This time everthing is parsed.
            string[] cmd = commandToParse.Trim().Split(splitChar);
            string command = cmd[0].ToLower();
            int commandStart = 0;

            //Time to check for IF statements and check for validity and pass/fail
            //If they are invalid, the entire function will return false, halting a parent script.
            //If they are valid but just fail, the command will halt, but it returns true.
            //FIXME - add "else" and multi-line "if/end if" routines
            if (command == "if" || command == "elseif")
            {
                string conditions = "";
                string[] ifCmd = new string[0];
                for (int i = 1; i < cmd.Length; i++)
                {
                    if (commandStart > 0) //already found THEN statement, adding to command string
                    {
                        Array.Resize(ref ifCmd, ifCmd.Length + 1);
                        ifCmd[ifCmd.Length - 1] = cmd[i];
                    }
                    else if (cmd[i].ToLower() == "then") //this is our THEN statement
                    {
                        if (i >= cmd.Length - 1)
                        {
                            Display.Error(sessionNum, "Script error: Missing statement after THEN");
                            return ScriptSystem.CommandResult.InvalidUsage;
                        }
                        commandStart = i + 1;
                    }
                    else if (i >= cmd.Length - 1)
                    {
                        Display.Error(sessionNum, "Script error: IF without THEN");
                        return ScriptSystem.CommandResult.InvalidUsage;
                    }
                    else
                    {
                        if (conditions != "") conditions += " ";
                        conditions += cmd[i];
                    }
                }
                if (!Conditions(sessionNum, conditions)) return ScriptSystem.CommandResult.ConditionFailed; //condition failed, but no errors
                cmd = ifCmd;
                command = cmd[0].ToLower();
            }
            else if (command == "else")
            {
                string c = "";
                for (int i = 1; i < cmd.Length; i++)
                {
                    if (c != "") c += " ";
                    c +=  cmd[i];
                }
                cmd = c.Split(splitChar);
                if (cmd.Length > 0) command = cmd[0];
                else return ScriptSystem.CommandResult.InvalidUsage;
            }

            //The purpose of this part is to separate the message from the rest of the command.
            //For example, in the command "im some-uuid-here Hi there!", details = "Hi there!"
            string details = "";
            int detailsStart = 1;
            if (command == "cam" || command == "estate" || command == "popup" || command == "busyim" || command == "im" || command == "lure" || command == "re" || command == "s" || command == "session" || command == "paybytext" || command == "paybyname") detailsStart++;
            else if (command == "dialog") detailsStart += 2;
            else if (command == "timer" && cmd.Length > 1)
            {
                if (cmd.Length > 1)
                {
                    if (cmd[1].Substring(0, 1) == "-") detailsStart += 4;
                    else detailsStart += 3;
                }
            }
            while (detailsStart < cmd.Length)
            {
                if (details != "") details += " ";
                details += cmd[detailsStart];
                detailsStart++;
            }
            
            //Check for user-defined aliases
            lock (Interface.Scripts)
            {
                foreach (ScriptSystem.UserScript s in Interface.Scripts.Values)
                {
                    foreach (KeyValuePair<string, string[]> pair in s.Aliases)
                    {
                        if (command == pair.Key.ToLower())
                        {
                            //ScriptSystem.CommandResult result = ScriptSystem.CommandResult.NoError;

                            Dictionary<string, string> identifiers = new Dictionary<string, string>();
                            identifiers.Add("$message", details);
                            Parse.CommandArray(sessionNum, s.ScriptName, pair.Value, identifiers);

                            //foreach (string c in pair.Value)
                            //{
                            //    string ctok = Tokens(c, details);
                            //    ScriptSystem.CommandResult aResult = Command(sessionNum, s.ScriptName, ctok, true, fromMasterIM);
                            //    if (aResult != ScriptSystem.CommandResult.NoError && aResult != ScriptSystem.CommandResult.ConditionFailed) return result;
                            //}

                            return ScriptSystem.CommandResult.NoError; //FIXME - make Parse.CommandArray return a CommandResult
                        }
                    }
                }
            }

            if (!Session.Client.Network.Connected)
            {
                string[] okIfNotConnected = { "clear", "debug", "echo", "exit", "help", "http", "login", "inc", "quit", "relog", "return", "s", "session", "sessions", "set", "script", "scripts", "stats", "timer", "timers" };
                int ok;
                for (ok = 0; ok < okIfNotConnected.Length; ok++)
                {
                    if (okIfNotConnected[ok] == command) break;
                }
                if (ok == okIfNotConnected.Length)
                {
                    Display.Error(sessionNum, "/" + command + ": Not connected");
                    return ScriptSystem.CommandResult.UnexpectedError;
                }
            }

            //Check for "/1 text" for channel 1, etc

            int chatChannel;
            if (int.TryParse(cmd[0], out chatChannel))
            {
                Session.Client.Self.Chat(details, chatChannel, ChatType.Normal);
                Display.SendMessage(sessionNum, chatChannel, UUID.Zero, details);
            }

            //And on to the actual commands...

            else if (command == "anim")
            {
                UUID anim;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out anim))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.AnimationStart(anim, false);
            }

            else if (command == "answer")
            {
                int channel = Session.LastDialogChannel;
                UUID id = Session.LastDialogID;
                if (cmd.Length < 2) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                else if (channel < 0 || id == UUID.Zero) Display.Error(sessionNum, "No dialogs received. Try /dialog <channel> <id> <message>.");
                else if (Command(sessionNum, scriptName, "dialog " + channel + " " + id + " " + details, parseVariables, fromMasterIM) == ScriptSystem.CommandResult.NoError)
                {
                    Display.InfoResponse(sessionNum, "Dialog reply sent.");
                }
            }

            else if (command == "balance")
            {
                Session.Client.Self.RequestBalance();
            }

            else if (command == "restartsim")
            {
                Session.Client.Network.CurrentSim.Estate.RestartRegion();
            }

            else if (command == "cancelrestart")
            {
                Session.Client.Network.CurrentSim.Estate.CancelRestart();
            }

            else if (command == "debug")
            {
                int mode = Session.Debug;
                if (!int.TryParse(cmd[1], out mode))
                {
                    if (cmd[1].ToLower() == "on") mode = 1;
                    else if (cmd[1].ToLower() == "off") mode = 0;
                }
                Session.Debug = mode;
                Display.InfoResponse(sessionNum, "Debug level: " + mode);
            }

            else if (command == "detachall")
            {
                //FIXME - detach all worn objects
                List<uint> attachments = new List<uint>();
                foreach (KeyValuePair<uint, Primitive> pair in Session.Prims)
                {
                    if (pair.Value.ParentID == Session.Client.Self.LocalID) attachments.Add(pair.Value.LocalID);
                }
                Session.Client.Objects.DetachObjects(Session.Client.Network.CurrentSim, attachments);
                Display.InfoResponse(sessionNum, "Detached " + attachments.Count + " objects");
            }

            else if (command == "return")
            {
                return ScriptSystem.CommandResult.Return;
            }

            else if (command == "paybytext" || command == "paybyname")
            {
                int amount;
                if (cmd.Length < 3 || !int.TryParse(cmd[1], out amount))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                uint localID = 0;
                UUID uuid = UUID.Zero;

                Console.WriteLine(command + " " + details);

                if (command == "paybytext")
                {
                    localID = Session.FindObjectByText(details.ToLower());
                    if (!Session.Prims.ContainsKey(localID))
                    {
                        Display.Error(sessionNum, "Missing info for local ID " + localID);
                        return ScriptSystem.CommandResult.UnexpectedError; //FIXME - should this return false and stop scripts?
                    }
                    uuid = Session.Prims[localID].ID;
                }
                else if (command == "paybyname")
                {
                    localID = Session.FindAgentByName(details.ToLower());
                    if (!Session.Avatars.ContainsKey(localID))
                    {
                        Display.Error(sessionNum, "Missing info for local ID " + localID);
                        return ScriptSystem.CommandResult.UnexpectedError; //FIXME - should this return false and stop scripts?
                    }
                    uuid = Session.Avatars[localID].ID;
                }
                else return ScriptSystem.CommandResult.UnexpectedError; //this should never happen

                if (localID > 0)
                {
                    Session.Client.Self.GiveMoney(uuid, amount, "", MoneyTransactionType.Gift, TransactionFlags.None);
                    Display.InfoResponse(sessionNum, "Paid L$" + amount + " to " + uuid);
                }

            }

            else if (command == "setmaster")
            {
                UUID master;
                if (cmd.Length != 2 || UUID.TryParse(cmd[1], out master)) return ScriptSystem.CommandResult.InvalidUsage;
                Session.Settings.MasterID = master;
                Display.InfoResponse(sessionNum, "Set master to " + cmd[1]);
            }

            else if (command == "settarget" && scriptName != "")
            {
                Vector3 target;
                if (cmd.Length < 2 || !Vector3.TryParse(details, out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                else Interface.Scripts[scriptName].SetTarget = target;
            }

            else if (command == "settime" && scriptName != "")
            {
                Interface.Scripts[scriptName].SetTime = Utils.GetUnixTime();
            }

            else if (command == "sitbytext")
            {
                if (cmd.Length < 2) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                uint localID = Session.FindObjectByText(details.ToLower());
                if (localID > 0)
                {
                    Display.InfoResponse(sessionNum, "Match found. Sitting...");
                    Session.Client.Self.RequestSit(Session.Prims[localID].ID, Vector3.Zero);
                    Session.Client.Self.Sit();
                    //Session.Client.Self.Movement.Controls.FinishAnim = false;
                    Session.Client.Self.Movement.SitOnGround = false;
                    Session.Client.Self.Movement.StandUp = false;
                    Session.Client.Self.Movement.SendUpdate();
                }
                else Display.InfoResponse(sessionNum, "No matching objects found.");
            }

            else if (command == "touchbytext")
            {
                if (cmd.Length < 2) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                uint localID = Session.FindObjectByText(details.ToLower());
                if (localID > 0)
                {
                    Display.InfoResponse(sessionNum, "Match found. Touching...");
                    Session.Client.Self.Touch(localID);
                }
                else Display.InfoResponse(sessionNum, "No matching objects found.");
            }

            else if (command == "clear")
            {
                Console.Clear();
            }

            else if (command == "crouch")
            {
                if (cmd.Length == 1 || cmd[1].ToLower() == "on")
                {
                    Session.Client.Self.Movement.UpNeg = true;
                    Session.Client.Self.Movement.SendUpdate();
                }
                else if (cmd.Length > 1 && cmd[1].ToLower() == "off")
                {
                    Session.Client.Self.Movement.UpNeg = false;
                    Session.Client.Self.Movement.FinishAnim = true;
                    Session.Client.Self.Movement.SendUpdate();
                    Session.Client.Self.Movement.FinishAnim = false;
                }
                else
                {
                    Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage;
                }
            }

            else if (command == "delete")
            {
                UUID itemid;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out itemid))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }

                if (!Session.Client.Inventory.Store.Contains(itemid))
                {
                    Display.Error(sessionNum, "Asset id not found in inventory cache");
                    return ScriptSystem.CommandResult.UnexpectedError;
                }

                InventoryBase item = Session.Client.Inventory.Store[itemid];
                string name = item.Name;
                //FIXME - reimplement
                //Session.Client.Inventory.Remove(item);

                Display.InfoResponse(sessionNum, "Deleted item \"" + name + "\".");
            }

            else if (command == "detach")
            {
                UUID itemid;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out itemid))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }

                if (!Session.Client.Inventory.Store.Contains(itemid))
                {
                    Display.Error(sessionNum, "Asset id not found in inventory cache");
                    return ScriptSystem.CommandResult.UnexpectedError;
                }

                InventoryItem item = (InventoryItem)Session.Client.Inventory.Store[itemid];

                string name = item.Name;
                Session.Client.Appearance.Detach(item);

                Display.InfoResponse(sessionNum, "Detached item \"" + name + "\".");
            }

            else if (command == "dialog")
            {
                int channel;
                UUID objectid;
                if (cmd.Length <= 3 || !int.TryParse(cmd[1], out channel) || !UUID.TryParse(cmd[2], out objectid))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Display.SendMessage(sessionNum, channel, objectid, details);
                Session.ScriptDialogReply(channel, objectid, details);
            }

            else if (command == "dir" || command == "ls")
            {
                //FIXME - remember folder and allow dir/ls without args
                //FIXME - move DirList function to UserSession and move output to Display class
                UUID folder;
                if (cmd.Length == 1) ScriptSystem.DirList(sessionNum, Session.Client.Inventory.Store.RootFolder.UUID);
                else if (!UUID.TryParse(cmd[1], out folder)) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                else ScriptSystem.DirList(sessionNum, folder);
            }

            else if (command == "dwell")
            {
                
            }

            else if (command == "echo")
            {
                //FIXME - move to Display.Echo
                if (cmd.Length < 1) return ScriptSystem.CommandResult.NoError;
                Console.WriteLine(details);
            }

            else if (command == "eban")
            {
                UUID target;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out target)) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                //FIXME - add display
                Session.Client.Network.CurrentSim.Estate.BanUser(target, false);
            }

            else if (command == "eunban")
            {
                UUID target;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out target)) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                //FIXME - add display
                Session.Client.Network.CurrentSim.Estate.UnbanUser(target, false);
            }

            else if (command == "ekick")
            {
                UUID target;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out target)) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                //FIXME - add display
                Session.Client.Network.CurrentSim.Estate.KickUser(target);
            }

            else if (command == "ekill")
            {
                UUID target;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out target)) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                Session.Client.Network.CurrentSim.Estate.TeleportHomeUser(target);
            }

            else if (command == "estate")
            {
                if (cmd.Length < 2) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                Session.Client.Network.CurrentSim.Estate.EstateOwnerMessage(cmd[1], details);
            }

            else if (command == "events")
            {
                if (cmd.Length == 1) Display.EventList(sessionNum);
            }

            else if (command == "exit")
            {
                Command(sessionNum, scriptName, "s -a quit", false, fromMasterIM);
                Interface.Exit = true;
            }

            else if (command == "fly")
            {
                if (cmd.Length == 1 || cmd[1].ToLower() == "on")
                {
                    if (Session.Client.Self.Movement.Fly)
                    {
                        Display.InfoResponse(sessionNum, "You are already flying.");
                    }
                    else
                    {
                        Session.Client.Self.Movement.Fly = true;
                        Display.InfoResponse(sessionNum, "Suddenly, you feel weightless...");
                    }
                }
                else if (cmd[1].ToLower() == "off")
                {
                    if (Session.Client.Self.Movement.Fly)
                    {
                        Session.Client.Self.Movement.Fly = false;
                        Display.InfoResponse(sessionNum, "You drop to the ground.");
                    }
                    else
                    {
                        Display.InfoResponse(sessionNum, "You are not flying.");
                    }
                }
                //Send either way, for good measure                
                Session.Client.Self.Movement.SendUpdate();
            }

            else if (command == "follow")
            {
                if (cmd.Length < 2) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                if (cmd.Length == 1 || cmd[1].ToLower() == "on")
                {
                    if (Session.FollowName == "")
                    {
                        Display.InfoResponse(sessionNum, "You are not following anyone.");
                    }
                    else
                    {
                        Session.Follow(Session.FollowName);
                    }
                }
                else if (cmd[1].ToLower() == "off")
                {
                    if (Session.FollowTimer.Enabled == true)
                    {
                        Session.FollowTimer.Stop();
                        Display.InfoResponse(sessionNum, "You stopped following " + Session.FollowName + ".");
                        Session.Client.Self.Movement.SendUpdate();
                    }
                    else
                    {
                        Display.InfoResponse(sessionNum, "You are not following.");
                    }
                }
                else Session.Follow(details);
            }

            else if (command == "friend")
            {
                UUID targetID;
                if (cmd.Length > 1 && cmd[1] == "-r")
                {
                    if (cmd.Length < 3 || !UUID.TryParse(cmd[2], out targetID)) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                    Session.Client.Friends.TerminateFriendship(targetID);
                }
                else
                {
                    if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out targetID)) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                    Session.Client.Friends.OfferFriendship(targetID);
                }
            }

            else if (command == "go")
            {
                Vector3 target;
                if (cmd.Length < 2 || !Vector3.TryParse(details, out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.AutoPilotLocal((int)target.X, (int)target.Y, target.Z);
            }

            else if (command == "fixme")
            {
                Session.UpdateAppearance();
                Session.Client.Self.Movement.FinishAnim = true;
                Session.Client.Self.Movement.SendUpdate();
                Session.Client.Self.Movement.FinishAnim = false;
                Session.Client.Self.Movement.SendUpdate();
            }

            else if (command == "gc")
            {
                Display.InfoResponse(sessionNum, "Performing garbage collection...");
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            else if (command == "groups")
            {
                Display.GroupList(sessionNum);
            }

            else if (command == "roles")
            {
                UUID groupID;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out groupID))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Groups.RequestGroupRoles(groupID);
            }

            else if (command == "groupinvite")
            {
                UUID inviteeID;
                UUID groupID;
                UUID roleID;
                if (cmd.Length < 4 || !UUID.TryParse(cmd[1], out inviteeID) || !UUID.TryParse(cmd[2], out groupID) || !UUID.TryParse(cmd[3], out roleID))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Display.InfoResponse(sessionNum, "Inviting user " + inviteeID + " to group " + groupID + " with the role " + roleID + ".");
                Session.Client.Groups.AddToRole(groupID, roleID, inviteeID);
            }

            else if (command == "help")
            {
                string topic = "";
                if (cmd.Length > 1) topic = cmd[1];
                Display.Help(topic);
            }

            else if (command == "home")
            {
                Session.Client.Self.Teleport(UUID.Zero);
            }

            else if (command == "http")
            {
                if (cmd.Length < 2)
                {
                    if (!Interface.HTTPServer.Server.Listening) Display.InfoResponse(0, "HTTPServer is disabled (" + Interface.HTTPServer.Server.Clients.Count + " client(s) connected)");
                    else Display.InfoResponse(0, "HTTPServer is enabled (" + Interface.HTTPServer.Server.Clients.Count + " client(s) connected)");
                }
                else
                {
                    string flag = cmd[1].ToLower();
                    if (flag == "off")
                    {
                        Interface.HTTPServer.Server.StopListening();
                        Display.InfoResponse(0, "HTTP server disabled (" + Interface.HTTPServer.Server.Clients.Count + " client(s) remaining)");
                    }
                    else if (flag == "on")
                    {
                        int port;
                        if (cmd.Length < 3 || !int.TryParse(cmd[2], out port)) port = 8066;
                        Interface.HTTPServer.Server.Listen(port);
                        Display.InfoResponse(0, "HTTP server enabled on port " + port);
                    }
                    else
                    {
                        Display.Help(command);
                        return ScriptSystem.CommandResult.InvalidUsage;
                    }
                }
            }

            else if (command == "im")
            {
                UUID target;
                if (cmd.Length < 3 || !UUID.TryParse(cmd[1], out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }

                if (Session.IMSessions.ContainsKey(target))
                {
                    Session.Client.Self.InstantMessage(target, details, Session.IMSessions[target].IMSessionID);
                }
                else Session.Client.Self.InstantMessage(target, details);
            }


            else if (command == "busyim")
            {
                UUID target;
                if (cmd.Length < 3 || !UUID.TryParse(cmd[1], out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }

                if (Session.IMSessions.ContainsKey(target))
                {
                    Session.Client.Self.InstantMessage(Session.Client.Self.Name, target, details, Session.IMSessions[target].IMSessionID, InstantMessageDialog.BusyAutoResponse, InstantMessageOnline.Online, Vector3.Zero, UUID.Zero, new byte[0]);
                }
                else Session.Client.Self.InstantMessage(Session.Client.Self.Name, target, details, UUID.Random(), InstantMessageDialog.BusyAutoResponse, InstantMessageOnline.Online, Vector3.Zero, UUID.Zero, new byte[0]);
            }

            else if (command == "forcefriend")
            {
                UUID target;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.InstantMessage(Session.Client.Self.Name, target, "", UUID.Random(), InstantMessageDialog.FriendshipAccepted, InstantMessageOnline.Online, Vector3.Zero, UUID.Zero, new byte[0] { });
            }

            else if (command == "popup")
            {
                UUID target;
                if (cmd.Length < 3 || !UUID.TryParse(cmd[1], out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }

                if (Session.IMSessions.ContainsKey(target))
                {
                    Session.Client.Self.InstantMessage(Session.Client.Self.Name, target, details, Session.IMSessions[target].IMSessionID, InstantMessageDialog.ConsoleAndChatHistory, InstantMessageOnline.Online, Vector3.Zero, UUID.Zero, new byte[0]);
                }
                else Session.Client.Self.InstantMessage(Session.Client.Self.Name, target, details, UUID.Random(), InstantMessageDialog.ConsoleAndChatHistory, InstantMessageOnline.Online, Vector3.Zero, UUID.Zero, new byte[0]);
            }

            else if (command == "jump")
            {
                if (cmd.Length == 1 || cmd[1].ToLower() == "on")
                {
                    Session.Client.Self.Jump(true);
                }
                else if (cmd.Length > 1 && cmd[1].ToLower() == "off")
                {
                    Session.Client.Self.Jump(false);
                }
                else
                {
                    Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage;
                }
            }

            else if (command == "crouch")
            {
                if (cmd.Length == 1 || cmd[1].ToLower() == "on")
                {
                    Session.Client.Self.Crouch(true);
                }
                else if (cmd.Length > 1 && cmd[1].ToLower() == "off")
                {
                    Session.Client.Self.Crouch(false);
                }
                else
                {
                    Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage;
                }
            }

            else if (command == "land")
            {
                Session.Client.Self.Movement.Fly = false;
                Session.Client.Self.Movement.FinishAnim = true;
                Session.Client.Self.Movement.SendUpdate();
                Session.Client.Self.Movement.FinishAnim = false;
                //Session.Client.Self.Movement.SendUpdate();
            }

            else if (command == "listen")
            {
                Session.Settings.DisplayChat = true;
            }

            else if (command == "login")
            {
                if (!LoginCommand(sessionNum, cmd))
                {
                    Display.InfoResponse(sessionNum, "Invalid login parameters");
                }
            }

            else if (command == "look")
            {
                int countText = 0;

                if (cmd.Length == 1)
                {
                    Vector3 sunDirection = Session.Client.Grid.SunDirection;
                    string simName = Session.Client.Network.CurrentSim.Name;
                    string weather = Display.RPGWeather(sessionNum, simName, sunDirection);
                    if (simName != "" && sunDirection != Vector3.Zero)
                    {
                        Display.InfoResponse(sessionNum, weather);
                    }

                    lock (Session.Prims)
                    {
                        foreach (KeyValuePair<uint, Primitive> pair in Session.Prims)
                        {
                            if (pair.Value.Text != "") countText++;
                        }
                    }
                    Display.InfoResponse(sessionNum, "There are " + countText + " objects with text nearby.");
                }

                else
                {
                    lock (Session.Prims)
                    {
                        foreach (KeyValuePair<uint, Primitive> pair in Session.Prims)
                        {
                            try
                            {
                                if (Regex.IsMatch(pair.Value.Text, details, RegexOptions.IgnoreCase))
                                {
                                    //FIXME - move to Display
                                    Console.WriteLine(pair.Value.LocalID + " " + pair.Value.ID + " " + pair.Value.Text);
                                    countText++;
                                }
                            }
                            catch
                            {
                                Display.Error(sessionNum, "/look: invalid regular expression");
                                return ScriptSystem.CommandResult.InvalidUsage;
                            }
                        }
                    }
                    Display.InfoResponse(sessionNum, "There are " + countText + " objects matching your query.");
                }

            }

            else if (command == "lure")
            {
                UUID target;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                string reason;
                if (cmd.Length > 2) reason = details;
                else reason = "Join me in " + Session.Client.Network.CurrentSim.Name + "!";
                Session.Client.Self.SendTeleportLure(target, reason);
            }

            else if (command == "mlook")
            {
                if (cmd.Length < 2 || cmd[1].ToLower() == "on")
                {
                    Session.Client.Self.Movement.Mouselook = true;
                    Display.InfoResponse(sessionNum, "Mouselook enabled");
                }
                else if (cmd[1].ToLower() == "off")
                {
                    Session.Client.Self.Movement.Mouselook = false;
                    Display.InfoResponse(sessionNum, "Mouselook disabled");
                }
                else
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
            }

            //TODO: reimplement
            /*
            else if (command == "shoot")
            {
                Vector3 target;
                if (cmd.Length == 1) Session.Client.Self.Movement.Shoot();
                else if (Vector3.TryParse(cmd[1], out target))
                {
                    Session.Client.Self.Movement.Shoot(target);
                }
            }
            */

            else if (command == "particles")
            {
                lock (Session.Prims)
                {
                    foreach (KeyValuePair<uint, Primitive> obj in Session.Prims)
                    {
                        if (obj.Value.ParticleSys.Pattern != Primitive.ParticleSystem.SourcePattern.None)
                        {
                            Console.WriteLine(obj.Value.ID.ToString() + " "
                                + Display.VectorString(obj.Value.Position) + " "
                                + obj.Value.ParentID
                            );
                        }
                    }
                }
            }

            else if (command == "pay")
            {
                UUID id;
                int amount;
                if (cmd.Length < 3 || !int.TryParse(cmd[1], out amount) || !UUID.TryParse(cmd[2], out id))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.GiveMoney(id, amount, "", MoneyTransactionType.Gift, TransactionFlags.None);
            }

            else if (command == "payme")
            {
                int amount;
                if (cmd.Length < 2 || !int.TryParse(cmd[1], out amount))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                else if (Session.Settings.MasterID == UUID.Zero)
                {
                    Display.Error(sessionNum, "MasterID not defined");
                    return ScriptSystem.CommandResult.UnexpectedError;
                }
                else
                {
                    Session.Client.Self.GiveMoney(Session.Settings.MasterID, amount, "Payment to master", MoneyTransactionType.Gift, TransactionFlags.None);
                }
            }

            else if (command == "quiet")
            {
                if (cmd.Length == 1 || cmd[1].ToLower() == "on")
                {
                    Session.Settings.DisplayChat = false;
                    Display.InfoResponse(sessionNum, "Hiding chat from objects/avatars.");
                }
                else if (cmd[1].ToLower() == "off")
                {
                    Session.Settings.DisplayChat = true;
                    Display.InfoResponse(sessionNum, "Showing chat from objects/avatars.");
                }
                else
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
            }

            else if (command == "quit")
            {
                Display.InfoResponse(sessionNum, "Disconnecting " + Session.Name + "...");
                Session.Client.Network.Logout();
            }

            else if (command == "re")
            {
                if (cmd.Length == 1)
                {
                    if (Session.IMSessions.Count == 0) Display.InfoResponse(sessionNum, "No active IM sessions");
                    else Display.IMSessions(sessionNum);
                }
                else if (cmd.Length < 3)
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                else
                {
                    UUID match = UUID.Zero;
                    foreach (KeyValuePair<UUID, GhettoSL.IMSession> pair in Session.IMSessions)
                    {
                        if (pair.Value.Name.Length < cmd[1].Length) continue; //too short to be a match
                        else if (pair.Value.Name.Substring(0, cmd[1].Length).ToLower() == cmd[1].ToLower())
                        {
                            if (match != UUID.Zero)
                            {
                                Display.Error(sessionNum, "\"" + cmd[1] + "\" could refer to more than one active IM session.");
                                return ScriptSystem.CommandResult.InvalidUsage;
                            }
                            match = pair.Key;
                        }
                    }
                    if (match != UUID.Zero)
                    {
                        Display.SendMessage(sessionNum, 0, match, details);
                        Session.Client.Self.InstantMessage(match, details, Session.IMSessions[match].IMSessionID);
                    }
                }
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

            else if (command == "reseturi")
            {
                Session.Settings.StartLocation = "last";
            }

            else if (command == "rez")
            {
                Console.WriteLine("FIXME");
            }

            else if (command == "addprim")
            {
                Vector3 scale;
                if (Vector3.TryParse(details, out scale))
                {
                    ObjectAddPacket add = new ObjectAddPacket();
                    add.AgentData = new ObjectAddPacket.AgentDataBlock();
                    add.AgentData.AgentID = Session.Client.Self.AgentID;
                    add.AgentData.GroupID = Session.Client.Self.ActiveGroup;
                    add.AgentData.SessionID = Session.Client.Self.SessionID;
                    add.ObjectData = new ObjectAddPacket.ObjectDataBlock();
                    add.ObjectData.AddFlags = 2;
                    add.ObjectData.BypassRaycast = 1;
                    add.ObjectData.PathBegin = 0;
                    add.ObjectData.PathCurve = 16;
                    add.ObjectData.PathEnd = 0;
                    add.ObjectData.PathRadiusOffset = 0;
                    add.ObjectData.PathRevolutions = 0;
                    add.ObjectData.PathScaleX = 100;
                    add.ObjectData.PathScaleY = 100;
                    add.ObjectData.PathShearX = 0;
                    add.ObjectData.PathShearY = 0;
                    add.ObjectData.PathSkew = 0;
                    add.ObjectData.PathTaperX = 0;
                    add.ObjectData.PathTaperY = 0;
                    add.ObjectData.PathTwist = 0;
                    add.ObjectData.PathTwistBegin = 0;
                    add.ObjectData.PCode = 9;
                    add.ObjectData.ProfileBegin = 0;
                    add.ObjectData.ProfileCurve = 1;
                    add.ObjectData.ProfileEnd = 0;
                    add.ObjectData.ProfileHollow = 0;
                    add.ObjectData.RayEnd = Session.Client.Self.SimPosition;
                    add.ObjectData.RayEndIsIntersection = 0;
                    add.ObjectData.RayStart = Session.Client.Self.SimPosition;
                    add.ObjectData.RayTargetID = UUID.Zero;
                    add.ObjectData.Rotation = Quaternion.Identity;
                    add.ObjectData.Scale = scale;
                    add.ObjectData.State = 0;

                    Session.Client.Network.SendPacket(add);

                    //LLObject.ObjectData prim = new LLObject.ObjectData();
                    //prim.PCode = PCode.Prim;
                    
                    //prim.ProfileCurve = (LLObject.ProfileCurve)1;


                    //Session.Client.Objects.AddPrim(Session.Client.Network.CurrentSim, prim, UUID.Zero, Session.Client.Self.SimPosition, scale, LLQuaternion.Identity);
                }
            }

            else if (command == "ride")
            {
                Session.RideWith(details);
            }

            else if (command == "rotto")
            {
                Vector3 target;
                if (cmd.Length < 2 || !Vector3.TryParse(details, out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.Movement.TurnToward(target);
            }

            else if (command == "brot")
            {
                Vector3 target;
                if (cmd.Length < 2 || !Vector3.TryParse(details, out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.Movement.TurnToward(target);
            }
            /*
            else if (command == "hrot")
            {
                Vector3 target;
                if (cmd.Length < 2 || !Vector3.TryParse(details, out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.Movement.HeadRotation = Vector3.Axis2Rot(target);
                Session.Client.Self.Movement.SendUpdate();
            }
            */
            else if (command == "cam")
            {
                Vector3 target;
                if (cmd.Length < 3 || !Vector3.TryParse(details, out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                string arg = cmd[1].ToLower();
                if (arg != "center" && arg != "left" && arg != "up" && arg != "at")
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }

                char[] space = { ' ' };
                string vString = details.Replace("<", "").Replace(">", "").Replace(",", " ");
                string[] v = vString.Split(space, StringSplitOptions.RemoveEmptyEntries);

                if (arg == "center") Session.Client.Self.Movement.Camera.Position = target;
                else if (arg == "at") Session.Client.Self.Movement.Camera.AtAxis = target;
                else if (arg == "left") Session.Client.Self.Movement.Camera.LeftAxis = target;
                else if (arg == "up") Session.Client.Self.Movement.Camera.UpAxis = target;

                Session.Client.Self.Movement.SendUpdate();
                Display.InfoResponse(sessionNum, "Camera settings updated");
            }

            else if (command == "run" || command == "running")
            {
                if (cmd.Length == 1 || cmd[1].ToLower() == "on")
                {
                    Session.Client.Self.Movement.AlwaysRun = true;
                    Display.InfoResponse(sessionNum, "Running enabled.");
                }
                else if (cmd[1].ToLower() == "off")
                {
                    Session.Client.Self.Movement.AlwaysRun = false;
                    Display.InfoResponse(sessionNum, "Running disabled.");
                }
                else
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
            }

            else if (command == "say")
            {
                Session.Client.Self.Chat(details, 0, ChatType.Normal);
            }

            else if (command == "script")
            {
                if (!LoadScriptCommand(sessionNum, cmd)) return ScriptSystem.CommandResult.UnexpectedError;
            }

            else if (command == "scripts")
            {
                Display.ScriptList();
            }

            else if (command == "s" || command == "session")
            {
                if (cmd.Length > 1)
                {
                    if (cmd[1] == "-a" || cmd[1] == "*")
                    {
                        foreach (KeyValuePair<uint, GhettoSL.UserSession> pair in Interface.Sessions)
                        {
                            Command(pair.Key, scriptName, details, parseVariables, fromMasterIM);
                        }
                        return ScriptSystem.CommandResult.NoError;
                    }
                    uint switchTo;
                    if (!uint.TryParse(cmd[1], out switchTo) || switchTo < 1 || !Interface.Sessions.ContainsKey(switchTo))
                    {
                        Display.Error(sessionNum, "Invalid session number");
                        return ScriptSystem.CommandResult.InvalidUsage;
                    }
                    else if (cmd.Length == 2)
                    {
                        Interface.CurrentSession = switchTo;
                        string name = Interface.Sessions[switchTo].Name;
                        Display.InfoResponse(switchTo, "Switched to " + name);
                        Console.Title = name + " @ " + Session.Client.Network.CurrentSim.Name + " - GhettoSL";
                    }
                    else
                    {
                        Command(switchTo, scriptName, details, parseVariables, fromMasterIM);
                    }
                }
                else Display.SessionList();
            }

            else if (command == "whois")
            {
                //FIXME - add group, land, etc searching
                if (cmd.Length < 2)
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                UUID targetID;
                bool isID = UUID.TryParse(cmd[1], out targetID);
                if (isID) Session.Client.Avatars.RequestAvatarName(targetID);
                else Session.Client.Directory.StartPeopleSearch(DirectoryManager.DirFindFlags.People, details, 0);

            }

            else if (command == "inc" && scriptName != "")
            {
                int amount = 1;
                if (unparsedCommand.Length < 2 || (unparsedCommand.Length > 2 && !int.TryParse(Variables(sessionNum, unparsedCommand[2], scriptName), out amount)))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                ScriptSystem.UserScript Script = Interface.Scripts[scriptName];
                int value = 0;
                string variableName = unparsedCommand[1];
                if (Script.Variables.ContainsKey(variableName) && !int.TryParse(Script.Variables[variableName], out value)) return ScriptSystem.CommandResult.InvalidUsage;
                //FIXME - change in the following code, int + "" to a proper string-to-int conversion
                else if (Script.Variables.ContainsKey(variableName)) Script.Variables[variableName] = "" + (value + amount);
                else Script.Variables.Add(variableName, "" + amount);
                //QUESTION - Right now, inc creates a new %var if the specified one doesn't exist. Should it?
            }

            else if (command == "set" && scriptName != "")
            {
                if (unparsedCommand.Length < 3 || unparsedCommand[1].Substring(0, 1) != "%")
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                details = "";
                for (int d = 2; d < unparsedCommand.Length; d++)
                {
                    if (details != "") details += " ";
                    details += unparsedCommand[d];
                }
                string variableName = unparsedCommand[1];
                ScriptSystem.UserScript Script = Interface.Scripts[scriptName];
                if (Script.Variables.ContainsKey(variableName)) Script.Variables[variableName] = Variables(sessionNum, details, scriptName);
                else Script.Variables.Add(variableName, Variables(sessionNum, details, scriptName));

            }

            else if (command == "sethome")
            {
                Session.Client.Self.SetHome();
            }

            else if (command == "shout")
            {
                Session.Client.Self.Chat(details, 0, ChatType.Shout);
            }

            else if (command == "simmessage")
            {
                Session.Client.Network.CurrentSim.Estate.SimulatorMessage(details);
            }

            else if (command == "sit")
            {
                UUID target;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out target))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.Movement.SitOnGround = false;
                Session.Client.Self.Movement.StandUp = false;
                Session.Client.Self.RequestSit(target, Vector3.Zero);
                Session.Client.Self.Sit();
                Session.Client.Self.Movement.SendUpdate();
            }

            else if (command == "sitg")
            {
                Session.Client.Self.SitOnGround();
            }

            /*
            else if (command == "sleep")
            {
                float interval;
                if (cmd.Length < 2 || scriptName == "" || !float.TryParse(cmd[1], out interval))
                {
                    Display.Help(command);
                    return false;
                }
                Interface.Scripts[scriptName].Sleep(interval);
                return false; //pause script
            }
            */

            else if (command == "spoofim")
            {
                UUID target;
                if (cmd.Length < 4 || !UUID.TryParse(cmd[1], out target))
                {
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                string fromName = cmd[2] + " " + cmd[3];
                Session.Client.Self.InstantMessage(fromName, target, cmd[4], UUID.Random(), new UUID[0]);
            }

            else if (command == "stand")
            {
                Session.Client.Self.Movement.SitOnGround = false;
                Session.Client.Self.Movement.StandUp = true;
                Session.Client.Self.Movement.SendUpdate();
                Session.Client.Self.Movement.StandUp = false;
            }

            else if (command == "stats")
            {
                Display.Stats(sessionNum);
            }

            else if (command == "stopanim")
            {
                UUID anim;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out anim))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.AnimationStop(anim, false);
            }

            else if (command == "teleport" || command == "tp")
            {
                if (!Session.Client.Network.Connected) { Display.Error(sessionNum, "Not connected"); return ScriptSystem.CommandResult.UnexpectedError; }

                if (cmd.Length < 2)
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }

                //assumed values
                string simName = ScriptSystem.QuoteArg(cmd, 1);
                Vector3 target = new Vector3(128, 128, 0);

                //assumed wrong
                if (cmd.Length > 2)
                {
                    //find what pos the vector starts at and how many tokens it is
                    int start = simName.Split(splitChar).Length + 1;
                    int len = cmd.Length - start;
                    if (len > 0)
                    {
                        string[] v = new string[len];
                        Array.Copy(cmd, start, v, 0, len);
                        if (!Vector3.TryParse(String.Join(" ", v), out target))
                        {
                            Display.Help(command);
                            return ScriptSystem.CommandResult.InvalidUsage;
                        }
                    }
                }

                Display.Teleporting(sessionNum, simName);
                Session.Client.Self.Teleport(simName, target);
            }

            //FIXME - move to /teleport and just check for ulong
            else if (command == "teleporth")
            {
                ulong handle;
                if (cmd.Length < 2 || !ulong.TryParse(cmd[1], out handle))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                Session.Client.Self.Teleport(handle, new Vector3(128, 128, 0));
            }

            else if (command == "timer")
            {
                if (cmd.Length < 3)
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                else if (cmd[2] == "off")
                {
                    if (Session.Timers.ContainsKey(cmd[1]))
                    {
                        Session.Timers[cmd[1]].Dispose();
                        Session.Timers.Remove(cmd[1]);
                    }
                }
                else
                {
                    string flags = "";
                    int start = 1;
                    if (cmd[1].Substring(0, 1) == "-")
                    {
                        flags = cmd[1];
                        start++;
                    }
                    string name = cmd[start];
                    int repeats;
                    int interval;
                    bool milliseconds = false;
                    foreach (char f in flags.ToCharArray())
                    {
                        if (f == 'm') milliseconds = true;
                    }
                    if (!int.TryParse(cmd[start + 1], out repeats) || !int.TryParse(cmd[start + 2], out interval))
                    {
                        Display.Help(command);
                        return ScriptSystem.CommandResult.InvalidUsage;
                    }
                    string m;
                    if (milliseconds) m = "ms";
                    else m = "sec";
                    Display.InfoResponse(sessionNum, "Timer \"" + name + "\" activated (Repeat: " + repeats + ", Interval: " + interval + m + ")");
                    ScriptSystem.UserTimer timer = new ScriptSystem.UserTimer(sessionNum, name, interval, milliseconds, repeats, details);
                    if (!Session.Timers.ContainsKey(name)) Session.Timers.Add(name, timer);
                    else Session.Timers[name] = timer;
                }
            }

            else if (command == "timers")
            {
                string arg = "";
                if (cmd.Length > 1) arg = cmd[1].ToLower();
                if (Session.Timers.Count == 0)
                {
                    Display.InfoResponse(sessionNum, "No active timers for this session");
                }
                else
                {
                    foreach (KeyValuePair<string, ScriptSystem.UserTimer> pair in Session.Timers)
                    {
                        //FIXME - move to Display
                        if (arg == "off") pair.Value.Dispose();
                        else Display.InfoResponse(sessionNum, Display.Pad(pair.Key, 15) + " " + Display.Pad(pair.Value.RepeatsRemaining.ToString(), 3) + " " + pair.Value.Command);
                    }
                }
            }

            else if (command == "topscripts")
            {
                Session.Client.Network.CurrentSim.Estate.GetTopScripts();
            }

            else if (command == "topcolliders")
            {
                Session.Client.Network.CurrentSim.Estate.GetTopColliders();
            }

            else if (command == "touch")
            {
                UUID findID;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out findID))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }
                lock (Session.Prims)
                {
                    foreach (Primitive prim in Session.Prims.Values)
                    {
                        if (prim.ID != findID) continue;
                        Session.Client.Self.Touch(prim.LocalID);
                        Display.InfoResponse(sessionNum, "You touch an object...");
                        return ScriptSystem.CommandResult.NoError;
                    }
                }
                Display.Error(sessionNum, "Object not found");
            }

            else if (command == "touchid")
            {
                if (cmd.Length < 2) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                uint touchid;
                if (uint.TryParse(cmd[1], out touchid)) Session.Client.Self.Touch(touchid);
            }

            else if (command == "updates")
            {
                if (cmd.Length < 2) { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
                string toggle = cmd[1].ToLower();
                if (toggle == "on")
                {
                    Session.Client.Settings.SEND_AGENT_UPDATES = true;
                    Display.InfoResponse(sessionNum, "Update timer ON");
                }
                else if (toggle == "off")
                {
                    Session.Client.Settings.SEND_AGENT_UPDATES = false;
                    Display.InfoResponse(sessionNum, "Update timer OFF");
                }
                else { Display.Help(command); return ScriptSystem.CommandResult.InvalidUsage; }
            }

            else if (command == "walk")
            {
                Session.Client.Self.Movement.AlwaysRun = false;
            }

            else if (command == "wear")
            {
                UUID itemid;
                if (cmd.Length < 2 || !UUID.TryParse(cmd[1], out itemid))
                {
                    Display.Help(command);
                    return ScriptSystem.CommandResult.InvalidUsage;
                }

                if (!Session.Client.Inventory.Store.Contains(itemid))
                {
                    Display.Error(sessionNum, "Asset id not found in inventory cache");
                    return ScriptSystem.CommandResult.UnexpectedError;
                }

                InventoryItem item = (InventoryItem)Session.Client.Inventory.Store[itemid];
                AttachmentPoint point = AttachmentPoint.Default;
                if (cmd.Length > 2)
                {
                    string p = cmd[2].ToLower();
                    if (p == "skull") point = AttachmentPoint.Skull;
                    else if (p == "chest") point = AttachmentPoint.Chest;
                    else if (p == "lhand") point = AttachmentPoint.LeftHand;
                    else if (p == "lhip") point = AttachmentPoint.LeftHip;
                    else if (p == "llleg") point = AttachmentPoint.LeftLowerLeg;
                    else if (p == "mouth") point = AttachmentPoint.Mouth;
                    else if (p == "nose") point = AttachmentPoint.Nose;
                    else if (p == "pelvis") point = AttachmentPoint.Pelvis;
                    else if (p == "rhand") point = AttachmentPoint.RightHand;
                    else if (p == "rhip") point = AttachmentPoint.RightHip;
                    else if (p == "rlleg") point = AttachmentPoint.RightLowerLeg;
                    else if (p == "spine") point = AttachmentPoint.Spine;
                    else if (p == "hudtop") point = AttachmentPoint.HUDTop;
                    else if (p == "hudbottom") point = AttachmentPoint.HUDBottom;
                    //FIXME - support all other points
                    else
                    {
                        Display.InfoResponse(sessionNum, "Unknown attachment point \"" + p + "\" - using object default");
                    }
                }

                Session.Client.Appearance.Attach(item, point);
                Display.InfoResponse(sessionNum, "Attached: " + item.Name + " (" + item.Description + ")");
            }

            else if (command == "whisper")
            {
                Session.Client.Self.Chat(details, 0, ChatType.Whisper);
            }

            else if (command == "who")
            {
                Display.Who(sessionNum);
            }

            else if (command == "whois")
            {
                //FIXME - add whois

            }

            else
            {
                Display.InfoResponse(sessionNum, "Unknown command: " + command.ToUpper());
                return ScriptSystem.CommandResult.InvalidUsage;
            }

            return ScriptSystem.CommandResult.NoError;
        }


    }
}
