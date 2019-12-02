using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class AvoidanceSystem : JobComponentSystem
{
    [BurstCompile] [RequireComponentTag(typeof(IsIntelligentTag))]
    struct AvoidanceSystemJob : IJobForEach<Translation, TargetComponent, MovementComponent>
    {
        [ReadOnly]
        public NativeMultiHashMap<int, BlockerData> BlockerMap;

        // todo: maybe intelligent ones should avoid not only the blockers but also each other?
        public void Execute([ReadOnly] ref Translation translation, [ReadOnly] ref TargetComponent target, ref MovementComponent movement)
        {
            int hashKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);
            float3 dir = new float3(0, 0, -1);
            float closestDis = 1000f;
            float3 closestPos = float3(10000f, 1000f, 1000f);
            bool bFoundPotentialThreat = false;
            if (BlockerMap.TryGetFirstValue(hashKey, out var data, out var it))
            {
                float zombieX = translation.Value.x;
                do
                {
                    // todo: maybe you should first find the closest target first and then determine if it's worth avoiding
                    // are you close enough to care?
                    float furthestRight = data.position.x + data.width / 2 + 2f; 
                    float furthestLeft = data.position.x - data.width / 2 - 2f;

                    if (zombieX < furthestLeft || zombieX > furthestRight)
                        continue;

                    float disSq = distancesq(translation.Value, data.position);
                    if (disSq > closestDis)
                        continue;

                    closestDis = disSq;
                    closestPos = data.position;
                    bFoundPotentialThreat = true;

                } while (BlockerMap.TryGetNextValue(out data, ref it));
            }
            if (!bFoundPotentialThreat)
            {
                movement.defaultDirection = dir;
                // todo: cause an irrational movement
                return;
            }
            if (closestDis > 150f)
            {
                movement.defaultDirection = dir;
                return;
            }

            // yes avoid
            float3 vFromAiToBlocker = normalize(closestPos - translation.Value);
            float dotProd = dot(dir, vFromAiToBlocker);
            // is it behind?
            if (dotProd < 0f)
            {
                movement.defaultDirection = dir;
                return;
            }

            // detect if to the left or to the right
            float3 right = new float3(1, 0, 0);
            float side = dot(right, vFromAiToBlocker);
            if (side > 0f)
            {
                // to the right, go left
                movement.defaultDirection.x -= ((movement.direction.x - 0.1) < -1) ? 0 : 0.1f;
                movement.defaultDirection.z += ((movement.direction.z + 0.1) > 0) ? 0 : 0.1f;
                return;
            }
            // to the left, go right
            movement.defaultDirection.x += ((movement.direction.x + 0.1) > 1) ? 0 : 0.1f;
            movement.defaultDirection.z += ((movement.direction.z + 0.1) > 0) ? 0 : 0.1f;

        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new AvoidanceSystemJob
        {
            BlockerMap = BlockerSystem.BlockerMap
        }.Schedule(this, inputDependencies);

        job.Complete();

        return job;
    }
}