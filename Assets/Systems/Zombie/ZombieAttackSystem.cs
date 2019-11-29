using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ZombieAttackSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;
    private float minAttackDistance = 10f;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [BurstCompile] [RequireComponentTag(typeof(ZombieTag))]
    struct ZombieAttackSystemJob : IJobForEachWithEntity<Translation, ZombieAttackStateComponent, TargetComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public float MinDistance;

        [ReadOnly] 
        public ComponentDataFromEntity<DamageDealerTag> DamageDealerData;

        [ReadOnly] 
        public ComponentDataFromEntity<DamageTakerTag> DamageTakerData;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation, ref ZombieAttackStateComponent state, ref TargetComponent target)
        {
            state.value = (ushort) ZombieAttackState.None;
            if (!target.isValid)
                return;

            float distance = distancesq(target.location, translation.Value);

            if (distance > MinDistance)
                return;

            state.value = (ushort) ZombieAttackState.Attacking;

            if(!DamageDealerData.Exists(e))
                CommandBuffer.AddComponent(jobIndex, e, typeof(DamageDealerTag));

            if(!DamageTakerData.Exists(target.entity))
                CommandBuffer.AddComponent(jobIndex, target.entity, typeof(DamageTakerTag));
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new ZombieAttackSystemJob
        {
            CommandBuffer = cmndBuffer,
            MinDistance = minAttackDistance,
            DamageDealerData = GetComponentDataFromEntity<DamageDealerTag>(true),
            DamageTakerData = GetComponentDataFromEntity<DamageTakerTag>(true)
        }.Schedule(this, inputDependencies);
        
        commandBuffer.AddJobHandleForProducer(job);
        return job;
    }
}