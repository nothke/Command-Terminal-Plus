using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandTerminalPlus
{
    public enum AssemblyLoadingBehavior
    {
        All,
        Custom
    }

    public class CommandShell
    {
        Dictionary<string, CommandInfo> commands = new Dictionary<string, CommandInfo>();
        Dictionary<string, PropertyInfo> variables = new Dictionary<string, PropertyInfo>();
        List<CommandArg> arguments = new List<CommandArg>(); // Cache for performance

        public string IssuedErrorMessage { get; private set; }

        public Dictionary<string, CommandInfo> Commands {
            get { return commands; }
        }

        public List<string> Variables {
            get { return new List<string>(variables.Keys); }
        }

        /// <summary>
        /// Uses reflection to find all RegisterCommand and RegisterVariable attributes
        /// and adds them to the commands dictionary.
        /// 
        /// If loading behavior is "All", it will read from all assemblies.
        /// For improving startup performance it is recommended to only load
        /// from manually set additional assemblies 
        /// (Terminal and CSharp script assemblies are already included)
        /// </summary>
        public void RegisterCommandsAndVariables(
            AssemblyLoadingBehavior assemblyLoadingBehavior,
            HashSet<string> additionalAssembliesToLoad)
        {
            var rejected_commands = new Dictionary<string, CommandInfo>();
            var method_flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var property_flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            // Cherry pick assemblies to check
            List<Assembly> assemblies = new List<Assembly>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
            if (assemblyLoadingBehavior == AssemblyLoadingBehavior.All)
            {
                assemblies.AddRange(allAssemblies);
            }
            else
            {
                for (int i = 0; i < allAssemblies.Length; i++)
                {
                    var _name = allAssemblies[i].GetName().Name;

                    if (_name == "Assembly-CSharp")
                    {
                        assemblies.Add(allAssemblies[i]);
                    }
                    else if (additionalAssembliesToLoad.Contains(_name))
                    {
                        assemblies.Add(allAssemblies[i]);
                    }
                }

                assemblies.Add(Assembly.GetExecutingAssembly());
            }

            foreach (var assembly in assemblies)
            {
                //Debug.Log($"Reading assembly \"{assembly.GetName().Name}\"");

                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(method_flags))
                    {
                        var attribute = Attribute.GetCustomAttribute(
                            method, typeof(RegisterCommandAttribute)) as RegisterCommandAttribute;

                        if (attribute == null)
                        {
                            if (method.Name.StartsWith("FRONTCOMMAND", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // Front-end Command methods don't implement RegisterCommand, use default attribute
                                attribute = new RegisterCommandAttribute();
                            }
                            else
                            {
                                continue;
                            }
                        }

                        var methods_params = method.GetParameters();

                        string command_name = InferFrontCommandName(method.Name);
                        Action<CommandArg[]> proc;

                        if (attribute.Name == null)
                        {
                            // Use the method's name as the command's name
                            command_name = InferCommandName(command_name == null ? method.Name : command_name);
                        }
                        else
                        {
                            command_name = attribute.Name;
                        }

                        if (methods_params.Length != 1 || methods_params[0].ParameterType != typeof(CommandArg[]))
                        {
                            // Method does not match expected Action signature,
                            // this could be a command that has a FrontCommand method to handle its arguments.
                            rejected_commands.Add(command_name.ToUpper(), CommandFromParamInfo(methods_params, attribute.Help));
                            continue;
                        }

                        // Convert MethodInfo to Action.
                        // This is essentially allows us to store a reference to the method,
                        // which makes calling the method significantly more performant than using MethodInfo.Invoke().
                        proc = (Action<CommandArg[]>)Delegate.CreateDelegate(typeof(Action<CommandArg[]>), method);
                        AddCommand(command_name, proc, attribute.MinArgCount, attribute.MaxArgCount, attribute.Help, attribute.Usage, attribute.Secret);
                    }

                    foreach (var property in type.GetProperties(property_flags))
                    {
                        var attribute = Attribute.GetCustomAttribute(
                            property, typeof(RegisterVariableAttribute)) as RegisterVariableAttribute;

                        if (attribute == null) continue;

                        string variable_name = attribute.Name ?? property.Name;

                        AddVariable(variable_name, property);
                    }
                }
                HandleRejectedCommands(rejected_commands);
            }
        }

        /// <summary>
        /// Parses an input line into a command and runs that command.
        /// </summary>
        public void RunCommand(string line) {
            Terminal.Log(line, TerminalLogType.Input);

            string remaining = line;
            IssuedErrorMessage = null;
            arguments.Clear();

            while (remaining != "") {
                var argument = EatArgument(ref remaining);

                if (argument.String != "") {
                    string variable_name = argument.String.Substring(1).ToUpper();

                    arguments.Add(argument);
                }
            }

            if (arguments.Count == 0) {
                // Nothing to run
                return;
            }

            string command_name = arguments[0].String.ToUpper();
            arguments.RemoveAt(0); // Remove command name from arguments

            if (!commands.ContainsKey(command_name)) {
                IssueErrorMessage("Command {0} could not be found", command_name);
                return;
            }

            RunCommand(command_name, arguments.ToArray());
        }

        private void RunCommand(string command_name, CommandArg[] arguments) {
            var command = commands[command_name];
            int arg_count = arguments.Length;
            string error_message = null;
            int required_arg = 0;

            if (arg_count < command.min_arg_count) {
                if (command.min_arg_count == command.max_arg_count) {
                    error_message = "exactly";
                } else {
                    error_message = "at least";
                }
                required_arg = command.min_arg_count;
            } else if (command.max_arg_count > -1 && arg_count > command.max_arg_count) {
                // Do not check max allowed number of arguments if it is -1
                if (command.min_arg_count == command.max_arg_count) {
                    error_message = "exactly";
                } else {
                    error_message = "at most";
                }
                required_arg = command.max_arg_count;
            }

            if (error_message != null) {
                string plural_fix = required_arg == 1 ? "" : "s";

                IssueErrorMessage(
                    "{0} requires {1} {2} argument{3}",
                    command_name,
                    error_message,
                    required_arg,
                    plural_fix
                );

                ShowUsage();
                return;
            }

            try
            {
                command.proc(arguments);
            }
            catch (Exception e)
            {
                IssueErrorMessage(e.Message);
            }

            if (IssuedErrorMessage != null)
                ShowUsage();

            void ShowUsage()
            {
                if (command.usage != null)
                    IssuedErrorMessage += string.Format("\n    -> Usage: {0}", command.usage);
            }
        }

        public void AddCommand(string name, CommandInfo info) {
            name = name.ToUpper();

            if (commands.ContainsKey(name)) {
                IssueErrorMessage("Command {0} is already defined.", name);
                return;
            }

            commands.Add(name, info);
        }

        public void AddCommand(string name, Action<CommandArg[]> proc, int min_args = 0, int max_args = -1, string help = "", string usage = null, bool secret = false) {
            var info = new CommandInfo() {
                proc = proc,
                min_arg_count = min_args,
                max_arg_count = max_args,
                help = help,
                usage = usage,
                secret = secret,
            };

            AddCommand(name, info);
        }

        public void AddVariable(string name, PropertyInfo info)
        {
            if (!IsAllowedPropertyType(info.PropertyType))
                throw new Exception($"can't register property {info.Name} - registered variables must be string, int, float, bool or enum");

            name = name.ToUpper();

            if (variables.ContainsKey(name))
                throw new Exception($"there is already a variable called {name}");

            variables.Add(name, info);
        }

        bool IsAllowedPropertyType(Type type)
        {
            return type == typeof(string)
                || type == typeof(int)
                || type == typeof(float)
                || type == typeof(bool)
                || type.IsEnum;
        }

        public void SetVariable(string name, string value) {
            SetVariable(name, new CommandArg() { String = value });
        }

        public void SetVariable(string name, CommandArg arg) {
            name = name.ToUpper();

            if (!variables.ContainsKey(name))
                throw new Exception($"no variable registered with name {name}");

            object value = null;

            var propertyType = variables[name].PropertyType;

            if (propertyType == typeof(string))
                value = arg.String;
            else if (propertyType == typeof(int))
                value = arg.Int;
            else if (propertyType == typeof(float))
                value = arg.Float;
            else if (propertyType == typeof(bool))
                value = arg.Bool;
            else if (propertyType.IsEnum)
                value = Enum.Parse(propertyType, arg.String);

            variables[name].SetMethod.Invoke(null, new object[] { value });
        }

        public object GetVariable(string name) {
            name = name.ToUpper();

            if (!variables.ContainsKey(name))
                throw new Exception($"no variable registered with name {name}");

            return variables[name].GetMethod.Invoke(null, null);
        }

        public void IssueErrorMessage(string format, params object[] message) {
            IssuedErrorMessage = string.Format(format, message);
        }

        string InferCommandName(string method_name) {
            string command_name;
            int index = method_name.IndexOf("COMMAND", StringComparison.CurrentCultureIgnoreCase);

            if (index >= 0) {
                // Method is prefixed, suffixed with, or contains "COMMAND".
                command_name = method_name.Remove(index, 7);
            } else {
                command_name = method_name;
            }

            return command_name;
        }

        string InferFrontCommandName(string method_name) {
            int index = method_name.IndexOf("FRONT", StringComparison.CurrentCultureIgnoreCase);
            return index >= 0 ? method_name.Remove(index, 5) : null;
        }

        void HandleRejectedCommands(Dictionary<string, CommandInfo> rejected_commands) {
            foreach (var command in rejected_commands) {
                if (commands.ContainsKey(command.Key)) {
                    commands[command.Key] = new CommandInfo() {
                        proc = commands[command.Key].proc,
                        min_arg_count = command.Value.min_arg_count,
                        max_arg_count = command.Value.max_arg_count,
                        help = command.Value.help
                    };
                } else {
                    IssueErrorMessage("{0} is missing a front command.", command);
                }
            }
        }

        CommandInfo CommandFromParamInfo(ParameterInfo[] parameters, string help) {
            int optional_args = 0;

            foreach (var param in parameters) {
                if (param.IsOptional) {
                    optional_args += 1;
                }
            }

            return new CommandInfo() {
                proc = null,
                min_arg_count = parameters.Length - optional_args,
                max_arg_count = parameters.Length,
                help = help
            };
        }

        static Dictionary<char, char> argumentGroupMarkers = new Dictionary<char, char>
        {
            {'\'', '\''},
            {'"', '"'},
            {'<', '>'},
            {'[', ']'}
        };
        
        CommandArg EatArgument(ref string s) {
            var arg = new CommandArg();

            int endIndex = -1;
            bool mutliWord = false;
            if(argumentGroupMarkers.TryGetValue(s[0], out char endChar))
            {
                endIndex = s.IndexOf(endChar, 1);
                mutliWord = true;
            }
            else
            {
                endIndex = s.IndexOf(' ');
            }

            if (endIndex >= 0) {
                arg.String = s.Substring(mutliWord ? 1 : 0, mutliWord ? endIndex - 1 : endIndex);
                s = s.Substring(endIndex + 1); // Remaining
            } else {
                arg.String = s;
                s = "";
            }

            return arg;
        }
    }
}
