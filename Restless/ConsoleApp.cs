using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Nulands.Restless
{

    public class AppInfo
    {
        public string Name { get; set; }
        public string Header { get; set; }
        public string Subheader { get; set; }
    }

    public class ConsoleCommand
    {
        public Func<IList<string>, bool> Condition { get; set; }
        public Func<IList<string>, Task> Run { get; set; }
        public string[] CommandArgNames { get; set; }
        public bool StartProgressBar { get; set; }

        public ConsoleCommand()
        {
            StartProgressBar = true;
        }

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
        public Action<string> WriteLn { get; set; }
        public Action<string> WriteLnGreen { get; set; }
        public Action<string> WriteLnRed { get; set; }
        public Action<string> WriteLnYellow { get; set; }
        public Action<string> WriteLnDarkRed { get; set; }
        public Action<string> WriteLnBlue { get; set; }

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
            ConsoleAppInit();
        }
        public ConsoleApp(string name, string header, string subHeader = "")
        {
            AppName = name;
            Header = header;
            Subheader = subHeader;
            ConsoleAppInit();
        }

        public ConsoleApp(AppInfo info)
        {
            AppName = info.Name;
            Header = info.Header;
            Subheader = info.Subheader;
            ConsoleAppInit();
        }
        void ConsoleAppInit()
        {
            Commands = new Dictionary<string, ConsoleCommand>();
            Help = new Dictionary<string, string>();
            this["help", startProgressBar: false] = _ => Tasks.Ok(PrintHelp);lkjasfd
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
            foreach (var cmd in Commands)
                PrintCmdName(cmd);
            WriteLnGreen("    exit");
            WriteLnGreen("");
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
                    WriteLn("\n    Can´t understand your command. \n    Try again or enter \"help\" for more information.\n");
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
                            WriteLnRed("Invalid arguments");
                            WriteLn("    Use   " + cmdArgNames.Aggregate(cmd, (acc, curr) => acc += " <" + curr + ">"));
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

        public Func<IList<string>, Task> this[string cmd, bool startProgressBar, params string[] cmdArgNames]
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
                            WriteLnRed("Invalid arguments");
                            WriteLn("    Use   " + cmdArgNames.Aggregate(cmd, (acc, curr) => acc += " <" + curr + ">"));
                        }
                        return result;
                    },
                    Run = value,
                    CommandArgNames = cmdArgNames,
                    StartProgressBar = startProgressBar
                };
            }
        }

        volatile int progressPrinterIsActive = 0;

        void StartProgressBar()
        {
            if (Interlocked.CompareExchange(ref progressPrinterIsActive, 1, 0) == 0)
            {
                Task.Run(() =>
                {

                    string line = "";

                    int i = 0;

                    int progressBarLength = 8;
                    int overallLength = 46;
                    StringBuilder strBuilder = new StringBuilder();

                    while (progressPrinterIsActive == 1)
                    {
                        string backup = new string('\b', line.Length);
                        Write(backup);
                        int modRes = (i++) % overallLength;

                        int progBarLen = modRes + 1;
                        progBarLen = progBarLen > progressBarLength ? progressBarLength : progBarLen;

                        if (modRes + 1 + progBarLen > overallLength)
                            progBarLen = overallLength - (modRes + 1);

                        int beforeWhiteSpace = modRes + 1 - progBarLen;
                        int afterWhiteSpace = overallLength - (progBarLen + beforeWhiteSpace);

                        strBuilder.Append(' ', beforeWhiteSpace).Append('-', progBarLen).Append(' ', afterWhiteSpace);

                        line = strBuilder.ToString();
                        strBuilder.Clear();
                        Write(line);

                        Task.Delay(66).GetAwaiter().GetResult();
                    }
                });
            }
        }

        void StopProgressBar()
        {
            Interlocked.CompareExchange(ref progressPrinterIsActive, 0, 1);
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

        void RunCommandHandlerIfPossible(ConsoleCommand cmdHandler, List<string> cmdArgs)
        {
            bool canRun = true;

            if (cmdHandler.Condition != null)
                canRun = cmdHandler.Condition(cmdArgs);

            if (canRun)
            {
                if(cmdHandler.StartProgressBar) StartProgressBar();
                cmdHandler.Run(cmdArgs).ContinueWith(task =>
                {
                    if (cmdHandler.StartProgressBar) StopProgressBar();
                    if (task.IsFaulted)
                        PrintException(task.Exception);
                });
            }
            else
                WriteLn("Something went wrong. Unable to run command");
        }

        void PrintException(Exception exc)
        {
            WriteLn("");
            WriteLnRed(exc.InnerException.Message);
            WriteLnRed(exc.InnerException.StackTrace);

            if(exc.InnerException != null)
                PrintException(exc.InnerException);
        }

        public void PrintHelpForCommand(string cmdKey)
        {
            string helpText = "";
            if (Help.TryGetValue(cmdKey, out helpText))
                WriteLnGreen("    " + helpText);
            else
                WriteLnGreen("No help text found sorry");
        }

        void PrintHeadersIfNotNullOrEmpty()
        {
            if (!String.IsNullOrEmpty(Header)) WriteLnBlue("    " + Header);
            if (!String.IsNullOrEmpty(Subheader)) WriteLnBlue("    " + Subheader + "\n");
        }

        void PrintCmdName(KeyValuePair<string, ConsoleCommand> cmd)
        {
            WriteLnGreen("    " + cmd.Value.CommandArgNamesToString(cmd.Key));
        }

        void PrintHelpText(KeyValuePair<string, ConsoleCommand> cmd)
        {
            string helpText = "";
            Help.TryGetValue(cmd.Key, out helpText);
            WriteLnGreen("    " + helpText);
        }

        void PrintCmdArgNamesIfNotNullOrEmpty(KeyValuePair<string, ConsoleCommand> cmd)
        {
            string cmdArgNamesStr = cmd.Value.CommandArgNamesToString();
            if (!String.IsNullOrEmpty(cmdArgNamesStr))
                Write(" " + cmdArgNamesStr);
        }

        void ExitAndKillSessionIfOpen()
        {

            WriteLnGreen("Shutting down...");
            // On Exit
            if (OnExit != null)
                OnExit();

            WriteLnBlue("Shutdown complete");
            WriteLnBlue("Press any key to exit when shutdown is complete...");
            ReadLine();
        }
    }
}
