using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BotWorldPart : MonoBehaviour {
    public BotBase botBase;
    public virtual void AttributesUpdated() { }
}
