using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct MeleeStrengthComponent : IComponentData
{
    public float value;
}
