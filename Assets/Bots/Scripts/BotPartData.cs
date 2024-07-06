using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BotPartData: RngItem {
    public BotPartData(GameObject prefab) {
        this.prefab = prefab;
        weight = 0;
        modifiers = new List<BotAttributeModifier>();
    }

    public GameObject prefab;
    public List<string> tags;
    public List<BotAttributeModifier> modifiers;
}
