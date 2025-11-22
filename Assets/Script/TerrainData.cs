using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Objects/Terrain Data")]
public class TerrainData : ScriptableObject
{
    public TileTypeCollection tileTypeCollection;
    public int[,] heightmap;
    public int[,] TileTypeMap;
    public System.Action OnChanged;

    public void SetHeightmap(int[,] map)
    {
        heightmap = map;
        OnChanged?.Invoke();
    }

}
