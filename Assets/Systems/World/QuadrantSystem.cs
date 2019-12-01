using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

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
    private EntityQuery quadrantEntityQuery;
    
    public static NativeMultiHashMap<int, QuadrantData> QuadrantEntityHashMap;

    public static int GetPositionHashMapKey(float3 position)
    {
        return (int) (math.floor(position.x / quadrantCellSize) + (quadrantZMul * math.floor(position.z / quadrantCellSize)));
    }

    protected override void OnCreate()
    {
        QuadrantEntityHashMap = new NativeMultiHashMap<int, QuadrantData>(0, Allocator.Persistent);

        quadrantEntityQuery = GetEntityQuery(typeof(Translation), typeof(QuadrantEntityComponent));
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        QuadrantEntityHashMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile]
    struct QuadrantSystemJob : IJobForEachWithEntity<Translation, QuadrantEntityComponent>
    {
        [WriteOnly]
        public NativeMultiHashMap<int, QuadrantData>.ParallelWriter EntityHashMap;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation, [ReadOnly] ref QuadrantEntityComponent quadEntity)
        {
            int hashKey = GetPositionHashMapKey(translation.Value);
            QuadrantData data = new QuadrantData
            {
                e = e,
                position = translation.Value,
                quadEntityData = quadEntity
            };
            EntityHashMap.Add(hashKey, data);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        int zombQueryLength = quadrantEntityQuery.CalculateEntityCount();
        
        QuadrantEntityHashMap.Clear();

        if (zombQueryLength > QuadrantEntityHashMap.Capacity)
        {
            QuadrantEntityHashMap.Capacity = zombQueryLength;
        }

        var job = new QuadrantSystemJob
        {
            EntityHashMap = QuadrantEntityHashMap.AsParallelWriter()
        }.Schedule(this, inputDependencies);

        return job;
    }
}