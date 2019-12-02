using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public struct BlockerData
{
    public Entity entity;
    public float3 position;
    public float width;
}

public class BlockerSystem : JobComponentSystem
{
    public static NativeMultiHashMap<int, BlockerData> BlockerMap;

    private EntityQuery query;

    protected override void OnCreate()
    {
        query = GetEntityQuery(typeof(BlockerTag));
        BlockerMap = new NativeMultiHashMap<int, BlockerData>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        BlockerMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile] [RequireComponentTag(typeof(BlockerTag))]
    struct ZombieBlockerSystemJob : IJobForEachWithEntity<Translation, WidthComponent>
    {
        public NativeMultiHashMap<int, BlockerData>.ParallelWriter BlockMap;

        public void Execute(Entity e, int jobIndex, ref Translation translation, ref WidthComponent width)
        {
            int hashKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);
            BlockMap.Add(hashKey, new BlockerData
            {
                entity = e, 
                position = translation.Value,
                width = width.Value
            });
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        int entityCount = query.CalculateEntityCount();
        BlockerMap.Clear();
        if (entityCount > BlockerMap.Capacity)
        {
            BlockerMap.Capacity = entityCount;
        }
        var job = new ZombieBlockerSystemJob
        {
            BlockMap = BlockerMap.AsParallelWriter()
        }.Schedule(this, inputDependencies);

        return job;
    }
}
