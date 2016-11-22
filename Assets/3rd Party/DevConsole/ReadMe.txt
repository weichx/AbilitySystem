/*************************************************
DevConsole v2.0 by CobsTech
**************************************************/

/*************************************************
HOW TO
**************************************************/
//****UPGRADE NOTES****
- Commands are now handled through .Net Generic. That means that are no more "OneStringCommand" or "NoArgsCommand" commands.
- Instead, every command can be created specifiying the type of the parameter i.e. "new Command<string, int>(...)".
- Another important change is that you can now execute any given command by code through "Console.ExecuteCommand".

//****CREATING THE CONSOLE****
- To add the Console to your project, just add the "Console" script to one of your gameobjects.
- It is recommended that you use the given prefab "_DevConsole".
- It is recommended to use a single purpouse gameobject as a Console, and there should only be one per scene.
- You can make the console persistent throughout scenes setting the "dontDestroyOnLoad" variable to true.

//****USING THE CONSOLE****
- Open/Close Console: "consoleKey" KeyCode button.
- When a command is entered, a function linked to the command is executed.
- Clear the Console: "DC_CLEAR" command.
- Everything typed after the command name (and a space) will be passed to the function as parameters.
- Use the Console for Debug: "DC_SHOW_DEBUGLOG" command with a value of "1", "true", or "yes".
- Show help for a specific command: Add a '?' at the end of the command. e.g. "DC_CLEAR?".
- Show all commands available: Type the "HELP" command.

//Completion Window
- A Completion Window appears when the text matches one or more commands.
- Navigate: UP and DOWN arrow keys.
- Close: ESC key.
- Show: F1 key.
- Autocomplete: TAB key.

//History
- When there is no text in the input field, you can access the history by pressing UP or DOWN arrow keys.
- Navigate: UP and DOWN arrow keys.

//Use Console through Code
- Calling the Console "Log" method logs a message to the Console.
- You can also specify a color for the text.
- Alternatively, calling "LogWarning", "LogInfo" and "LogError" will log a light blue, yellow and red text, respectively.

//****ADDING COMMANDS****
Add command by calling the "Console.AddCommand" at anytime, as it is done in the "Example" script.
1) When adding a new command, the type of its arguments (if any) should be specified.
   For instance, to call a method with this signature "void TestMehotd(string text, int n)", the command would be created such as "new Command<string, int>(...)".
2) The constructor takes a delegate as a reference to the method. Note that the signature should match the Generics types specified.
    2.1) Alternatively, before passing the delegate, a string can be specified. That will be the name for the command showing on the console.
    2.2) If no string is used, the name for the command will be the full name of the method, including its class.
3) The last parameter, if added, represents the help or info shown with '?'. e.g. "DC_CLEAR?".
	3.1) Usually, you just want to shown some invariable string, so you should use the string overload.
	3.2) Sometimes, though, you want to run a function when asking for help. To do that, pass a reference to a method with no parameters.
4) There are some examples in the "Example" script.
