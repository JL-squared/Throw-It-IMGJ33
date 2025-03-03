using System;
using UnityEngine;

[Serializable]
public class StatusEffect {
    public string id;
    public string description;

    // Not sure how to do status effects system yet, dummy implementation
    public void Affect() {
        throw new NotImplementedException();
    }
}
