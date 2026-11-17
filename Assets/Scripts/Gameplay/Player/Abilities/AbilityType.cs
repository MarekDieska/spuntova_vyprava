using System;

[Flags]
public enum AbilityType
{
    None = 0,
    DoubleJump = 1 << 0,
    Dash = 1 << 1,
    GrapplingHook = 1 << 2,
    Glide = 1 << 3
}