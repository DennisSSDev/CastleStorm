using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MovementComponent : IComponentData
{
    public float3 direction;
    public float speed;
}
