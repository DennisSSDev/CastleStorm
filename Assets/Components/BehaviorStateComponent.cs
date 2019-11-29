using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;


public enum Behavior
{
    // within a zone find an open slot for shooting at an enemy.
    Position,
    // fall back to the next zone
    Retreat,
    // stay at position and shoot at enemy
    Shoot,
    // charge at the enemy
    Attack
}

[Serializable]
public struct BehaviorStateComponent : IComponentData
{
    public Behavior state;
}
