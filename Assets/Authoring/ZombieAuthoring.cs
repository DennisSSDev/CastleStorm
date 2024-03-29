﻿using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ZombieAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float HP = 0f;
    public float meleeStrength = 1f;
    public float movementSpeed = 0f;
    public float3 initialDirection = new float3(0, 0,-1);

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new HealthComponent { hp = HP });
        dstManager.AddComponentData(entity, new MovementComponent { speed = movementSpeed, direction = initialDirection, defaultDirection = new float3(0, 0, -1)});
        dstManager.AddComponentData(entity, new AttackStateComponent { value = (ushort)AttackState.None });
        dstManager.AddComponentData(entity, new TargetComponent { entity = Entity.Null, targetMask = (ushort) (QuadEntityType.Cavalry | QuadEntityType.Archer) });
        dstManager.AddComponentData(entity, new MeleeStrengthComponent{ value = meleeStrength });
        dstManager.AddComponentData(entity, new QuadrantEntityComponent{type = QuadEntityType.Zombie});
        dstManager.AddComponentData(entity, new MovementStateComponent{Value = (ushort)MovementState.Moving});
        dstManager.AddComponent<ZombieTag>(entity);
        dstManager.AddComponent<NoLeaderTag>(entity);
        // by default all zombies start in the cavalry zone (could lead to issues if space changes)
        dstManager.AddComponent<CavalryZoneTag>(entity);
        dstManager.AddComponent<CanDamageTag>(entity);
    }
}
