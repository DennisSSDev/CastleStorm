using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class CavalryLeaderAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    { 
        dstManager.AddComponent<SquadLeaderTag>(entity);
        dstManager.AddComponent<NoLeaderTag>(entity);
        dstManager.AddComponent<CavalryZoneTag>(entity);
        dstManager.AddComponentData(entity, new QuadrantEntityComponent{type = QuadEntityType.Cavalry});
        dstManager.AddComponentData(entity, new MovementComponent {defaultDirection = new float3(0,0,1), direction = new float3(0,0,1), speed = 5f} );
        dstManager.AddComponentData(entity, new TargetComponent { entity = Entity.Null, targetMask = (ushort) QuadEntityType.Zombie } );
        dstManager.AddComponentData(entity, new AttackStateComponent{value = (ushort)AttackState.None});
        dstManager.AddComponentData(entity, new MovementStateComponent{Value = (ushort)MovementState.Moving});
        dstManager.AddComponentData(entity, new LeaderComponent() );
    }
}
