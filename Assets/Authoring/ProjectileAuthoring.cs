using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class ProjectileAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Weight = 10f;
    public float AirResistance = 1f;
    public float DamageRadius = 1f;
    public ProjectileType type = ProjectileType.Arrow; 

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ProjectileComponent{AirResistance = AirResistance, DamageRadius = DamageRadius, Weight = Weight, type = type});
        dstManager.AddComponentData(entity, new VelocityComponent{Value = float3.zero});
        dstManager.SetComponentData(entity, new Translation{Value = new float3(0, -10, 0)});
        dstManager.AddComponent<AffectedByGravityTag>(entity);
    }
}