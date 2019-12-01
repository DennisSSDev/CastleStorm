using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class CavalrySpawner : MonoBehaviour
{
    public GameObject cavalryPrefab;

    public GameObject sqaudLeaderPrefab;

    public uint unitsPerRow = 7;

    public uint rows = 2;

    // Start is called before the first frame update
    void Start()
    {
        var cavArchetype = GameObjectConversionUtility.ConvertGameObjectHierarchy(cavalryPrefab, World.Active);
        var leaderArchetype = GameObjectConversionUtility.ConvertGameObjectHierarchy(sqaudLeaderPrefab, World.Active);
        var entityManager = World.Active.EntityManager;

        var leaderInstance = entityManager.Instantiate(leaderArchetype);
        
        entityManager.SetComponentData(leaderInstance, new Translation{ Value = new float3(transform.position.x, 1f, transform.position.z)});
        entityManager.SetComponentData(leaderInstance, new LeaderComponent{ position = new float3(transform.position.x, 1f, transform.position.z)});
        float columnOffset = 0f;
        for (int i = 0; i < rows; i++)
        {
            float rowOffset = -1 * columnOffset;
            for (int j = 0; j < unitsPerRow; j++)
            {
                var cavInstance = entityManager.Instantiate(cavArchetype);
                // each unit should receive the cavalry squad entity
                // each entity will have an offset from that squad that is unique to that entity
                // until the squad is destroyed, all must follow it
                entityManager.AddComponentData(cavInstance, new FollowerComponent
                {
                    Leader = leaderInstance, 
                    Offset = new float3(rowOffset, 1f, columnOffset)
                });

                rowOffset += 5f;
            }
            columnOffset -= 5f;
        }
    }
}
