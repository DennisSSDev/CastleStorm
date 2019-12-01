using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public enum AttackState: ushort 
{
    Attacking = 0,
    None = 1
}

[Serializable]
public struct AttackStateComponent : IComponentData
{
    public ushort value;
}
