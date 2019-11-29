using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ZombieDirectionSystem : JobComponentSystem
{
    [BurstCompile] [RequireComponentTag(typeof(ZombieTag))]
    struct ZombieDirectionSystemJob : IJobForEach<Translation, MovementComponent, TargetComponent>
    {
        public void Execute([ReadOnly] ref Translation translation, ref MovementComponent movement, [ReadOnly] ref TargetComponent target)
        {
            // todo: to make the movement more smooth you can attempt to get a cross product to get a right vector
            float3 direction = float3(0, 0, -1);
            direction = target.isValid ? normalize(target.location - translation.Value) : direction;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ZombieDirectionSystemJob();
        return job.Schedule(this, inputDependencies);
    }
}