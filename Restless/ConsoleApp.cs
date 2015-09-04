using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nulands.Restless
{
    public class ConsoleCommand
    {
        public Func<IList<string>, bool> Condition { get; set; }
        public Func<IList<string>, Task> Run { get; set; }
        public string[] CommandArgNames { get; set; }

        public string CommandArgNamesToString(string cmd = "")
        {
            if (CommandArgNames == null)
                return cmd;
            return CommandArgNames.Aggregate(cmd, (acc, curr) => acc += " <" + curr + ">");
        }
    }

    public static class Tasks
    {
        public static readonly Task<int> TaskOk = Task.FromResult(42);

        public static Task<int> Ok(Action action)
        {
            action();
            return TaskOk;
        }

    }

    public class ConsoleApp
    {
        public Action<string> Write { get; set; }
        public Action<string> WriteLine { get; set; }
        public Action<string> WriteLineGreen { get; set; }
        public Action<string> WriteLineRed { get; set; }

        public Func<string> ReadLine { get; set; }

        public static Task<int> Ok(Action action)
        {
            action();
            return Tasks.TaskOk;
        }

        public string AppName { get; set; }

        public string Header { get; set; }

        public string Subheader { get; set; }

        public Dictionary<string, ConsoleCommand> Commands { get; private set; }

        public Dictionary<string, string> Help { get; private set; }

        public Action OnExit { get; set; }

        public ConsoleApp()
        {
            Commands = new Dictionary<string, ConsoleCommand>();
            Help = new Dictionary<string, string>();
            this["help"] = _ => Tasks.Ok(PrintHelp);
        }

        public bool HandleCommand(string command)
        {
            int cmdEndIndex = command.IndexOf(' ');
            if (cmdEndIndex == -1)
                cmdEndIndex = command.Length;

            var cmdKey = command.Substring(0, cmdEndIndex);
            ConsoleCommand cmdHandler;

            if (Commands.TryGetValue(cmdKey, out cmdHandler))
            {
                string cmdArgsStr = GetCommandArgString(command, cmdEndIndex);
                List<string> cmdArgs = GetCommandArgParts(cmdArgsStr);

                if (cmdArgs.Count == 1 && cmdArgs[0] == "help")  // Print help for command
                {
                    PrintHelpForCommand(cmdKey);
                }
                else
                {
                    RunCommandHandlerIfPossible(cmdHandler, cmdArgs);
                }
            }

            return cmdHandler != null;
        }

        public void PrintHelp()
        {
            PrintHeadersIfNotNullOrEmpty();

            WriteLine("    ------------------------------------\n");
            foreach (var cmd in Commands)
            {
                PrintCmdName(cmd);
                WriteLine("");
            }
            WriteLine("    exit");
            WriteLine("\n    ------------------------------------\n");
            WriteLine("");
        }

        public void Run(bool initialyPrintHelp = true)
        {
            if (initialyPrintHelp)
                PrintHelp();

            string command = "";
            while (true)
            {
                Write("\n    " + AppName + " # ");

                command = ReadLine();

                if (command == "exit")
                    break;

                if (!HandleCommand(command))
                    WriteLine("\n    Sorry, can´t understand your command. \n    Please try again or enter Help for more information.\n");
            }
            ExitAndKillSessionIfOpen();
        }

        public Func<IList<string>, Task> this[string cmd, params string[] cmdArgNames]
        {
            set
            {
                Commands[cmd] = new ConsoleCommand()
                {
                    Condition = ca =>
                    {
                        bool result = ca.Count == cmdArgNames.Length;
                        if (!result)
                        {
                            WriteLineRed("Invalid arguments");
                            WriteLine("    Use   " + cmdArgNames.Aggregate(cmd, (acc, curr) => acc += " <" + curr + ">"));
                        }
                        return result;
                    },
                    Run = value,
                    CommandArgNames = cmdArgNames
                };
            }
        }

        public Func<IList<string>, Task> this[string cmd, Func<IList<string>, bool> condition = null]
        {
            set
            {
                Commands[cmd] = new ConsoleCommand() { Condition = condition, Run = value };
            }
        }

        static string GetCommandArgString(string command, int cmdEndIndex)
        {
            return cmdEndIndex == command.Length
                ? ""
                : command.Substring(cmdEndIndex + 1);
        }

        static List<string> GetCommandArgParts(string cmdArgsStr)
        {
            return String.IsNullOrEmpty(cmdArgsStr)
                                ? new List<string>()
                                : cmdArgsStr.Split(' ').ToList();
        }

        static void RunCommandHandlerIfPossible(ConsoleCommand cmdHandler, List<string> cmdArgs)
        {
            bool canRun = true;

            if (cmdHandler.Condition != null)
                canRun = cmdHandler.Condition(cmdArgs);

            if (canRun)
                cmdHandler.Run(cmdArgs);
        }

        void PrintHelpForCommand(string cmdKey)
        {
            string helpText = "";
            if (Help.TryGetValue(cmdKey, out helpText))
                WriteLine("    " + helpText);
            else
                WriteLine("No help text found sorry");
        }


        void PrintHeadersIfNotNullOrEmpty()
        {
            if (!String.IsNullOrEmpty(Header)) WriteLine("    " + Header);
            if (!String.IsNullOrEmpty(Subheader)) WriteLine("    " + Subheader);
        }

        void PrintCmdName(KeyValuePair<string, ConsoleCommand> cmd)
        {
            WriteLine("    " + cmd.Value.CommandArgNamesToString(cmd.Key));
        }

        void PrintHelpText(KeyValuePair<string, ConsoleCommand> cmd)
        {
            string helpText = "";
            Help.TryGetValue(cmd.Key, out helpText);
            WriteLine("    " + helpText);
        }

        void PrintCmdArgNamesIfNotNullOrEmpty(KeyValuePair<string, ConsoleCommand> cmd)
        {
            string cmdArgNamesStr = cmd.Value.CommandArgNamesToString();
            if (!String.IsNullOrEmpty(cmdArgNamesStr))
                Write(" " + cmdArgNamesStr);
        }

        void ExitAndKillSessionIfOpen()
        {

            WriteLine("Shutting down...");
            // On Exit
            if (OnExit != null)
                OnExit();

            WriteLine("Shutdown complete");
            WriteLine("Press any key to exit when shutdown is complete...");
            ReadLine();
        }
    }
}
