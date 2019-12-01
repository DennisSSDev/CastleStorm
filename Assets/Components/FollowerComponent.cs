using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct FollowerComponent : IComponentData
{
    public Entity Leader;
    public float3 Offset;
}
