using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class LauncherAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    // type of projectile to launch
    public GameObject projectile;
    public float health = 100f;
    public float coolDown = 1f;
    public QuadEntityType launcherType = QuadEntityType.Archer;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        uint seed = 1001 + (uint) coolDown;
        Random rand = new Random(seed);
        dstManager.AddComponentData(entity, new HealthComponent{hp = health});
        dstManager.AddComponentData(entity, new QuadrantEntityComponent{ type = launcherType });
        Entity e = GameObjectConversionUtility.ConvertGameObjectHierarchy(projectile, World.Active);
        // to be spawned by the launcher system separately
        dstManager.AddComponentData(entity, new LauncherComponent{projectileEntity = e});
        if (rand.NextInt(0, 100) < 50)
        {
            // chance that the Launcher will begin with a cooldown so that all the launchers don't fire at the same time
            dstManager.AddComponentData(entity, new CooldownComponent{waitTime = coolDown});
        }
    }
}
