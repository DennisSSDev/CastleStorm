using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class DamageSystem : JobComponentSystem
{
    private EntityQuery DamageDealerQuery;
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        //todo: this might need to be moved, or else it'll never work
        DamageDealerQuery = GetEntityQuery(
            ComponentType.ReadOnly<DamageDealerTag>(), 
            ComponentType.ReadOnly<MeleeStrengthComponent>(), 
            ComponentType.ReadOnly<Translation>()
            );
        base.OnCreate();
    }

    struct DamageDealerData
    {
        public Entity e;
        public float3 position;
        public float strength;
    }

    [BurstCompile] [RequireComponentTag(typeof(DamageTakerTag))]
    struct DamageSystemJob : IJobForEach<HealthComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [DeallocateOnJobCompletion]
        public NativeArray<DamageDealerData> DamageDealers;

        public void Execute(ref HealthComponent health)
        {
            health.hp -= 1;
            //todo: loop through all the damage dealer locations and detect the ones that are closest
            //todo: combine all the ones that are close enough into a variable that will deduct an x amount from the health stat
            //todo: if the health value is below 0 -> ask the command buffer to remove the entity
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var locations = DamageDealerQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        var strengths = DamageDealerQuery.ToComponentDataArray<MeleeStrengthComponent>(Allocator.TempJob);
        var entities = DamageDealerQuery.ToEntityArray(Allocator.TempJob);


        var damageDealerData = new NativeArray<DamageDealerData>(entities.Length, Allocator.TempJob);

        for (int i = 0; i < entities.Length; i++)
        {
            damageDealerData[i] = new DamageDealerData { position = locations[i].Value, e = entities[i], strength = strengths[i].value };
        }

        var job = new DamageSystemJob
        {
            CommandBuffer = cmndBuffer,
            DamageDealers = damageDealerData
        }.Schedule(this, inputDependencies);

        locations.Dispose();
        strengths.Dispose();
        entities.Dispose();

        commandBuffer.AddJobHandleForProducer(job);
        return job;
    }
}