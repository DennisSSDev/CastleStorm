using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ZombieContinuousSpawnSystem : JobComponentSystem
{

    private uint spawnCount = 0;
    private Random rand;
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        rand = new Random(42);
        commandBuffer = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }



    [ExcludeComponent(typeof(CooldownComponent))]
    struct ZombieContinuousSpawnSystemJob : IJobForEachWithEntity<ZombieSpawnerComponent>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;

        public uint SpawnCount;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public Random Rand;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref ZombieSpawnerComponent zombieSpawner)
        {
            int2 minPos = new int2(-100, 175);
            int2 maxPos = new int2(100, 230);
            SpawnCount += 80;
            if (SpawnCount > 1000)
            {
                CommandBuffer.RemoveComponent(jobIndex, e, typeof(ZombieSpawnerComponent));
            }

            var prefab = zombieSpawner.prefab;
            uint count = 0;
            while (count++ < 80)
            {
                var instance = CommandBuffer.Instantiate(jobIndex, prefab);
                var space = Rand.NextInt2(minPos, maxPos);
                space.x += Rand.NextInt(-2, 2);
                space.y += Rand.NextInt(-2, 2);
                var position = new float3(space.x, 1, space.y);
                CommandBuffer.SetComponent(jobIndex, instance, new Translation { Value = position });
                CommandBuffer.SetComponent(jobIndex, instance, new MovementComponent { speed = 10 + Rand.NextFloat(-4f, 0f), direction =  new float3(0, 0, -1), defaultDirection = new float3(0, 0, -1) });
            }
            CommandBuffer.AddComponent(jobIndex, e, new CooldownComponent{waitTime = 5f});
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new ZombieContinuousSpawnSystemJob
        {
            SpawnCount = spawnCount,
            CommandBuffer = cmndBuffer,
            Rand = rand
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);

        return job;
    }
}