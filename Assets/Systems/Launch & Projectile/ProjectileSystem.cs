using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class ProjectileSystem : JobComponentSystem
{
    private EntityCommandBufferSystem commandBuffer;

    protected override void OnCreate()
    {
        commandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    [BurstCompile] [RequireComponentTag(typeof(AffectedByGravityTag))]
    struct ProjectileSystemJob : IJobForEachWithEntity<Translation, VelocityComponent, ProjectileComponent>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public NativeMultiHashMap<int, QuadrantData> QuadMap;

        [ReadOnly]
        public float deltaTime;
        
        public void Execute(Entity e, int jobIndex, ref Translation translation, ref VelocityComponent velocity, [ReadOnly] ref ProjectileComponent projectile)
        {
            if (translation.Value.y < 0.15f)
            {
                // find all the entities in the nearby quad and destroy it if within the radius
                int key = QuadrantSystem.GetPositionHashMapKey(translation.Value);

                if (QuadMap.TryGetFirstValue(key, out var data, out var it))
                {
                    do
                    {
                        float distance = distancesq(data.position, translation.Value);
                        if (data.quadEntityData.type == QuadEntityType.Cavalry) 
                            continue;
                        if (distance < projectile.DamageRadius)
                        {
                            CommandBuffer.DestroyEntity(jobIndex, data.e);
                        }
                    } while (QuadMap.TryGetNextValue(out data, ref it));
                }
                CommandBuffer.DestroyEntity(jobIndex, e);
            } 
            // apply gravity
            velocity.Value.y += -0.0098f * projectile.Weight;
            velocity.Value.y = clamp(velocity.Value.y, -10f, 100f);
            // apply air resistance
            velocity.Value.z += projectile.AirResistance * -0.0001f;
            velocity.Value.z = clamp(velocity.Value.z, 0.5f, 100f);

            translation.Value += velocity.Value * deltaTime;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cmndBuffer = commandBuffer.CreateCommandBuffer().ToConcurrent();

        var job = new ProjectileSystemJob
        {
            deltaTime = Time.DeltaTime,
            CommandBuffer = cmndBuffer,
            QuadMap = QuadrantSystem.QuadrantEntityHashMap
        }.Schedule(this, inputDependencies);
        
        commandBuffer.AddJobHandleForProducer(job);

        job.Complete();

        return job;
    }
}