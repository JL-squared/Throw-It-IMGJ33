using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BotPart {
    public GameObject prefab;
    public float spawnChance;

    public List<BotAttributeModifier> modifiers;
}
