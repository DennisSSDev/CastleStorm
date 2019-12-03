using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Random = Unity.Mathematics.Random;

public class LaunchSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;
    private Random Random;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        Random = new Random(10019320);
        base.OnCreate();
    }
    
    struct LaunchSystemJob : IJobForEachWithEntity<Translation, LauncherComponent, QuadrantEntityComponent>
    {

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public Random Randomizer;

        [ReadOnly] 
        public ComponentDataFromEntity<CooldownComponent> CooldownData;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation, [ReadOnly] ref LauncherComponent launcher, [ReadOnly] ref QuadrantEntityComponent quadData)
        {
            
            //detect if on cooldown
            if (CooldownData.Exists(e))
                return;
            // if not on cooldown, spawn a projectile in a random forward direction (velocity component assignment)
            // x and y values are allowed to be slightly random for the velocity direction
            Entity projectile = CommandBuffer.Instantiate(jobIndex, launcher.projectileEntity);
            
            CommandBuffer.SetComponent(jobIndex, projectile, new Translation{ Value = translation.Value });
            CommandBuffer.SetComponent(jobIndex, projectile, new VelocityComponent{ Value = new float3(Randomizer.NextFloat(-1.5f, 1.5f), Randomizer.NextFloat(0.25f, 0.8f), Randomizer.NextFloat(3f, 6f)) * ( quadData.type == QuadEntityType.Archer ? 15f : 23f )});
            CommandBuffer.AddComponent(jobIndex, e, new CooldownComponent{ waitTime = Randomizer.NextFloat(0.5f, 2f) });
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();
        var job = new LaunchSystemJob
        {
            CommandBuffer = cmndBuffer,
            CooldownData = GetComponentDataFromEntity<CooldownComponent>(true),
            Randomizer = Random
        }.Schedule(this, inputDependencies);

        commandBuffer.AddJobHandleForProducer(job);

        return job;
    }
}