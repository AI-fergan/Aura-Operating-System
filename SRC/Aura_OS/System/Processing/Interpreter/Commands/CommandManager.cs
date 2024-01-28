﻿/*
* PROJECT:          Aura Operating System Development
* CONTENT:          Command Interpreter - CommandManager
* PROGRAMMER(S):    John Welsh <djlw78@gmail.com>
*                   Valentin Charbonnier <valentinbreiz@gmail.com>
*/

using Aura_OS.System.Utils;
using Cosmos.System.Network;
using System;
using System.Collections.Generic;
using System.IO;
using Aura_OS.System.Processing.Interpreter.Commands.Util;
using Aura_OS.System.Processing.Interpreter.Commands.Filesystem;
using Aura_OS.System.Processing.Interpreter.Commands.Power;
using Aura_OS.System.Processing.Interpreter.Commands.c_Console;
using Aura_OS.System.Processing.Interpreter.Commands.Network;
using Aura_OS.System.Processing.Interpreter.Commands.SystemInfomation;
using Aura_OS.System.Processing.Interpreter.Commands.Graphics;
using Aura_OS.System.Processing.Interpreter.Commands.Processing;

namespace Aura_OS.System.Processing.Interpreter.Commands
{
    public class CommandManager
    {
        public static List<ICommand> Commands = new List<ICommand>();

        public CommandManager()
        {
            RegisterAllCommands();
        }

        public void RegisterAllCommands()
        {
            Commands.Add(new CommandReboot(new string[] { "reboot", "rb" }));
            Commands.Add(new CommandShutdown(new string[] { "shutdown", "sd" }));

            Commands.Add(new CommandClear(new string[] { "clear", "clr" }));
            Commands.Add(new CommandKeyboardMap(new string[] { "setkeyboardmap", "setkeyboard" }));
            Commands.Add(new CommandEnv(new string[] { "export", "set" }));
            Commands.Add(new CommandEcho(new string[] { "echo" }));

            Commands.Add(new CommandIPConfig(new string[] { "ipconfig", "ifconfig", "netconf" }));
            Commands.Add(new CommandPing(new string[] { "ping" }));
            Commands.Add(new CommandUdp(new string[] { "udp" }));
            Commands.Add(new CommandDns(new string[] { "dns" }));
            Commands.Add(new CommandWget(new string[] { "wget" }));
            Commands.Add(new CommandFtp(new string[] { "ftp" }));
            Commands.Add(new CommandPackage(new string[] { "package", "pkg" }));

            Commands.Add(new CommandVersion(new string[] { "version", "ver", "about" }));
            Commands.Add(new CommandSystemInfo(new string[] { "systeminfo", "sysinfo" }));
            Commands.Add(new CommandTime(new string[] { "time", "date" }));
            Commands.Add(new CommandHelp(new string[] { "help" }));

            Commands.Add(new CommandChangeRes(new string[] { "changeres", "cr" }));
            Commands.Add(new CommandLspci(new string[] { "lspci" }));
            //CMDs.Add(new CommandCrash(new string[] { "crash" }));

            Commands.Add(new CommandLsprocess(new string[] { "lsprocess" }));

            Commands.Add(new CommandVol(new string[] { "vol" }));
            Commands.Add(new CommandDir(new string[] { "dir", "ls", "l" }));
            Commands.Add(new CommandMkdir(new string[] { "mkdir", "md" }));
            Commands.Add(new CommandCat(new string[] { "cat" }));
            Commands.Add(new CommandCD(new string[] { "cd" }));
            Commands.Add(new CommandMkfil(new string[] { "touch", "mkfil", "mf" }));
            Commands.Add(new CommandRm(new string[] { "rm", "rmf", "rmd" }));
            Commands.Add(new CommandHex(new string[] { "hex" }));
            Commands.Add(new CommandTree(new string[] { "tree" }));
            Commands.Add(new CommandRun(new string[] { "run" }));
            Commands.Add(new CommandCopy(new string[] { "cp" }));
            Commands.Add(new CommandPicture(new string[] { "pic" }));

            Commands.Add(new CommandZip(new string[] { "zip" }));

            /*
            CMDs.Add(new CommandPCName(new string[] { "pcn" }));

            CMDs.Add(new CommandMIV(new string[] { "miv", "edit" }));*/

            Commands.Add(new CommandAction(new string[] { "beep" }, () =>
            {
                Cosmos.System.PCSpeaker.Beep();
            }));
            Commands.Add(new CommandAction(new string[] { "crash" }, () =>
            {
                throw new Exception("Exception test");
            }));
            Commands.Add(new CommandAction(new string[] { "crashn" }, () =>
            {
                string[] test =
                {
                    "test1",
                    "tert2"
                };
                test[2] = "test3"; //Should make a Null reference exception
            }));
        }

