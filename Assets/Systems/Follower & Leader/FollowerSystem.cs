using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class FollowerSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }
    
    [RequireComponentTag(typeof(CavalryTag))] // can't burst :'(
    struct FollowerSystemJob : IJobForEachWithEntity<Translation, FollowerComponent>
    {
        [ReadOnly]
        public ComponentDataFromEntity<LeaderComponent> LeaderPositionData;

        [WriteOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity e, int jobIndex, ref Translation translation, [ReadOnly] ref FollowerComponent follow)
        {
            if (!LeaderPositionData.Exists(follow.Leader))
            {
                // there is no longer a valid leader, switch to target search
                CommandBuffer.RemoveComponent(jobIndex, e, typeof(FollowerComponent));
                CommandBuffer.AddComponent(jobIndex, e, new NoLeaderTag());
                return;
            }

            float3 leaderPos = LeaderPositionData[follow.Leader].position;
            float3 desiredPosition = leaderPos + follow.Offset;
            float distanceToLeader = distancesq(translation.Value, desiredPosition);

            if (distanceToLeader < 1f)
            {
                // already on top don't do anything
                return;
            }

            float3 direction = normalize(desiredPosition - translation.Value);
            translation.Value += direction;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new FollowerSystemJob
        {
            CommandBuffer = cmndBuffer,
            LeaderPositionData = GetComponentDataFromEntity<LeaderComponent>(true)
        };

        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}