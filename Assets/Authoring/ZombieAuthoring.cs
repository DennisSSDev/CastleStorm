using Unity.Entities;
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
        dstManager.AddComponentData(entity, new MovementComponent { speed = movementSpeed, direction = initialDirection });
        dstManager.AddComponentData(entity, new ZombieAttackStateComponent { value = (ushort)ZombieAttackState.None });
        dstManager.AddComponentData(entity, new TargetComponent { isValid = false });
        dstManager.AddComponentData(entity, new MeleeStrengthComponent{ value = meleeStrength });
        dstManager.AddComponent<ZombieTag>(entity);
        // by default all zombies start in the cavalry zone (could lead to issues if space changes)
        dstManager.AddComponent<CavalryZoneTag>(entity);
    }
}
