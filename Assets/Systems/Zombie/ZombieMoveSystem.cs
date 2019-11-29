using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


public class ZombieMoveSystem : JobComponentSystem
{
    [BurstCompile, RequireComponentTag(typeof(ZombieTag))]
    struct ZombieMoveSystemJob : IJobForEachWithEntity<Translation, MovementComponent, ZombieAttackStateComponent>
    {
        public float deltaTime;
        
        public void Execute(Entity entity, int index, ref Translation translation, [ReadOnly] ref MovementComponent movement, [ReadOnly] ref ZombieAttackStateComponent zombState)
        {
            translation.Value += movement.direction * movement.speed * deltaTime * zombState.value;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ZombieMoveSystemJob
        {
            deltaTime = UnityEngine.Time.deltaTime
        };
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}