        /// <summary>
        /// Shell Interpreter
        /// </summary>
        /// <param name="cmd">Command</param>
        public void Execute(string cmd)
        {
            //CommandsHistory.Add(cmd); //adding last command to the commands history

            if (cmd.Length <= 0)
            {
                Console.WriteLine();
                return;
            }

            #region Parse command

            string[] parts = cmd.Split(new char[] { '>' }, 2);
            string redirectionPart = parts.Length > 1 ? parts[1].Trim() : null;
            cmd = parts[0].Trim();

            if (!string.IsNullOrEmpty(redirectionPart))
            {
                Kernel.Redirect = true;
                Kernel.CommandOutput = "";
            }

            List<string> arguments = Misc.ParseCommandLine(cmd);

            string firstarg = arguments[0]; //command name

            if (arguments.Count > 0)
            {
                arguments.RemoveAt(0); //get only arguments
            }

            #endregion

            foreach (var command in Commands)
            {
                if (command.ContainsCommand(firstarg))
                {
                    ReturnInfo result;

                    if (arguments.Count > 0 && (arguments[0] == "/help" || arguments[0] == "/h"))
                    {
                        ShowHelp(command);
                        result = new ReturnInfo(command, ReturnCode.OK);
                    }
                    else
                    {
                        result = CheckCommand(command);

                        if (result.Code == ReturnCode.OK)
                        {
                            if (arguments.Count == 0)
                            {
                                result = command.Execute();
                            }
                            else
                            {
                                result = command.Execute(arguments);
                            }
                        }
                    }

                    ProcessCommandResult(result);

                    if (Kernel.Redirect)
                    {
                        Kernel.Redirect = false;

                        Console.WriteLine();

                        HandleRedirection(redirectionPart, Kernel.CommandOutput);

                        Kernel.CommandOutput = "";
                    }

                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Unknown command.");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine();

            if (Kernel.Redirect)
            {
                Kernel.Redirect = false;

                HandleRedirection(redirectionPart, Kernel.CommandOutput);

                Kernel.CommandOutput = "";
            }
        }

        /// <summary>
        /// Show command description
        /// </summary>
        /// <param name="command">Command</param>
        private void ShowHelp(ICommand command)
        {
            Console.WriteLine("Description: " + command.Description + ".");
            Console.WriteLine();
            if (command.CommandValues.Length > 1)
            {
                Console.Write("Aliases: ");
                for (int i = 0; i < command.CommandValues.Length; i++)
                {
                    if (i != command.CommandValues.Length - 1)
                    {
                        Console.Write(command.CommandValues[i] + ", ");
                    }
                    else
                    {
                        Console.Write(command.CommandValues[i]);
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }
            command.PrintHelp();
        }

        /// <summary>
        /// Check command availability to avoid unwanted behavior.
        /// </summary>
        /// <param name="command">Command</param>
        private ReturnInfo CheckCommand(ICommand command)
        {
            if (command.Type == CommandType.Filesystem)
            {
                if (Kernel.VirtualFileSystem == null || Kernel.VirtualFileSystem.GetVolumes().Count == 0)
                {
                    return new ReturnInfo(command, ReturnCode.ERROR, "No volume detected!");
                }
            }
            if (command.Type == CommandType.Network)
            {
                if (NetworkStack.ConfigEmpty())
                {
                    return new ReturnInfo(command, ReturnCode.ERROR, "No network configuration detected! Use ipconfig /set.");
                }
            }
            return new ReturnInfo(command, ReturnCode.OK);
        }

        /// <summary>
        /// Process result info of the command
        /// </summary>
        /// <param name="result">Result information</param>
        private void ProcessCommandResult(ReturnInfo result)
        {
            if (result.Code == ReturnCode.ERROR_ARG)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Command arguments are incorrectly formatted.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else if (result.Code == ReturnCode.ERROR)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Error: " + result.Info);
                Console.ForegroundColor = ConsoleColor.White;
            }

            Console.WriteLine();
        }

        private void HandleRedirection(string filePath, string commandOutput)
        {
            string fullPath = Kernel.CurrentDirectory + filePath;

            File.WriteAllText(fullPath, commandOutput);
        }
    }
}
