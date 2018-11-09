using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandTerminalPlus
{
    public static class BuiltinVariables
    {
        [RegisterVariable]
        public static bool HandleUnityLog { get => Terminal.LogUnityMessages; set => Terminal.LogUnityMessages = value; }

        [RegisterVariable]
        public static float TimeScale { get => Time.timeScale; set => Time.timeScale = value; }
    }
}