using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class MoodleDefinition {
    public string name;
    public string desc;
    public List<StatusEffect> effects = new List<StatusEffect>();
}
