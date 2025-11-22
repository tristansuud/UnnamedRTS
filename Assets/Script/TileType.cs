using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Terrain Objects/TileType")]
public class TileType : ScriptableObject
{
    public int AtlasIndex;
    public bool isBuildable;
    public bool isGround;
    public bool isWater;
}
