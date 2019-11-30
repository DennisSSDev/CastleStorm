using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ZombieTargetSearch : JobComponentSystem
{

    [BurstCompile] [RequireComponentTag(typeof(ZombieTag), typeof(CavalryZoneTag))]
    struct ZombieTargetSearchJob : IJobForEach<Translation, TargetComponent>
    {
        [ReadOnly]
        public NativeMultiHashMap<int, QuadrantData> cavalryHashMap;

        public void Execute([ReadOnly] ref Translation translation, ref TargetComponent target)
        {
            float3 zombieLoc = translation.Value;
            float closestDistance = 1000f;

            int hashKey = QuadrantSystem.GetPositionHashMapKey(zombieLoc);

            // if the zombie is so far off the map, it doesn't even need to search for anything
            if (zombieLoc.z > 80)
                return;

            if (cavalryHashMap.TryGetFirstValue(hashKey, out var quadData, out var it))
            {
                do
                {
                    float3 cavUnitLoc = quadData.position;

                    // only care to find a target that is in the approximate same lane
                    if (abs(cavUnitLoc.x - zombieLoc.x) > 15f)
                    {
                        continue;
                    }

                    // check distance after
                    float currentDis = distancesq(zombieLoc, cavUnitLoc);

                    // if the distance is closer then the minimum, it found the necessary target,
                    // use that cavalry unit as the target immediately
                    if (closestDistance > currentDis)
                    {
                        closestDistance = currentDis;
                        target.entity = quadData.e;
                        target.location = cavUnitLoc;
                    }
                } while (cavalryHashMap.TryGetNextValue(out quadData, ref it));
            }

            if (closestDistance > 999f)
            {
                target.entity = Entity.Null;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new ZombieTargetSearchJob
        {
            cavalryHashMap = QuadrantSystem.cavalryHashMap
        }.Schedule(this, inputDependencies);

        return job;
    }
}