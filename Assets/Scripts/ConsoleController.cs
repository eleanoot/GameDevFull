// Implements the development console commands for dev manipulation of the game for test purposes.
// Adapted from code by Eliot Lash 2014-2015.
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

public delegate void CommandHandler(string[] args);

public class ConsoleController {
	
	// Used to communicate with ConsoleView
	public delegate void LogChangedHandler(string[] log);
	public event LogChangedHandler logChanged;
	
	public delegate void VisibilityChangedHandler(bool visible);
	public event VisibilityChangedHandler visibilityChanged;

	// Object to hold information about each command.
	class CommandRegistration {
		public string command { get; private set; }
		public CommandHandler handler { get; private set; }
		public string help { get; private set; }
		
		public CommandRegistration(string command, CommandHandler handler, string help) {
			this.command = command;
			this.handler = handler;
			this.help = help;
		}
	}

	/// <summary>
	/// How many log lines should be retained?
	/// Note that strings submitted to appendLogLine with embedded newlines will be counted as a single line.
	/// </summary>
	const int scrollbackSize = 20;

	Queue<string> scrollback = new Queue<string>(scrollbackSize);
	List<string> commandHistory = new List<string>();
	Dictionary<string, CommandRegistration> commands = new Dictionary<string, CommandRegistration>();

	public string[] log { get; private set; } //Copy of scrollback as an array for easier use by ConsoleView
	
	
	public ConsoleController() {
		//When adding commands, you must add a call below to registerCommand() with its name, implementation method, and help text.
		registerCommand("help", help, "Print this help.");
		registerCommand("hide", hide, "Hide the console.");
        registerCommand("allitems", listAllItems, "List all the possible items to spawn.");
        registerCommand("allenemies", listAllEnemies, "List all the possible enemies to spawn.");
		registerCommand("restart", restart, "Restart run.");
        registerCommand("stats", printStats, "Prints the player's current stats.");
        registerCommand("settime", setTimer, "Set the run timer to the given value.");
        registerCommand("spawnitem", spawnItem, "Spawns the given item at the given position.");
        registerCommand("spawnenemy", spawnEnemy, "Spawns the given enemy at the given position.");
        registerCommand("setstat", setStat, "Set the value of the given stat.");
	}
	
	void registerCommand(string command, CommandHandler handler, string help) {
		commands.Add(command, new CommandRegistration(command, handler, help));
	}
	
	public void appendLogLine(string line) {
		
		if (scrollback.Count >= ConsoleController.scrollbackSize) {
			scrollback.Dequeue();
		}
		scrollback.Enqueue(line);
		
		log = scrollback.ToArray();
		if (logChanged != null) {
			logChanged(log);
		}
	}
	
	public void runCommandString(string commandString) {
		appendLogLine("$ " + commandString);
		
		string[] commandSplit = parseArguments(commandString);
		string[] args = new string[0];
		if (commandSplit.Length < 1) {
			appendLogLine(string.Format("Unable to process command '{0}'", commandString));
			return;
			
		} else if (commandSplit.Length >= 2) {
			int numArgs = commandSplit.Length - 1;
			args = new string[numArgs];
			Array.Copy(commandSplit, 1, args, 0, numArgs);
		}
		runCommand(commandSplit[0].ToLower(), args);
		commandHistory.Add(commandString);
	}
	
	public void runCommand(string command, string[] args) {
		CommandRegistration reg = null;
		if (!commands.TryGetValue(command, out reg)) {
			appendLogLine(string.Format("Unknown command '{0}', type 'help' for list.", command));
		} else {
			if (reg.handler == null) {
				appendLogLine(string.Format("Unable to process command '{0}', handler was null.", command));
			} else {
				reg.handler(args);
			}
		}
	}
	
	static string[] parseArguments(string commandString)
	{
		LinkedList<char> parmChars = new LinkedList<char>(commandString.ToCharArray());
		bool inQuote = false;
		var node = parmChars.First;
		while (node != null)
		{
			var next = node.Next;
			if (node.Value == '"') {
				inQuote = !inQuote;
				parmChars.Remove(node);
			}
			if (!inQuote && node.Value == ' ') {
				node.Value = '\n';
			}
			node = next;
		}
		char[] parmCharsArr = new char[parmChars.Count];
		parmChars.CopyTo(parmCharsArr, 0);
		return (new string(parmCharsArr)).Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
	}

