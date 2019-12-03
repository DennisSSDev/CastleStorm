using System;
using Unity.Entities;

[Serializable]
public struct CooldownComponent : IComponentData
{
    public float waitTime;
}
