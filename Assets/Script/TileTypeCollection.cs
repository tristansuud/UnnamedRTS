using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Terrain Objects/Tile Type Collection")]
public class TileTypeCollection : ScriptableObject
{
    public List<TileType> TileTypes = new List<TileType>();
}
