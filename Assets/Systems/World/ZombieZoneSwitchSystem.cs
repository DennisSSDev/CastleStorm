using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class ZombieZoneSwitchSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [RequireComponentTag(typeof(ZombieTag))] // can't burst compile
    struct ZombieZoneSwitchSystemJob : IJobForEachWithEntity<Translation>
    {
        [WriteOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public float SpikeZoneThreshold;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation)
        {
            if (translation.Value.z < SpikeZoneThreshold)
            {
                //todo: maybe should remove the previous zone
                CommandBuffer.AddComponent(jobIndex, e, ComponentType.ReadOnly<SpikeZoneTag>());
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new ZombieZoneSwitchSystemJob
        {
            SpikeZoneThreshold = Board.SpikeZone,
            CommandBuffer = cmndBuffer
        }.Schedule(this, inputDependencies);

        job.Complete();

        return job;
    }
}