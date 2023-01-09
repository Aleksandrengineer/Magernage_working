using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{

    /// <summary>
    /// Generating map function
    /// </summary>
    /// <param name="mapWidth"> map Width</param>
    /// <param name="mapHeight"> map Height</param>
    /// <param name="seed"> generation seed</param>
    /// <param name="scale"> scale of the noise map resolution on the plane</param>
    /// <param name="octaves"> number of octaves?</param>
    /// <param name="persistance"> decrease the lenghth of octave</param>
    /// <param name="lacunarity"> increase the frequency of octave</param>
    /// <param name="offset"> user defenition of offset of octaves</param>
    /// <returns></returns>
    
    public enum NormalizeMode {Local, Global};
    public static float [,] GenerateNoiseMap (int mapWidth, int mapHeight, NoiseSettings settings, Vector2 sampleCentre)
    {
        float [,] noiseMap = new float [mapWidth, mapHeight];

        //Seed of the generation
        System.Random prng = new System.Random(settings.seed);

        //Sampling octave from different location
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequency = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            octaveOffsets [i] = new Vector2 (offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2;
        float halfHeight = mapHeight / 2;

        for (int y = 0; y < mapHeight; y++)
        {
            for ( int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / settings.scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / settings.scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= settings.persistance; //decreases the octaves
                    frequency *= settings.lacunarity; //increases the octaves
                }

                //rangeof the noise map height
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap [x, y] = noiseHeight;

                if (settings.normalizeMode == NormalizeMode.Global)
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 2f); //the last number is crusial to make the terrain seemless
                    noiseMap [x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }

            }
        }


        //normalizing our noise map
        if (settings.normalizeMode == NormalizeMode.Local)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                for ( int x = 0; x < mapWidth; x++)
                {
                    noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
                }
            }
        }

        return noiseMap;
    }
}

[System.Serializable]
public class NoiseSettings
{
    public Noise.NormalizeMode normalizeMode;
    public float scale = 50;

    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = 0.6f;
    public float lacunarity = 2;

    //randomizing generation values
    public int seed;
    public Vector2 offset;

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01 (persistance);
    }
}