using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BotPart {
    public BotPart(GameObject prefab) {
        this.prefab = prefab;
        spawnChance = 0;
        modifiers = new List<BotAttributeModifier>();
    }

    public GameObject prefab;
    public float spawnChance;

    public List<BotAttributeModifier> modifiers;
}
