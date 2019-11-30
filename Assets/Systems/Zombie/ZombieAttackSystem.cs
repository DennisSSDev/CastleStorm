using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ZombieAttackSystem : JobComponentSystem
{
    public static NativeMultiHashMap<Entity, float> DamageMap;

    private EntityCommandBufferSystem commandBuffer;
    private EntityQuery cavalryUnits;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        DamageMap = new NativeMultiHashMap<Entity, float>(0, Allocator.Persistent);
        cavalryUnits = GetEntityQuery(ComponentType.ReadOnly<CavalryTag>());
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        DamageMap.Dispose();
        base.OnDestroy();
    }

    [BurstCompile] [RequireComponentTag(typeof(ZombieTag))]
    struct ZombieAttackSystemJob : IJobForEachWithEntity<Translation, MeleeStrengthComponent, ZombieAttackStateComponent, TargetComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public float MinDistance;

        [ReadOnly] 
        public ComponentDataFromEntity<DamageDealerTag> DamageDealerData;

        [ReadOnly] 
        public ComponentDataFromEntity<DamageTakerTag> DamageTakerData;

        [ReadOnly] 
        public ComponentDataFromEntity<HealthComponent> HealthData;

        [WriteOnly] 
        public NativeMultiHashMap<Entity, float>.ParallelWriter CavDamageMap;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation, [ReadOnly] ref MeleeStrengthComponent strength, ref ZombieAttackStateComponent state, ref TargetComponent target)
        {
            state.value = (ushort) ZombieAttackState.None;
            if (target.entity == Entity.Null)
                return;

            float distance = distancesq(target.location, translation.Value);

            if (distance > MinDistance)
                return;

            state.value = (ushort) ZombieAttackState.Attacking;

            if(!DamageDealerData.Exists(e))
                CommandBuffer.AddComponent(jobIndex, e, typeof(DamageDealerTag));

            if (!DamageTakerData.Exists(target.entity))
            {
                // todo: would need to detect if alive here
                if (!HealthData.Exists(target.entity))
                {
                    // entity is dead
                    target.entity = Entity.Null;
                    CommandBuffer.RemoveComponent(jobIndex, e, typeof(DamageDealerTag));
                    return;
                }
                CommandBuffer.AddComponent(jobIndex, target.entity, typeof(DamageTakerTag));
                // add entry in the map
                CavDamageMap.Add(target.entity, strength.value);
            }
            // add a new entry in the map
            CavDamageMap.Add(target.entity, strength.value);
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        int cavalryCount = cavalryUnits.CalculateEntityCount();

        DamageMap.Clear();

        if (cavalryCount > DamageMap.Capacity)
        {
            DamageMap.Capacity = cavalryCount;
        }

        var job = new ZombieAttackSystemJob
        {
            CommandBuffer = cmndBuffer,
            MinDistance = GameGlobals.zombieAttackReach,
            DamageDealerData = GetComponentDataFromEntity<DamageDealerTag>(true),
            DamageTakerData = GetComponentDataFromEntity<DamageTakerTag>(true),
            HealthData = GetComponentDataFromEntity<HealthComponent>(true),
            CavDamageMap = DamageMap.AsParallelWriter()
        }.Schedule(this, inputDependencies);
        
        commandBuffer.AddJobHandleForProducer(job);
        return job;
    }
}