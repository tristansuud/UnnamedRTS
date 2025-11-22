using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public enum TileShapeType
    {
        Flat,
        Ramp,
        OneCorner,
        ThreeCorner,
        Saddle,
        Steep
    }
    public TileCoord tileCoord;
    public Vector3 WorldSpaceCenterPosition;
    public TileType tileType;
    //public List<Unit> unitsOnTop;
    public TileShapeType shapeType;
    public Tile(TileCoord tileCoord, Vector3 worldSpaceCenterPos, TileType tileType, TileShapeType shapeType)
    {
        this.tileCoord = tileCoord;
        this.WorldSpaceCenterPosition = worldSpaceCenterPos;
        this.tileType = tileType;
        this.shapeType = shapeType;
    }
}
