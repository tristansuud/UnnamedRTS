using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTerrain : MonoBehaviour
{
    Tile[,] TileMap;
    // Start is called before the first frame update
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RegisterNewMapInstance(int width, int height)
    {
        TileMap = new Tile[width, height];
    }
    public void RegisterTileInstance(Tile t)
    {
        if (TileMap == null)
        {
            Debug.LogError("Tile map uninstanstiated");
        }
        TileMap[t.tileCoord.x, t.tileCoord.y] = t;
    }
    public void LogAllTilePositions()
    {
        if (TileMap == null)
        {
            Debug.LogWarning("TileMap is null.");
            return;
        }

        int width = TileMap.GetLength(0);
        int height = TileMap.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Tile t = TileMap[x, z];
                if (t == null)
                {
                    Debug.Log($"Tile ({x},{z}) is NULL");
                    continue;
                }

                Debug.Log(
                    $"Tile ({x},{z})  |  TileCoord: {t.tileCoord}  |  WorldCenter: {t.WorldSpaceCenterPosition}"
                );
            }
        }
    }
}
