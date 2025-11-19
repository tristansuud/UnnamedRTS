using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Objects/Tile Mesh Set")]
public class TileMeshSet : ScriptableObject
{
    public Mesh FlatTile;
    public Mesh RampTile;
    public Mesh Corner1Tile;
    public Mesh Corner3Tile;
    public Mesh SaddleTile;
    public Mesh SteepTile;
}
