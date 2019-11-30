using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class DamageSystem : JobComponentSystem // for zombies that damage cavalry
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    
    [BurstCompile] [RequireComponentTag(typeof(DamageTakerTag), typeof(CavalryTag))]
    struct DamageSystemJob : IJobForEachWithEntity<HealthComponent>
    {
        [WriteOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly] 
        public NativeMultiHashMap<Entity, float> DamageMap;

        public void Execute(Entity e, int jobIndex, ref HealthComponent health)
        {
            float dmgAccumulation = 0f;
            if (DamageMap.TryGetFirstValue(e, out var damage, out var it))
            {
                do
                {
                    dmgAccumulation += damage;
                } while (DamageMap.TryGetNextValue(out damage, ref it));
            }
            health.hp -= dmgAccumulation;
            if (health.hp < 0f)
            {
                CommandBuffer.DestroyEntity(jobIndex, e);
            }
            else
            {
                //todo: maybe unneeded
                CommandBuffer.RemoveComponent(jobIndex, e, typeof(DamageTakerTag));
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new DamageSystemJob
        {
            CommandBuffer = cmndBuffer,
            DamageMap = ZombieAttackSystem.DamageMap
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);
        return job;
    }
}