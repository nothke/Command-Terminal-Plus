using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommandTerminalPlus
{
    public class TerminalDrawer : MonoBehaviour
    {
        public Terminal terminal;

        void OnGUI()
        {
            if (terminal.ShowGUIButtons)
            {
                terminal.DrawGUIButtons();
            }

            if (terminal.IsClosed)
            {
                enabled = false;
                return;
            }

            terminal.HandleOpenness();
            terminal.DrawWindow();
        }
    }
}