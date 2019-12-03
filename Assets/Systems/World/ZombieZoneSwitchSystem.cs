using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

public class ZombieZoneSwitchSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;
    private Random randomizer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        randomizer = new Random(1000);
        base.OnCreate();
    }

    [RequireComponentTag(typeof(ZombieTag), typeof(CavalryZoneTag))] // can't burst compile
    struct ZombieZoneSwitchSystemJob : IJobForEachWithEntity<Translation>
    {
        [WriteOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public float SpikeZoneThreshold;

        public Random Randomizer;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation)
        {
            if (translation.Value.z > SpikeZoneThreshold)
                return;

            CommandBuffer.AddComponent(jobIndex, e, ComponentType.ReadOnly<SpikeZoneTag>());
            CommandBuffer.RemoveComponent<CavalryZoneTag>(jobIndex, e);
            if (Randomizer.NextUInt(0, 100) < 12)
            {
                CommandBuffer.AddComponent(jobIndex, e, ComponentType.ReadOnly<IsIntelligentTag>());
            }

            if (translation.Value.z < -170f)
            {
                CommandBuffer.DestroyEntity(jobIndex, e);
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new ZombieZoneSwitchSystemJob
        {
            SpikeZoneThreshold = Board.SpikeZone,
            CommandBuffer = cmndBuffer,
            Randomizer = randomizer
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);

        return job;
    }
}