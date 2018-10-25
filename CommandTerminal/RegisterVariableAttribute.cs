using System;
using System.Reflection;

namespace CommandTerminalPlus
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RegisterVariableAttribute : Attribute
    {
        public string Name { get; set; }

        public RegisterVariableAttribute(string command_name = null)
        {
            Name = command_name;
        }
    }
}
