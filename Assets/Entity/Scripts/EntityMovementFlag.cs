using System;

[Flags]
public enum EntityMovementFlags {
    /*
    AllowedToMove,
    ApplyGravity,
    */
    None,
    AllowedToRotate,
    ApplyMovement,
    Default = ApplyMovement | AllowedToRotate,
}

public static class EntityMovementFlagsExt {
    public static void AddFlag(this ref EntityMovementFlags myFlags, EntityMovementFlags flag) {
        myFlags |= flag;
    }

    public static void RemoveFlag(this ref EntityMovementFlags myFlags, EntityMovementFlags flag) {
        myFlags &= ~flag;
    }
}