using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

public class DamageSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [BurstCompile][RequireComponentTag(typeof(DamageTakerTag))]
    struct DamageSystemJob : IJobForEachWithEntity<HealthComponent>
    {
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
                CommandBuffer.RemoveComponent<DamageTakerTag>(jobIndex, e);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new DamageSystemJob
        {
            CommandBuffer = cmndBuffer,
            DamageMap = AttackSystem.DamageMap
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);
        return job;
    }
}