using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Flags]
public enum QuadEntityType: ushort
{
    Cavalry = 0x0001,
    Zombie = 0x0002,
    Archer = 0x0004,
    Cover = 0x0008,
    Fire = 0x0010,
    Bolder = 0x0020
}

[Serializable]
public struct QuadrantEntityComponent : IComponentData
{
    public QuadEntityType type;
}
