using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DevConsole : MonoBehaviour {
    public class ConsoleCommand {
        public string main;
        public string desc;

        public delegate void Action(string[] args, Player player);
        public Action moment;
    }


    public bool fardNation = false;
    bool tabbed = false;
    string command = "";
    string lastCommand = "";
    private List<ConsoleCommand> commands;

    private void Start() {
        commands = new List<ConsoleCommand> {
            new ConsoleCommand {
                main = "repeat",
                desc = "Repeats the last command (#1) times",
                moment = (args, player) => {
                    if (!lastCommand.StartsWith("repeat")) {
                        for (int i = 0; i < int.Parse(args[0]); i++) {
                            Parse(lastCommand);
                        }
                    }
                },
            },
            new ConsoleCommand {
                main = "kill",
                desc = "Kills the player",
                moment = (args, player) => { player.health.health.Damage(100000); },
            },
            new ConsoleCommand {
                main = "give",
                desc = "Gives the specified item (#1) with count (#2) to player",
                moment = (args, player) => {
                    string item = args[0];
                    int count = args.Length == 1 ? 1 : int.Parse(args[1]);
                    player.inventory.AddItemUnclamped(new ItemStack(item, count));
                },
            },
            new ConsoleCommand {
                main = "set_speed",
                desc = "Sets the speed of the player to #1",
                moment = (args, player) => {
                    //player.controller.speed = float.Parse(args[0]);
                },
            },
            new ConsoleCommand {
                main = "set_accel",
                desc = "Sets the acceleration of the player to #1",
                moment = (args, player) => {
                    //player.controller.maxAcceleration = float.Parse(args[0]);
                },
            },
            new ConsoleCommand {
                main = "heal",
                desc = "Heals the player by the ammount specified by #1",
                moment = (args, player) => {
                    float ammount = float.Parse(args[0]);
                    player.health.health.Heal(ammount);
                },
            },
            new ConsoleCommand {
                main = "damage",
                desc = "Damages the player by the ammount specified by #1",
                moment = (args, player) => {
                    float ammount = float.Parse(args[0]);
                    player.health.health.Damage(ammount);
                },
            },
            new ConsoleCommand {
                main = "summon",
                desc = "Summons an entity (#1). Ex: bots base | items snowball | projectiles snowball 10",
                moment = (args, player) => {
                    string type = args[0];
                    string name = args[1];

                    if (type == "bots") {
                        BotData data = Registries.bots[name];
                        BotBase.Summon(data, player.transform.position, Quaternion.identity);
                    } else if (type == "items") {
                        WorldItem.Spawn(new ItemStack(name, 1), player.camera.transform.forward + player.camera.transform.position, Quaternion.identity);
                    } else if (type == "projectiles") {
                        ProjectileItemData data = (ProjectileItemData)Registries.items[name];
                        Projectile.Spawn(data, player.camera.transform.forward + player.camera.transform.position, player.camera.transform.forward * float.Parse(args[2]));
                    }
                },
            },
            new ConsoleCommand {
                main = "explode",
                desc = "Blows up",
                moment = (args, player) => {
                    Utils.BlowUp(player.transform.position);
                },
            },
            new ConsoleCommand {
                main = "save",
                desc = "Save the current session to the appopriate save folder",
                moment = (args, player) => {
                    if (PersistentSaveManager.Instance != null) {
                        PersistentSaveManager.Instance.Save();
                    } else {
                        Debug.LogWarning("Can't do shit, don't have a save manager");
                    }
                },
            },
            new ConsoleCommand {
                main = "load",
                desc = "Load a backup of the current save (last save state)",
                moment = (args, player) => {
                    if (PersistentSaveManager.Instance != null) {
                        PersistentSaveManager.Instance.Load();
                    } else {
                        Debug.LogWarning("Can't do shit, don't have a save manager");
                    }
                },
            },
        };
    }

    private void OnGUI() {
        if (!fardNation)
            return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 30;
        style.normal.textColor = Color.white;

        GUIStyle descStyle = new GUIStyle();
        descStyle.fontSize = 30;
        descStyle.normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);

        Rect rect = new Rect(0f, Screen.height - 30, Screen.width, 30);
        GUI.Box(rect, "");
        GUI.SetNextControlName("console");
        GUI.contentColor = Color.white;
        command = GUI.TextField(rect, command, style);
        GUI.FocusControl("console");

        if (tabbed) {
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            te.cursorIndex = command.Length;
            te.selectIndex = command.Length;
            tabbed = false;
        }

        GUI.SetNextControlName("");

        ConsoleCommand[] cmds = commands.AsEnumerable()
            .Where((cmd) => command.Length < cmd.main.Length)
            .Where((cmd) => cmd.main.Substring(0,command.Length) == command)
            .ToArray();

        float value = 30 * cmds.Length;
        Rect anotherRect = new Rect(0f, Screen.height - value - 30, Screen.width, value);
        GUI.Box(anotherRect, "");
        float offset = 30 * 2;
        foreach (ConsoleCommand cmd in cmds) {
            Rect aaa = new Rect(0f, Screen.height - offset, Screen.width, 30);
            offset += 30;
            GUI.Label(aaa, cmd.main, style);

            aaa.x = Screen.width / 3;
            GUI.Label(aaa, cmd.desc, descStyle);
        }

        Event a = Event.current;
        if (a.keyCode == KeyCode.Return || a.keyCode == KeyCode.KeypadEnter) {
            UIScriptMaster.Instance.inGameHUD.ToggleDevConsole();
            fardNation = false;
            Parse(command);
            lastCommand = command;
            command = "";
        }

        if (a.keyCode == KeyCode.Tab && cmds.Length > 0) {
            command = cmds[0].main;
            GUI.FocusControl("console");
            tabbed = true;
        }

        if (a.keyCode == KeyCode.UpArrow) {
            command = lastCommand;
        }
    }

    private void Parse(string command) {
        command = command.Trim();

        if (command == "")
            return;

        string[] splat = command.Split(' ');
        ConsoleCommand cmd = commands.Find((cmd) => splat[0] == cmd.main);

        if (cmd == null)
            return;

        cmd.moment.Invoke(splat[1..], Player.Instance);
        /*
        try {
        } catch (Exception) {
            Debug.LogWarning("Command invocation exception");
            throw;
        }
        */
    }
}
