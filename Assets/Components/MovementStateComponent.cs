using System;
using Unity.Entities;

public enum MovementState
{
    Blocked = 0,
    Moving = 1,
}

[Serializable]
public struct MovementStateComponent : IComponentData
{
    public ushort Value;
}
