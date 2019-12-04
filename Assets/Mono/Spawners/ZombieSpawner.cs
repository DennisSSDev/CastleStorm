using System.Collections;
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

    private int2 minPos = new int2(-100, 175);
    private int2 maxPos = new int2(100, 230);

    private uint spawnedCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        spawnedCount = initialZombieSpawnCount;
        Unity.Mathematics.Random rand = new Unity.Mathematics.Random(42);
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(zombiePrefab, World.Active);
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
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

        EntityArchetype arch = entityManager.CreateArchetype(typeof(ZombieSpawnerComponent));
        Entity e = entityManager.CreateEntity(arch);
        entityManager.SetComponentData(e, new ZombieSpawnerComponent{prefab = prefab});
        entityManager.Instantiate(e);

        // StartCoroutine(SpawnZombieBatch());
    }

    IEnumerator SpawnZombieBatch()
    {
        while (spawnedCount < 5000)
        {
            yield return new WaitForSeconds(4);
            spawnedCount += 80;
            Unity.Mathematics.Random rand = new Unity.Mathematics.Random(42);
            var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(zombiePrefab, World.DefaultGameObjectInjectionWorld);
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            uint count = 0;
            while (count++ < 100)
            {
                var instance = entityManager.Instantiate(prefab);
                var space = rand.NextInt2(minPos, maxPos);
                space.x += rand.NextInt(-2, 2);
                space.y += rand.NextInt(-2, 2);
                var position = transform.TransformPoint(new float3(space.x, 1, space.y));
                entityManager.SetComponentData(instance, new Translation { Value = position });
                var movementData = entityManager.GetComponentData<MovementComponent>(instance);
                movementData.speed += rand.NextFloat(-4f, 0f);
                entityManager.SetComponentData(instance, movementData);
            }
        }
    }
}
