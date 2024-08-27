using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build;
using UnityEngine;
using UnityEngine.Events;

public class ConsoleCommand {
    public string main;
    public string desc;
    
    public delegate void Action(string[] args, Player player);
    public Action moment;
}
