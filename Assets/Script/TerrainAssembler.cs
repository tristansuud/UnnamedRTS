using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerrainAssembler : MonoBehaviour
{
    
    [Header("Terrain Assembler Parameters")]
    
    public float cellSize = 1f;
    [Header("Terrain tile Shapes data")]
    public TileMeshSet tileMeshSet;
    [Header("Material")]
    public Material material;
    [Header("Temporary")]
    public TerrainGenerator generator;


    private GameObject terrainParent;
    public enum TileType
    {
        Flat,
        Ramp,
        OneCorner,
        ThreeCorner,
        Saddle,
        Steep
    }
    public struct TileResult
    {
        public TileType Type;
        public int Rotation;   // 0, 90, 180, 270 degrees
    }
    private Mesh ResolveTileMesh(TileType t)
    {
        if (t == TileType.Flat) { return tileMeshSet.FlatTile; }
        if (t == TileType.Ramp) { return tileMeshSet.RampTile; }
        if (t == TileType.OneCorner) { return tileMeshSet.Corner1Tile; }
        if (t == TileType.ThreeCorner) { return tileMeshSet.Corner3Tile; }
        if (t == TileType.Saddle) { return tileMeshSet.SaddleTile; }
        if (t == TileType.Steep) { return tileMeshSet.SteepTile; }
        else { return tileMeshSet.FlatTile; }
    }
    
    public GameObject SpawnMesh(Mesh mesh, Material material, Vector3 position, Vector3 rotation, GameObject parent)
    {
        GameObject go = new GameObject("Tile");

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = material;

        go.transform.position = position;
        go.transform.rotation = Quaternion.Euler(rotation);
        go.transform.parent = parent.transform;

        return go;
    }
    private void Start()
    {
        GenerateTerrainInChunks(generator.GenerateHeightmap());
    }
    private const int CHUNK_SIZE = 16;

    private void GenerateTerrainInChunks(int[,] inputArray)
    {
        terrainParent = new GameObject("Terrain");
        terrainParent.transform.position = Vector3.zero;
        terrainParent.transform.rotation = Quaternion.Euler(Vector3.zero);

        int width = inputArray.GetLength(0);
        int height = inputArray.GetLength(1);

        int chunkCountX = width / CHUNK_SIZE;
        int chunkCountZ = height / CHUNK_SIZE;

        for (int cz = 0; cz < chunkCountZ; cz++)
        {
            for (int cx = 0; cx < chunkCountX; cx++)
            {
                GenerateChunk(inputArray, cx, cz);
            }
        }
    }
    private void GenerateChunk(int[,] input, int chunkX, int chunkZ)
    {
        int startX = chunkX * CHUNK_SIZE;
        int startZ = chunkZ * CHUNK_SIZE;



        List<Vector3> verts = new List<Vector3>();
        List<Vector3> norms = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();

        int triOffset = 0;

        for (int z = 0; z < CHUNK_SIZE; z++)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                int gx = startX + x;
                int gz = startZ + z;

                if (gx == input.GetLength(0) - 1) continue;
                if (gz == input.GetLength(1) - 1) continue;

                int A = input[gx, gz];
                int B = input[gx + 1, gz];
                int C = input[gx, gz + 1];
                int D = input[gx + 1, gz + 1];

                TileResult tile = ClassifyTile(A, B, C, D);
                int h = DetermineTileHeight(A, B, C, D);

                Mesh tileMesh = ResolveTileMesh(tile.Type);

                AppendTileMesh(
                    tileMesh,
                    verts, norms, uvs, tris,
                    ref triOffset,
                    new Vector3((float)x * cellSize, h, (float)z * cellSize),
                    Quaternion.Euler(0, tile.Rotation, 0)
                );
            }
        }

        // Create chunk object
        GameObject chunk = new GameObject($"Chunk_{chunkX}_{chunkZ}");
        chunk.transform.parent = terrainParent.transform;
        chunk.transform.position = new Vector3(2 * startX, 0, 2* startZ );

        MeshFilter mf = chunk.AddComponent<MeshFilter>();
        MeshRenderer mr = chunk.AddComponent<MeshRenderer>();
        MeshCollider mc = chunk.AddComponent<MeshCollider>();
        mr.sharedMaterial = material;

        Mesh combined = new Mesh();
        combined.name = $"ChunkMesh_{chunkX}_{chunkZ}";
        combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combined.SetVertices(verts);
        combined.SetNormals(norms);
        combined.SetUVs(0, uvs);
        combined.SetTriangles(tris, 0);

        mf.sharedMesh = combined;
        mc.sharedMesh = combined;
    }
    private void AppendTileMesh(
    Mesh src,
    List<Vector3> verts,
    List<Vector3> norms,
    List<Vector2> uvs,
    List<int> tris,
    ref int triOffset,
    Vector3 worldPos,
    Quaternion rot)
    {
        Vector3[] v = src.vertices;
        Vector3[] n = src.normals;
        Vector2[] uv = src.uv;
        int[] t = src.triangles;

        for (int i = 0; i < v.Length; i++)
            verts.Add(worldPos + rot * v[i]);

        for (int i = 0; i < n.Length; i++)
            norms.Add(rot * n[i]);

        for (int i = 0; i < uv.Length; i++)
            uvs.Add(uv[i]);

        for (int i = 0; i < t.Length; i++)
            tris.Add(t[i] + triOffset);

        triOffset += v.Length;
    }
    private void GenerateTerrainFromPresetTiles(int[,] inputArray)
    {
        int width = inputArray.GetLength(0);
        int height = inputArray.GetLength(1);

        for (int z = 0; z < height - 1; z++)
        {
            for (int x = 0; x < width - 1; x++)
            {
                int A = inputArray[x, z];
                int B = inputArray[x + 1, z];
                int C = inputArray[x, z + 1];
                int D = inputArray[x + 1, z + 1];
                TileResult r = ClassifyTile(A, B, C, D);
                int h = DetermineTileHeight(A, B, C, D);
                SpawnMesh(ResolveTileMesh(r.Type), material, new Vector3(x * cellSize, h, z * cellSize), new Vector3(0, r.Rotation, 0), terrainParent);
                //Instantiate(ResolveTilePrefab(r.Type), new Vector3(x * cellSize, h, z * cellSize), Quaternion.Euler(new Vector3(0, r.Rotation, 0)), terrainParent.transform);
            }
        }
    }
    public int DetermineTileHeight(int A, int B, int C, int D)
    {
        return Math.Min(Math.Min(A, B), Math.Min(C, D)); ;
    }
    public TileResult ClassifyTile(int A, int B, int C, int D)
    {
        TileResult result = new TileResult();

        // A = bottom left, B = bottm right, C = Top left, D = Top right
        int h = Math.Min(Math.Min(A, B), Math.Min(C, D));


        int a = A - h;
        int b = B - h;
        int c = C - h;
        int d = D - h;


        int max = Math.Max(Math.Max(a, b), Math.Max(c, d));
        int min = Math.Min(Math.Min(a, b), Math.Min(c, d));


        if (a == 0 && b == 0 && c == 0 && d == 0)
        {
            result.Type = TileType.Flat;
            result.Rotation = 0;

            return result;
        }


        bool steep =
            (a == 0 && d == 2) || (d == 0 && a == 2) ||
            (b == 0 && c == 2) || (c == 0 && b == 2);

        if (steep)
        {
            result.Type = TileType.Steep;


            if (a == 0 && d == 2) result.Rotation = 270;
            else if (b == 0 && c == 2) result.Rotation = 180;
            else if (d == 0 && a == 2) result.Rotation = 90;
            else if (c == 0 && b == 2) result.Rotation = 0;


            return result;
        }


        int count1 =
            (a == 1 ? 1 : 0) +
            (b == 1 ? 1 : 0) +
            (c == 1 ? 1 : 0) +
            (d == 1 ? 1 : 0);


        if (count1 == 1)
        {
            result.Type = TileType.OneCorner;

            if (a == 1) result.Rotation = 90;
            else if (b == 1) result.Rotation = 0;
            else if (d == 1) result.Rotation = 270;
            else if (c == 1) result.Rotation = 180;

            return result;
        }


        if (count1 == 3)
        {
            result.Type = TileType.ThreeCorner;

            if (a == 0) result.Rotation = 0;
            else if (b == 0) result.Rotation = 270;
            else if (d == 0) result.Rotation = 180;
            else if (c == 0) result.Rotation = 90;

            return result;
        }


        if (count1 == 2)
        {

            if (a == 1 && b == 1)
            {
                result.Type = TileType.Ramp;
                result.Rotation = 0;
                return result;
            }
            if (b == 1 && d == 1)
            {
                result.Type = TileType.Ramp;
                result.Rotation = 270;
                return result;
            }
            if (d == 1 && c == 1)
            {
                result.Type = TileType.Ramp;
                result.Rotation = 180;
                return result;
            }
            if (c == 1 && a == 1)
            {
                result.Type = TileType.Ramp;
                result.Rotation = 90;
                return result;
            }


            result.Type = TileType.Saddle;


            if (a == 0) result.Rotation = 0;
            else if (b == 0) result.Rotation = 90;
            else if (d == 0) result.Rotation = 180;
            else if (c == 0) result.Rotation = 270;

            return result;
        }


        result.Type = TileType.Flat;
        result.Rotation = 0;
        return result;
    }

    
    
}
