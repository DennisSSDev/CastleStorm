using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class Quadrant
{
    private const int quadrantZMul = 1000;
    private const int quadrantCellSize = 20;

    private static void DebugDrawQuadrant(float3 position)
    {
        Vector3 lowerLeft = new Vector3(math.floor(position.x / quadrantCellSize) * quadrantCellSize, math.floor(position.z / quadrantCellSize) * quadrantCellSize);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(1, 0, 0) * quadrantCellSize, Color.red);
        Debug.DrawLine(lowerLeft, lowerLeft + new Vector3(0, 0, 1) * quadrantCellSize, Color.red);
        Debug.DrawLine(lowerLeft  + new Vector3(1, 0, 0)  * quadrantCellSize, lowerLeft + new Vector3(1, 0, 1) * quadrantCellSize, Color.red);
        Debug.DrawLine(lowerLeft  + new Vector3(0, 0, 1)  * quadrantCellSize, lowerLeft + new Vector3(1, 0, 1) * quadrantCellSize, Color.red);
    }
}
