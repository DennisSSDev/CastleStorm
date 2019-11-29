using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct HealthComponent : IComponentData
{
    public float hp;
}
