using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ZombieTargetSearch : JobComponentSystem
{
    private EntityQuery CavalryUnitsQuery;

    struct CavalryData
    {
        public float3 translation;
        public Entity entity;
    }

    protected override void OnCreate()
    {
        CavalryUnitsQuery = GetEntityQuery(typeof(CavalryTag), typeof(Translation));
        base.OnCreate();
    }

    [BurstCompile] [RequireComponentTag(typeof(ZombieTag), typeof(CavalryZoneTag))]
    struct ZombieTargetSearchJob : IJobForEach<Translation, TargetComponent>
    {
        [DeallocateOnJobCompletion]
        public NativeArray<CavalryData> Cavalry;

        public void Execute([ReadOnly] ref Translation translation, ref TargetComponent target)
        {
            if (Cavalry.Length == 0)
                return;

            float3 zombieLoc = translation.Value;

            Entity e = Cavalry[0].entity;
            float3 closestCavEntityLocation = Cavalry[0].translation;
            float closestDistance = 100f;

            //todo: maybe if the zombie is so far off the map, it doesn't even need to search for anything?

            for (int i = 0; i < Cavalry.Length; ++i)
            {
                float3 cavUnitLoc = Cavalry[i].translation;

                // only care to find a target that is in the right lane
                if (abs(cavUnitLoc.x - zombieLoc.x) > 10f)
                {
                    continue;
                }

                // check distance after
                float currentDis = distancesq(zombieLoc, cavUnitLoc);

                // if the distance is closer then the minimum, it found the necessary target, use that cavalry unit as the target immediately
                if (closestDistance > currentDis)
                {
                    target.isValid = true;
                    target.entity = e;
                    target.location = closestCavEntityLocation;
                    return;
                }
            }
            target.isValid = false;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        var cavLocations = CavalryUnitsQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        var cavEntities = CavalryUnitsQuery.ToEntityArray(Allocator.TempJob);

        var cavDataArray = new NativeArray<CavalryData>(cavLocations.Length, Allocator.TempJob);

        for (int i = 0; i < cavDataArray.Length; i++)
        {
            cavDataArray[i] = new CavalryData { translation = cavLocations[i].Value, entity = cavEntities[i] };
        }

        var job = new ZombieTargetSearchJob
        {
            Cavalry = cavDataArray
        }.Schedule(this, inputDependencies);
        
        cavLocations.Dispose();
        cavEntities.Dispose();
        
        return job;
    }
}