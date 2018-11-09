using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandTerminalPlus
{
    public struct CommandInfo
    {
        public Action<CommandArg[]> proc;
        public int max_arg_count;
        public int min_arg_count;
        public string help;
        public string usage;
        public bool secret;
    }
}
