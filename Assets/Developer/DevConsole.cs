using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build;
using UnityEngine;
using System.Linq;

public class DevConsole : MonoBehaviour {
    public bool consoleNation = false;
    string command = "";

    private List<ConsoleCommand> commands;

    private void Start() {
        commands = new List<ConsoleCommand> {
            new ConsoleCommand {
                main = "kill",
                desc = "Kills the player",
                moment = (args) => { Player.Instance.health.Damage(100000); },
            },
            new ConsoleCommand {
                main = "give",
                desc = "Gives the specified item (#1) with count (#2) to player",
                moment = (args) => {
                    string item = args[0];
                    int count = args.Length == 1 ? 1 : Int32.Parse(args[1]);
                    Player.Instance.AddItemUnclamped(new Item(item, count));
                },
            },
            new ConsoleCommand {
                main = "sigmoidate",
                desc = "Sigmoidate",
                moment = (args) => {
                    Player.Instance.movement.AddImpulse(UnityEngine.Random.onUnitSphere * 10000);
                },
            },
            new ConsoleCommand {
                main = "set_speed",
                desc = "Sets the speed of the player to #1",
                moment = (args) => {
                    Player.Instance.movement.speed = float.Parse(args[0]);
                },
            },
            new ConsoleCommand {
                main = "set_accel",
                desc = "Sets the acceleration of the player to #1",
                moment = (args) => {
                    Player.Instance.movement.maxAcceleration = float.Parse(args[0]);
                },
            },
            new ConsoleCommand {
                main = "nearby_say",
                desc = "Makes nearby bots say something using the TTS package",
                moment = (args) => {
                    Collider[] overlap = Physics.OverlapSphere(Player.Instance.transform.position, 5);

                    foreach (Collider col in overlap) {
                        var speech = col.GetComponent<BotTextToSpeech>();
                        if (speech != null) {
                            var txt = String.Join(' ', args);
                            speech.SayString(txt);
                        }
                    }
                },
            },
        };
    }

    private void OnGUI() {
        if (!consoleNation)
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

            aaa.x = Screen.width / 2;
            GUI.Label(aaa, cmd.desc, descStyle);
        }

        Event a = Event.current;
        if (a.keyCode == KeyCode.Return) {
            UIMaster.Instance.ToggleDevConsole();
            consoleNation = false;
            Parse(command);
            command = "";
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

        try {
            cmd.moment.Invoke(splat[1..]);
        } catch (Exception) {
            Debug.LogWarning("Command invocation exception");
            throw;
        }
    }
}
