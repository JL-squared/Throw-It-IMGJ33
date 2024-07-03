using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

[Serializable]
public class BotAttributeModifier {
    public BotAttribute attribute;
    public float delta;
}

[Serializable]
public enum BotAttribute {
    MovementSpeed,
    AttackSpeed,
    HeadHealth,
    BodyHealth,
    DamageResistance,
    KnockbackResistance,
}
