using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct FollowerComponent : IComponentData
{
    public Entity Leader;
    public float3 Offset;
}
