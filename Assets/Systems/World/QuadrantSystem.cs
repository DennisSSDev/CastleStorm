using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public struct QuadrantData
{
    public Entity e;
    public float3 position;
    public QuadrantEntityComponent quadEntityData;
}

public class QuadrantSystem : JobComponentSystem
{
    private const int quadrantZMul = 1000;
    private const int quadrantCellSize = 20;
    
    private EntityQuery zombQuery;
    private EntityQuery cavQuery;
    
    public static NativeMultiHashMap<int, QuadrantData> zombieHashMap;
    public static NativeMultiHashMap<int, QuadrantData> cavalryHashMap;

    public static int GetPositionHashMapKey(float3 position)
    {
        return (int) (math.floor(position.x / quadrantCellSize) + (quadrantZMul * math.floor(position.z / quadrantCellSize)));
    }

    protected override void OnCreate()
    {
        zombieHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);
        cavalryHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);

        zombQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntityComponent), typeof(ZombieTag));
        cavQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntityComponent), typeof(CavalryTag));
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        zombieHashMap.Dispose();
        cavalryHashMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile]
    struct QuadrantSystemJob : IJobForEachWithEntity<Translation, QuadrantEntityComponent>
    {
        [WriteOnly]
        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter cavHashMap;
        [WriteOnly]
        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter zombHashMap;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation, [ReadOnly] ref QuadrantEntityComponent quadEntity)
        {
            int hashKey = GetPositionHashMapKey(translation.Value);
            QuadrantData data = new QuadrantData
            {
                e = e,
                position = translation.Value,
                quadEntityData = quadEntity
            };

            switch (quadEntity.type)
            {
                case QuadEntityType.Zombie:
                    zombHashMap.Add(hashKey, data);
                    break;
                case QuadEntityType.Cavalry:
                    cavHashMap.Add(hashKey, data);
                    break;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        int cavQueryLength = cavQuery.CalculateEntityCount();
        int zombQueryLength = zombQuery.CalculateEntityCount();
        
        zombieHashMap.Clear();
        cavalryHashMap.Clear();

        if (zombQueryLength > zombieHashMap.Capacity)
        {
            zombieHashMap.Capacity = zombQueryLength;
        }

        if (cavQueryLength > cavalryHashMap.Capacity)
        {
            cavalryHashMap.Capacity = cavQueryLength;
        }

        var job = new QuadrantSystemJob
        {
            cavHashMap = cavalryHashMap.AsParallelWriter(),
            zombHashMap = zombieHashMap.AsParallelWriter()
        }.Schedule(this, inputDependencies);

        return job;
    }
}