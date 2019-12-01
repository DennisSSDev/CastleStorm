using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class CavalryAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Health = 100f;
    public float movementSpeed = 5f;
    public float meleeStrenth = 5f;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new HealthComponent {hp = Health});
        dstManager.AddComponentData(entity, new MovementComponent { direction = new float3(0f, 0f, 1f ), speed = movementSpeed, defaultDirection = new float3(0,0,1)});
        dstManager.AddComponentData(entity, new MeleeStrengthComponent{ value = meleeStrenth });
        dstManager.AddComponentData(entity, new CavalryTag());
        dstManager.AddComponentData(entity, new TargetComponent{entity = Entity.Null, targetMask = (ushort) QuadEntityType.Zombie });
        dstManager.AddComponentData(entity, new QuadrantEntityComponent{type = QuadEntityType.Cavalry});
        dstManager.AddComponentData(entity, new BehaviorStateComponent{state = Behavior.Await});
        dstManager.AddComponentData(entity, new AttackStateComponent{value = (ushort)AttackState.None});
        // by default cavalry is only in the cavalry zone
        dstManager.AddComponent<CavalryZoneTag>(entity);
        dstManager.AddComponent<CanDamageTag>(entity);
    }
}
