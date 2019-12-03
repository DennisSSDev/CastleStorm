using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

public class ZombieBoundarySystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;
    private Random randomizer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        randomizer = new Random(1000);
        base.OnCreate();
    }

    [BurstCompile][RequireComponentTag(typeof(ZombieTag), typeof(SpikeZoneTag))] // can't burst compile
    struct ZombieBoundaryJob : IJobForEachWithEntity<Translation>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation)
        {
            if (translation.Value.z < -170f)
            {
                CommandBuffer.DestroyEntity(jobIndex, e);
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new ZombieBoundaryJob
        {
            CommandBuffer = cmndBuffer
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);

        return job;
    }
}