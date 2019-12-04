
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class BlockerDamageSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [BurstCompile][RequireComponentTag(typeof(BlockerTag))]
    struct BlockerDamageSystemJob : IJobForEachWithEntity<BlockerDamageTag, HealthComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref BlockerDamageTag blockerDamage, ref HealthComponent health)
        {
            health.hp -= 1;
            if (health.hp < 0)
            {
                // destroy entity
                CommandBuffer.DestroyEntity(jobIndex, e);
                return;
            }
            CommandBuffer.RemoveComponent<BlockerDamageTag>(jobIndex, e);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new BlockerDamageSystemJob
        {
            CommandBuffer = cmndBuffer
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);

        return job;
    }
}