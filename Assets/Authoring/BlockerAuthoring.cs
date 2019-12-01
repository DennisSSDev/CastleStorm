using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class BlockerAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float Health = 1000f;
    public float Width = 10f;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<BlockerTag>(entity);
        dstManager.AddComponentData(entity, new HealthComponent{ hp = Health });
        dstManager.AddComponentData(entity, new WidthComponent{ Value = Width});
    }
}
