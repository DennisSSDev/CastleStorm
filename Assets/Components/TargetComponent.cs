using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TargetComponent : IComponentData
{
    public Entity entity;
    public float3 location;
    public ushort targetMask;
}
