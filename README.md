## Update 2019-08-30

I will no longer be maintaining Command Terminal Plus. In my personal projects, I have replaced it with a frontend for [LICC](http://licc.software) called Fancy Pants Console. You can see FPC in action [here](https://www.youtube.com/watch?v=QDp5wE1Se6o). I do plan on open sourcing FPC eventually, but it relies on a lot of custom UI code so that will happen after we open source our custom UI library.

If you would like to take ownership of this repo, hit me up.


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
