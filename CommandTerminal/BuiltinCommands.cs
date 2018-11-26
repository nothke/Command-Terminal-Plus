using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace CommandTerminalPlus
{
    public static class BuiltinCommands
    {
        [RegisterCommand(Name = "Clear", Help = "Clear the command console", MaxArgCount = 0)]
        static void CommandClear(CommandArg[] args) {
            Terminal.Buffer.Clear();
        }

        [RegisterCommand(Name = "Help", Help = "Display help information about a command", MaxArgCount = 1)]
        static void CommandHelp(CommandArg[] args) {
            if (args.Length == 0) {
                foreach (var command in Terminal.Shell.Commands) {
                    if(!command.Value.secret)
                        Terminal.Log("{0}: {1}", command.Key.PadRight(16), command.Value.help);
                }
                return;
            }

            string command_name = args[0].String.ToUpper();

            if (!Terminal.Shell.Commands.ContainsKey(command_name)) {
                Terminal.Shell.IssueErrorMessage("Command {0} could not be found.", command_name);
                return;
            }

            var info = Terminal.Shell.Commands[command_name];

            if (info.help == null) {
                Terminal.Log("{0} does not provide any help documentation.", command_name);
            } else if (info.usage == null) {
                Terminal.Log(info.help);
            } else {
                Terminal.Log("{0}\nUsage: {1}", info.help, info.usage);
            }
        }

        [RegisterCommand(Name = "Time", Help = "Measure the execution time of a command", MinArgCount = 1)]
        static void CommandTime(CommandArg[] args) {
            var sw = new Stopwatch();
            sw.Start();

            Terminal.Shell.RunCommand(JoinArguments(args));

            sw.Stop();
            Terminal.Log("Time: {0}ms", (double)sw.ElapsedTicks / 10000);
        }

        [RegisterCommand(Name = "Schedule", Help = "Schedule a command to be executed some time in the future", MinArgCount = 2,
            Usage = "schedule [delay] [command] - delay is in seconds")]
        static void CommandSchedule(CommandArg[] args)
        {
            Terminal.RunCommandAfterDelay(args[0].Float, JoinArguments(args, 1), scaledTime: true);
        }

        [RegisterCommand(Name = "ScheduleUnscaled", Help = "Schedule a command using the time scale", MinArgCount = 2,
            Usage = "schedule [delay] [command] - delay is in seconds")]
        static void CommandScheduleUnScaled(CommandArg[] args)
        {
            Terminal.RunCommandAfterDelay(args[0].Float, JoinArguments(args, 1), scaledTime: false);
        }

        [RegisterCommand(Name = "Print", Help = "Output message")]
        static void CommandPrint(CommandArg[] args) {
            Terminal.Log(JoinArguments(args));
        }

    #if DEBUG
        [RegisterCommand(Name = "Trace", Help = "Output the stack trace of the previous message", MaxArgCount = 0)]
        static void CommandTrace(CommandArg[] args) {
            int log_count = Terminal.Buffer.Logs.Count;

            if (log_count - 2 < 0) {
                Terminal.Log("Nothing to trace.");
                return;
            }

            var log_item = Terminal.Buffer.Logs[log_count - 2];

            if (log_item.stack_trace == "") {
                Terminal.Log("{0} (no trace)", log_item.message);
            } else {
                Terminal.Log(log_item.stack_trace);
            }
        }
    #endif

        [RegisterCommand(Name = "Set", Help = "List all variables or set a variable value")]
        static void CommandSet(CommandArg[] args) {
            if (args.Length == 0) {
                foreach (var v in Terminal.Shell.Variables) {
                    Terminal.Log("{0}: {1}", v.PadRight(16), Terminal.Shell.GetVariable(v));
                }
                return;
            }

            string variable_name = args[0].String;

            try
            {
                Terminal.Shell.SetVariable(variable_name, JoinArguments(args, 1));
            }
            catch(Exception e)
            {
                throw e?.InnerException ?? e;
            }
        }

        [RegisterCommand(Name = "Bind", Help = "Bind a key to a command", MinArgCount = 2,
            Usage = "bind [keycode] [command] - see https://docs.unity3d.com/ScriptReference/KeyCode.html for a list of valid keycodes")]
        static void CommandBind(CommandArg[] args)
        {
            string fullCommand = JoinArguments(args, start: 1);
            Terminal.AddBinding(args[0].AsEnum<KeyCode>(), fullCommand);
        }

        [RegisterCommand(Name = "Unbind", Help = "Remove all bindings from a key", MinArgCount = 1, MaxArgCount = 1,
            Usage = "unbind [keycode] - see https://docs.unity3d.com/ScriptReference/KeyCode.html for a list of valid keycodes")]
        static void CommandUnbind(CommandArg[] args)
        {
            Terminal.ResetBinding(args[0].AsEnum<KeyCode>());
        }

        [RegisterCommand(Name = "Screenshot", Help = "Save a screenshot of the game. You probably want to bind this to a key", MaxArgCount = 2, 
            Usage = "screenshot [supersize] [file name or path]")]
        static void CommandScreenshot(CommandArg[] args)
        {
            var filePath = Path.Combine(Application.persistentDataPath, "screenshots", DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff"));
            var superSize = 1;

            if (args.Length > 0)
                superSize = args[0].Int;

            if (args.Length > 1)
            {
                var name = args[1].String;
                if (Path.IsPathRooted(name))
                    filePath = name;
                else
                    filePath = Path.Combine(Application.persistentDataPath, "screenshots", name);
            }

            filePath = filePath.Replace('\\', '/'); // this is mostly so that the Terminal.Log message looks consistent on Windows
            filePath = Path.ChangeExtension(filePath, ".png");
            string folderPath = new DirectoryInfo(filePath).Parent.FullName;
            Directory.CreateDirectory(folderPath);

            ScreenCapture.CaptureScreenshot(filePath, superSize);
            Terminal.Log($"saved screenshot as {filePath} (supersize {superSize})");
        }

        [RegisterCommand(Name = "Noop", Help = "No operation")]
        static void CommandNoop(CommandArg[] args) { }

        [RegisterCommand(Name = "Quit", Secret = true)]
        static void CommandQuit(CommandArg[] args) => CommandExit(args);

        [RegisterCommand(Name = "Exit", Help = "Quit running application. 'exit' also works.", MaxArgCount = 0)]
        static void CommandExit(CommandArg[] args) {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }

        static string JoinArguments(CommandArg[] args, int start = 0) {
            var sb = new StringBuilder();
            int arg_length = args.Length;

            for (int i = start; i < arg_length; i++) {
                sb.Append(args[i].String);

                if (i < arg_length - 1) {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }
    }
}
