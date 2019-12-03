using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ZombieSpawnerComponent : IComponentData
{
    public Entity prefab;
}
