using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class LeaderSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [BurstCompile][RequireComponentTag(typeof(SquadLeaderTag))]
    struct LeaderSystemJob : IJobForEachWithEntity<Translation, TargetComponent, LeaderComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation, [ReadOnly] ref TargetComponent target, ref LeaderComponent leader)
        {
            Entity entity = target.entity;
            leader.position = translation.Value;
            if (entity == Entity.Null)
                return;

            float distanceToTarget = distancesq(translation.Value, target.location);
            if (distanceToTarget < 1000f)
            {
                CommandBuffer.DestroyEntity(jobIndex, e);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new LeaderSystemJob
        {
            CommandBuffer = cmndBuffer
        }.Schedule(this, inputDependencies);

        job.Complete();

        return job;
    }
}