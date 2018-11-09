Command Terminal Plus
======================

This is a fork of [stillwwater/command_terminal](https://github.com/stillwwater/command_terminal) with a bunch more features. Here's a list of them so far:

* added `bind` default command, which allows you to bind any command to a keyboard key. Keys can have multiple commands bound to them.
* added `unbind` default command, to reset bindings of a key
* added `schedule` default command, to schedule a command to execute in the future
* added `scheduleunscaled` default command, which is like `schedule` but it uses unscaled time
* added `screenshot` default command, which can be used with custom values for supersize, file name and file path
* removed the (IMO pointless) variable system from the orginal. Now you use the `set` command to modify or read properties in your game. Use the `[RegisterVariable]` attribute for this.
* added `timescale` default variable, for modifying `UnityEngine.Time.timeScale`
* added `handleunitylog` default variable, which can be used to disable unity console output in the terminal
* added user-editable file StartupCommands.txt. When the terminal starts up, each line of this file is read. Each line which is not empty and does not start with the character `#` (used for comments) is run as a command.
* cursor is now automatically unlocked when the terminal is opened. It is set back to its previous state (locked or unlocked) when the terminal is closed.
* pressing enter on the numpad can also be used to input a command. This is thanks to [@bgr](https://github.com/bgr)'s [pull request](https://github.com/stillwwater/command_terminal/pull/8) on the orginal repo.
* terminal font size is customizable
* you can now get a CommandArg as any enum type
* added events when the terminal opens and closes. This is useful if you have a player controller you want to disable while the terminal is open.
* added `Secret` bool to RegisterCommandAttribute. If it's true, the command won't show up with the `help` command. This is intended for easter eggs.
* added secret `exit` command which does the same thing as `quit`
* tweaked some help messages on default commands
* tweaked some variable names to be more self-explanitory
* generally improved a bunch of code
