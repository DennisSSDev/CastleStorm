using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class CooldownSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [BurstCompile]
    struct CooldownSystemJob : IJobForEachWithEntity<CooldownComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public float deltaTime;

        public void Execute(Entity e, int jobIndex, ref CooldownComponent cooldown)
        {
            cooldown.waitTime -= 0.5f * deltaTime;
            if (cooldown.waitTime < 0f)
            {
                CommandBuffer.RemoveComponent<CooldownComponent>(jobIndex, e);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new CooldownSystemJob
        {
            deltaTime = Time.DeltaTime,
            CommandBuffer = cmndBuffer
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);

        return job;
    }
}