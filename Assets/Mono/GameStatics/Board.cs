using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

/**
 * Look up for where an entity is in the world of the game board
 */
public static class Board
{
    // maximum z value
    public static float CavalryZone = 0f;
    public static float SpikeZone = -100f;
    public static float TrenchZone = -170f;
    public static float CastleZone = -270f;

    public static float[] zones = new[] {CavalryZone, SpikeZone, TrenchZone, CastleZone};
}
