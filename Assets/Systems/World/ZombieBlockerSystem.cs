using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class ZombieBlockerSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [BurstCompile][RequireComponentTag(typeof(SpikeZoneTag))]
    struct ZombieBlockerSystemJob : IJobForEachWithEntity<Translation, MeleeStrengthComponent, MovementStateComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public NativeMultiHashMap<int, BlockerData> BlockerMap;

        [ReadOnly]
        public ComponentDataFromEntity<BlockerDamageTag> DamageData;

        [ReadOnly]
        public ComponentDataFromEntity<HealthComponent> HealthData;

        public void Execute(Entity e, int jobIndex, [ReadOnly] ref Translation translation, [ReadOnly] ref MeleeStrengthComponent strength, ref MovementStateComponent state)
        {
            int hashKey = QuadrantSystem.GetPositionHashMapKey(translation.Value);
            state.Value = (ushort) MovementState.Moving;
            if (BlockerMap.TryGetFirstValue(hashKey, out var blockerData, out var it))
            {
                float zombieX = translation.Value.x;
                do
                {
                    float furthestRight = blockerData.position.x + blockerData.width / 2;
                    float furthestLeft = blockerData.position.x - blockerData.width / 2;

                   if (zombieX < furthestLeft || zombieX > furthestRight)
                       continue;

                   // within the bounds of the blocker
                   // check how far away are you from the blocker
                   if (abs(translation.Value.z - blockerData.position.z) > 2.5f)
                       continue;

                   if (!HealthData.Exists(blockerData.entity))
                   {
                        // entity is dead
                        return;
                   }
                   // the zombie is right in front of the blocker
                   state.Value = (ushort) MovementState.Blocked;

                   // detect if the blocker has a DamageTag. If not, add one
                   if (!DamageData.Exists(blockerData.entity))
                   {
                       CommandBuffer.AddComponent(jobIndex, blockerData.entity, new BlockerDamageTag());
                       return;
                   }

                } while(BlockerMap.TryGetNextValue(out blockerData, ref it));
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new ZombieBlockerSystemJob
        {
            CommandBuffer = cmndBuffer,
            BlockerMap = BlockerSystem.BlockerMap,
            DamageData = GetComponentDataFromEntity<BlockerDamageTag>(true),
            HealthData = GetComponentDataFromEntity<HealthComponent>(true)
        }.Schedule(this, inputDependencies);

        job.Complete();

        return job;
    }
}