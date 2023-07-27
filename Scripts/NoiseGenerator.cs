using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator : MonoBehaviour
{
    public int seed;

    public int sizeX = 15;
    public int sizeY = 15;

    public float noiseScale = 1f;

    public int steps = 3;
    public float transparency = .5f;

    public Renderer texRenderer;
    public Texture2D tex;

    public void OnValidate()
    {
        DrawTexture();
        transform.localScale = new Vector3(sizeX * 8, sizeY * 8, 1f);
    }

    public void DrawTexture()
    {
        tex = new Texture2D(sizeX, sizeY);

        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] colorMap = new Color[sizeX * sizeY];
        for(int i = 0; i < sizeX; i++)
        {
            for(int j = 0; j < sizeY; j++)
            {
                float offset = seed % 50;

                float percent = 1 - Mathf.Floor(Mathf.Clamp01(Mathf.PerlinNoise(offset + noiseScale * i, offset + noiseScale * j)) * steps) / steps;

                colorMap[j * sizeX + i] = new Color(percent, percent, percent, transparency);
            }
        }

        tex.SetPixels(colorMap);
        tex.Apply();

        if (texRenderer != null) texRenderer.sharedMaterial.mainTexture = tex;
    }
}
