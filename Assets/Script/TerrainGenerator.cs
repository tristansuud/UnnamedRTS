using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator : MonoBehaviour
{
    [Header("Terrain Generator Parameters")]
    public int width; // N (horizontal)
    public int height; // M (vertical)
    public int octaves;
    public int seed;
    public float PerlinScale;
    
    public int MaxHeight;
    public int maxStep;
    [Header("Terrain Data")]
    public TileTypeCollection tileTypeCollection;
    public TerrainData terrainData;
    private void Start()
    {
        if (terrainData != null) {
            terrainData.SetHeightmap(GenerateHeightmap());
            terrainData.tileTypeCollection = tileTypeCollection;
        } 
    }
    public void EnforceChunkMultiple(ref int width, ref int height)
    {
        if (width % 16 != 0)
        {
            int corrected = width - (width % 16);
            Debug.LogWarning($"Width {width} is not a multiple of 16. Corrected to {corrected}.");
            width = corrected;
        }
        if (width == 0)
        {
            width = 16;
        }
        if (height % 16 != 0)
        {
            int corrected = height - (height % 16);
            Debug.LogWarning($"Height {height} is not a multiple of 16. Corrected to {corrected}.");
            height = corrected;
        }
        if (height == 0)
        {
            height = 16;
        }
    }
    public int[,] GenerateHeightmap()
    {
        EnforceChunkMultiple(ref width, ref height);
        int[,] inputArray = GenerateQuantizedPerlin(width, height, octaves, seed, PerlinScale, MaxHeight);
        inputArray = EnforceMaxStep(inputArray, maxStep);
        return inputArray;
    }
    public int[,] GenerateQuantizedPerlin(
    int width,
    int height,
    int octaves,
    int seed,
    float scale,
    int maxHeight
)
    {
        int[,] result = new int[width, height];

        System.Random rng = new System.Random(seed);
        float offsetX = rng.Next(-100000, 100000);
        float offsetY = rng.Next(-100000, 100000);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseValue = 0f;
                float totalAmplitude = 0f;

                for (int o = 0; o < octaves; o++)
                {
                    float sampleX = (x + offsetX) * 0.01f * frequency / scale;
                    float sampleY = (y + offsetY) * 0.01f * frequency / scale;

                    float n = Mathf.PerlinNoise(sampleX, sampleY);

                    noiseValue += n * amplitude;
                    totalAmplitude += amplitude;

                    amplitude *= 0.5f;
                    frequency *= 2f;
                }

                noiseValue /= totalAmplitude;

                int quantized = Mathf.RoundToInt(noiseValue * maxHeight);
                quantized = Mathf.Clamp(quantized, 0, maxHeight);
                //Debug.Log("Quantized: "+ quantized + " from " + noiseValue * maxHeight);
                result[x, y] = quantized;
            }
        }

        return result;
    }
    public int[,] EnforceMaxStep(int[,] heights, int maxStep)
    {
        int w = heights.GetLength(0);
        int h = heights.GetLength(1);

        bool changed = true;

        while (changed)
        {
            changed = false;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    int center = heights[x, y];

                    // Check four directions
                    TryClamp(x + 1, y);
                    TryClamp(x - 1, y);
                    TryClamp(x, y + 1);
                    TryClamp(x, y - 1);

                    void TryClamp(int nx, int ny)
                    {
                        if (nx < 0 || ny < 0 || nx >= w || ny >= h) return;

                        int neighbor = heights[nx, ny];
                        int diff = neighbor - center;

                        if (diff > maxStep)
                        {
                            heights[nx, ny] = center + maxStep;
                            changed = true;
                        }
                        else if (diff < -maxStep)
                        {
                            heights[nx, ny] = center - maxStep;
                            changed = true;
                        }
                    }
                }
            }
        }

        return heights;
    }
}
