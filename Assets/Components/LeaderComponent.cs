﻿using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct LeaderComponent : IComponentData
{
    public float3 position;
}
