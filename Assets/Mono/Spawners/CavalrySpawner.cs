using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CavalrySpawner : MonoBehaviour
{
    public GameObject cavalryPrefab;

    public uint unitsPerRow = 7;

    public uint rows = 2;

    // Start is called before the first frame update
    void Start()
    {
        var prefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(cavalryPrefab, World.Active);
        var entityManager = World.Active.EntityManager;

        // each spawner should receive the cavalry squad entity
        // each entity will have an offset from that squad that is unique to that entity
        // until the squad is destroyed, all must follow
    }
}
