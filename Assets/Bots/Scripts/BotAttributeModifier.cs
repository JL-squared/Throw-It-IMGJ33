using System;

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
