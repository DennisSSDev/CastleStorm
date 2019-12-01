using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class TargetSearchSystem : JobComponentSystem
{
    [BurstCompile] [RequireComponentTag(typeof(CavalryZoneTag), typeof(NoLeaderTag))]
    struct TargetSearchJob : IJobForEach<Translation, TargetComponent, QuadrantEntityComponent>
    {
        [ReadOnly]
        public NativeMultiHashMap<int, QuadrantData> EntityHashMap;

        public void Execute([ReadOnly] ref Translation translation, ref TargetComponent target, [ReadOnly] ref QuadrantEntityComponent entityQuadInfo)
        {
            float3 entityPosition = translation.Value;
            float closestDistance = 1000f;

            int hashKey = QuadrantSystem.GetPositionHashMapKey(entityPosition);

            // if the entity is so far off the map, it doesn't even need to search for anything
            if (entityPosition.z > 80 || entityPosition.z < -200)
                return;

            if (EntityHashMap.TryGetFirstValue(hashKey, out var quadData, out var it))
            {
                do
                {
                    if (quadData.quadEntityData.type == entityQuadInfo.type)
                        continue;

                    float3 otherUnitLoc = quadData.position;

                    // only care to find a target that is in the approximate same lane
                    if (abs(otherUnitLoc.x - entityPosition.x) > 20f)
                    {
                        continue;
                    }

                    // check distance after
                    float currentDis = distancesq(entityPosition, otherUnitLoc);

                    // if the distance is closer then the minimum, it found the necessary target,
                    // use that cavalry unit as the target immediately
                    if (closestDistance > currentDis)
                    {
                        closestDistance = currentDis;
                        target.entity = quadData.e;
                        target.location = otherUnitLoc;
                    }
                } while (EntityHashMap.TryGetNextValue(out quadData, ref it));
            }

            if (closestDistance > 999f)
            {
                target.entity = Entity.Null;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new TargetSearchJob
        {
            EntityHashMap = QuadrantSystem.QuadrantEntityHashMap
        }.Schedule(this, inputDependencies);
        
        job.Complete();

        return job;
    }
}