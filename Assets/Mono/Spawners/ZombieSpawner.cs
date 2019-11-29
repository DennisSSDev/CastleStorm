using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public uint initialZombieSpawnCount = 1000;

    private List<int2> usedPositions = new List<int2>(1000);

    private int2 minPos = new int2(-100, 65);
    private int2 maxPos = new int2(100, 100);

    // Start is called before the first frame update
    void Start()
    {
        Unity.Mathematics.Random rand = new Unity.Mathematics.Random(42);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(zombiePrefab, World.Active);
        var entityManager = World.Active.EntityManager;
        uint count = 0;
        while (count++ < initialZombieSpawnCount)
        {
            var instance = entityManager.Instantiate(prefab);
            var space = rand.NextInt2(minPos, maxPos);
            space.x += rand.NextInt(-2, 2);
            space.y += rand.NextInt(-2, 2);

            while (usedPositions.Contains(space))
            {
                space = rand.NextInt2(minPos, maxPos);
            }

            usedPositions.Add(space);
            
            var position = transform.TransformPoint(new float3(space.x, 1, space.y));
            entityManager.SetComponentData(instance, new Translation { Value = position });

            var movementData = entityManager.GetComponentData<MovementComponent>(instance);
            // todo: the ones that can maneuver should be faster
            // todo: leaders -> force outer zombies to stick around them
            movementData.speed += rand.NextFloat(-6f, 1f);
            entityManager.SetComponentData(instance, movementData);
        }
    }
}
