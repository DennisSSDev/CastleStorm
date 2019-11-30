using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum QuadEntityType: ushort
{
    Cavalry,
    Zombie,
    Archer,
    Cover,
    Fire,
    Bolder
}

[Serializable]
public struct QuadrantEntityComponent : IComponentData
{
    public QuadEntityType type;
}