	#region Command handlers
	//Implement new commands in this region of the file.


    void printStats(string[] args)
    {
        appendLogLine(string.Format("Max HP: {0}", Stats.MaxHp));
        appendLogLine(string.Format("Damage: {0}", Stats.Dmg));
        appendLogLine(string.Format("Range: {0}", Stats.Range));
        appendLogLine(string.Format("Speed: {0}", Stats.Speed));
    }

    void setTimer(string[] args)
    {
        if (args.Length < 1)
        {
            appendLogLine("No time value given. Expected use: settimer [time in seconds]");
            return;
        }

        float newTime = 0.0f;
        if (!float.TryParse(args[0], out newTime))
        {
            appendLogLine("Expected a number value.");
        }
        else
        {
            Manager.instance.ResetTimer(newTime);
        }
        
    }
    

    void spawnItem(string[] args)
    {
        if (args.Length < 3)
        {
            appendLogLine("Expected use: [ItemName] [x coord] [y coord]. Were spaces included?");
        }

        string itemName = args[0];
        if (string.IsNullOrEmpty(itemName))
        {
            appendLogLine("Expected arg1 to be text.");
        }

        int x = 0;
        if (!Int32.TryParse(args[1], out x))
        {
            appendLogLine("Expected an integer for arg2.");
        }
        int y = 0;
        if (!Int32.TryParse(args[2], out y))
        {
            appendLogLine("Expected an integer for arg3.");
        }


        // Spawn the item given by the parameter. Will always be at the same point.
        GameObject prefab = GameObject.Instantiate(Resources.Load("Items/" + itemName)) as GameObject;
        prefab.transform.position = new Vector2(x - 4 + 0.5f, y - 4 + 0.5f);
    }

    void listAllItems(string[] args)
    {
        foreach(GameObject i in ItemManager.instance.allItems)
        {
            appendLogLine(string.Format("{0}", i.name));
        }
    }

    void listAllEnemies(string[] args)
    {
        appendLogLine("FoxMage");
        appendLogLine("FoxMageDark");
        appendLogLine("MouseBasic");

    }

    void spawnEnemy(string[] args)
    {
        if (args.Length < 3)
        {
            appendLogLine("Expected use: [EnemyName] [x coord] [y coord]. Were spaces included?");
        }

        string enemyName = args[0];
        if (string.IsNullOrEmpty(enemyName))
        {
            appendLogLine("Expected arg1 to be text.");
        }

        int x = 0;
        if (!Int32.TryParse(args[1], out x))
        {
            appendLogLine("Expected an integer for arg2.");
        }
        int y = 0;
        if (!Int32.TryParse(args[1], out y))
        {
            appendLogLine("Expected an integer for arg3.");
        }


        // Spawn the item given by the parameter. Will always be at the same point.
        GameObject prefab = GameObject.Instantiate(Resources.Load("Enemies/" + enemyName)) as GameObject;
        prefab.transform.position = new Vector2(x - 4 + 0.5f, y - 4 + 0.5f);
    }

    void setStat(string[] args)
    {
        if (args.Length < 2)
        {
            appendLogLine("Expected use: [Stat] [value]");
        }

        string stat = args[0];

        int value = 0;
        if (!Int32.TryParse(args[1], out value))
        {
            appendLogLine("Expected an integer for arg2.");
        }

        switch (stat)
        {
            case "Range": Stats.Range = value; break;
            case "Damage":
            case "Dmg": Stats.Dmg = value; break;
            case "HP":
            case "Health":
            case "hp":
            case "Hp": Stats.Heal(value); break;
            case "Speed": Stats.Speed = value / 10; break;
            default:
                appendLogLine("Error: stat not recognised"); break;

        }
    }

   

	void help(string[] args) {
		foreach(CommandRegistration reg in commands.Values) {
			appendLogLine(string.Format("{0}: {1}", reg.command, reg.help));
		}
	}
	
	void hide(string[] args) {
		if (visibilityChanged != null) {
			visibilityChanged(false);
		}
	}
	
	
	void restart(string[] args) {
        Manager.instance.RestartGame();
	}
	
	#endregion
}
