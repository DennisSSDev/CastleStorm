using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public enum ProjectileType: ushort
{
    Arrow,
    Bolder
}

[Serializable]
public struct ProjectileComponent : IComponentData
{
    public float Weight;
    public float AirResistance;
    public float DamageRadius;
    public ProjectileType type;
}
