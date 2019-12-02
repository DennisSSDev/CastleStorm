
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class BlockerDamageSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [RequireComponentTag(typeof(BlockerTag))]
    struct BlockerDamageSystemJob : IJobForEachWithEntity<BlockerDamageComponent, HealthComponent>
    {
        [WriteOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref BlockerDamageComponent blockerDamage, ref HealthComponent health)
        {
            health.hp -= 1;
            if (health.hp < 0)
            {
                // destroy entity
                CommandBuffer.DestroyEntity(jobIndex, e);
                return;
            } 
            CommandBuffer.RemoveComponent(jobIndex, e, typeof(BlockerDamageComponent));
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new BlockerDamageSystemJob
        {
            CommandBuffer = cmndBuffer
        }.Schedule(this, inputDependencies);

        return job;
    }
}