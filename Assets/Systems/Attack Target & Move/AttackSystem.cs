using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class AttackSystem : JobComponentSystem
{
    public static NativeMultiHashMap<Entity, float> DamageMap;

    private EntityCommandBufferSystem commandBuffer;
    private EntityQuery damageableUnits;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        DamageMap = new NativeMultiHashMap<Entity, float>(0, Allocator.Persistent);
        damageableUnits = GetEntityQuery(ComponentType.ReadOnly<HealthComponent>());
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        DamageMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile][RequireComponentTag(typeof(CanDamageTag), typeof(NoLeaderTag))]
    struct ZombieAttackSystemJob : IJobForEachWithEntity<Translation, MeleeStrengthComponent, AttackStateComponent, TargetComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public float MinDistance;

        [ReadOnly]
        public ComponentDataFromEntity<DamageTakerTag> DamageTakerData;

        [ReadOnly]
        public ComponentDataFromEntity<HealthComponent> HealthData;

        [WriteOnly]
        public NativeMultiHashMap<Entity, float>.ParallelWriter EntityDamageMap;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation, [ReadOnly] ref MeleeStrengthComponent strength, ref AttackStateComponent state, ref TargetComponent target)
        {
            state.value = (ushort) AttackState.None;
            if (target.entity == Entity.Null)
                return;

            float distance = distancesq(target.location, translation.Value);
            if (distance > MinDistance)
                return;

            state.value = (ushort) AttackState.Attacking;

            if (!DamageTakerData.Exists(target.entity))
            {
                if (!HealthData.Exists(target.entity))
                {
                    // entity is dead
                    target.entity = Entity.Null;
                    return;
                }
                CommandBuffer.AddComponent<DamageTakerTag>(jobIndex, target.entity);
                // add entry in the map
                EntityDamageMap.Add(target.entity, strength.value);
            }
            // add a new entry in the map
            EntityDamageMap.Add(target.entity, strength.value);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        int unitCount = damageableUnits.CalculateEntityCount();

        DamageMap.Clear();

        if (unitCount > DamageMap.Capacity)
        {
            DamageMap.Capacity = unitCount;
        }

        var job = new ZombieAttackSystemJob
        {
            CommandBuffer = cmndBuffer,
            MinDistance = GameGlobals.zombieAttackReach,
            DamageTakerData = GetComponentDataFromEntity<DamageTakerTag>(true),
            HealthData = GetComponentDataFromEntity<HealthComponent>(true),
            EntityDamageMap = DamageMap.AsParallelWriter()
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);
        return job;
    }
}