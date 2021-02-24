Command Terminal Plus
======================

This is a fork of [stillwwater/command_terminal](https://github.com/stillwwater/command_terminal) with a bunch more features. Here's a list of them so far:

* added `bind` default command, which allows you to bind any command to a keyboard key. There can be multiple commands bound to a single key.
* added `unbind` default command, to reset bindings of a key
* added `schedule` default command, to schedule a command to execute in the future
* added `scheduleunscaled` default command, which is like `schedule` but it uses unscaled time
* added `screenshot` default command, which can be used with custom values for supersize, file name and file path
* removed the (IMO pointless) variable system from the original. Now you use the `set` command to modify or read properties in your game. Use the `[RegisterVariable]` attribute for this.
* added `timescale` default variable, for modifying `UnityEngine.Time.timeScale`
* added `handleunitylog` default variable, which can be used to disable unity console output in the terminal
* added user-editable file StartupCommands.txt. When the terminal starts up, each line of this file is read. Each line which is not empty and does not start with the character `#` (used for comments) is run as a command.
* cursor is now automatically unlocked when the terminal is opened. It is set back to its previous state (locked or unlocked) when the terminal is closed.
* pressing enter on the numpad can also be used to input a command. This is thanks to [@bgr](https://github.com/bgr)'s [pull request](https://github.com/stillwwater/command_terminal/pull/8) on the orginal repo.
* terminal font size is customizable
* you can now get a CommandArg as any enum type
* commands (and variables) are registered in ALL assemblies, not just the main assembly
* added events when the terminal opens and closes. This is useful if you have a player controller you want to disable while the terminal is open.
* commands now accept yes/no/y/n/on/off as values for booleans
* added `Secret` bool to RegisterCommandAttribute. If it's true, the command won't show up with the `help` command. This is intended for easter eggs.
* added secret `exit` command which does the same thing as `quit`
* `Terminal.cs.meta` is part of version control, so if you use CTP as a submodule, it doesn't break when loaded on somebody else's computer
* added the necessary files (`package.json` and `CommandTerminalPlus.asmdef`) so that it can be used as a Unity package via the package manager
* tweaked some help messages on default commands
* tweaked some variable names to be more self-explanatory
* fixed default commands having the incorrect names in WebGL
* generally improved a bunch of code

Nothke's changes:
* Added an option to manually select which assemblies to load. The terminal was loading all assemblies (asmdefs, which includes packages) by default, and projects that have a large amount of assemblies would take a long time to start up (5+ seconds in my project)
* Moved OnGUI method into a separate TerminalDrawer component to eliminate GC allocations when terminal is closed
* Merged [@Wokaroi's](https://github.com/Wokarol) addition of multiword arguments
