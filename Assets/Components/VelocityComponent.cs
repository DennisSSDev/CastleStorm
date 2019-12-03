using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct VelocityComponent : IComponentData
{
    public float3 Value;
}
