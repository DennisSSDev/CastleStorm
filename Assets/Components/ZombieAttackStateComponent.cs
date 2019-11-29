using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public enum ZombieAttackState: ushort {
    Attacking = 0,
    None = 1
}

[Serializable]
public struct ZombieAttackStateComponent : IComponentData
{
    public ushort value;
}
