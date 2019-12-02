using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public class MoveSystem : JobComponentSystem
{
    [BurstCompile, RequireComponentTag(typeof(NoLeaderTag))]
    struct MoveSystemJob : IJobForEach<Translation, MovementComponent, AttackStateComponent, MovementStateComponent>
    {
        public float deltaTime;
        
        public void Execute(ref Translation translation, [ReadOnly] ref MovementComponent movement, [ReadOnly] ref AttackStateComponent state, [ReadOnly] ref MovementStateComponent moveState)
        {
            translation.Value += movement.direction * movement.speed * deltaTime * state.value * moveState.Value;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MoveSystemJob
        {
            deltaTime = UnityEngine.Time.deltaTime
        };
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}