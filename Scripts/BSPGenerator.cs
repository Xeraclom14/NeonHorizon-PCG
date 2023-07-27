using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSPGenerator : MonoBehaviour
{
    public Vector2 size;
    public int iterations = 5;
    [Range(0, 1)]
    public float splitFactor = .5f;
    [Range(0, 1)]
    public float spawnThreshold = .5f;
    public int seed = 0;
    public float noiseScale = 20f;
    public float noiseOffset = 0f;
    public float maxHeight = 20f;
    public bool autoUpdate;
    public GameObject cubePrefab;
    public List<Partition> partitions = new List<Partition>();

    public void GenerateMap()
    {
        UnityEngine.Random.InitState(seed);

        partitions.Clear();
        partitions.Add(new Partition(-size.x / 2f, size.x / 2f, size.y / 2f, -size.y / 2f, 0f));

        List<Partition> newPartitions = new List<Partition>();

        for (int i = 0; i < iterations; i++)
        {
            foreach(Partition p in partitions)
            {
                Partition[] split = p.Split(splitFactor);

                newPartitions.Add(split[0]);
                newPartitions.Add(split[1]);
            }

            partitions.Clear();
            partitions.AddRange(newPartitions);

            newPartitions.Clear();
        }
    }

    public void SpawnBlocks()
    {
        if (partitions.Count == 0) return;

        for (int i = transform.childCount; i > 0; --i)
            DestroyImmediate(transform.GetChild(0).gameObject);

        foreach(Partition p in partitions)
        {
            if (p.percent <= spawnThreshold) continue;

            Vector2 center = p.GetCenter();

            float height = Mathf.PerlinNoise((center.x + noiseOffset) * noiseScale, (center.y + noiseOffset) * noiseScale) * maxHeight;
            Vector3 worldCenter = transform.TransformPoint(new Vector3(center.x, height/2f, center.y));

            GameObject cube = Instantiate(cubePrefab, worldCenter, Quaternion.identity, transform);
            cube.transform.localScale = new Vector3(p.GetWidth(), height, p.GetHeight());
        }
    }

    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        GenerateMap();
        SpawnBlocks();
    }

    public void OnValidate()
    {
        if (autoUpdate) GenerateMap();
    }

    public void OnDrawGizmosSelected()
    {
        foreach(Partition p in partitions)
        {
            Vector2 center = p.GetCenter();

            float height = Mathf.PerlinNoise((center.x + noiseOffset) * noiseScale, (center.y + noiseOffset) * noiseScale) * maxHeight;
            Vector3 size = new Vector3(p.GetWidth(), height, p.GetHeight());

            if(p.percent > spawnThreshold)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.5f);
                Gizmos.DrawWireCube(transform.TransformPoint(center.x, height/2f, center.y), size);
            }
        }
    }
}

public class Partition
{
    public float left = 0f;
    public float right = 0f;
    public float top = 0f;
    public float bottom = 0f;
    public float percent = 0f;

    public Partition(float l, float r, float t, float b, float p)
    {
        left = l;
        right = r;
        top = t;
        bottom = b;
        percent = p;
    }

    public float GetWidth()
    {
        return Mathf.Abs(left - right);
    }

    public float GetHeight()
    {
        return Mathf.Abs(top - bottom);
    }

    public Vector2 GetCenter()
    {
        return new Vector2((left + right) / 2f, (top + bottom) / 2f);
    }

    public Partition[] Split(float splitFactor)
    {
        Partition[] ret = new Partition[2];

        float width = GetWidth();
        float height = GetHeight();

        float splitPercent = 0.5f + Random.Range(-splitFactor, splitFactor) / 2f;

        if (width > height) // split vertically
        {
            ret[0] = new Partition(left, Mathf.Lerp(left, right, splitPercent), top, bottom, Random.Range(0f, 1f));
            ret[1] = new Partition(Mathf.Lerp(left, right, splitPercent), right, top, bottom, Random.Range(0f, 1f));
        }
        else
        {
            ret[0] = new Partition(left, right, top, Mathf.Lerp(top, bottom, splitPercent), Random.Range(0f, 1f));
            ret[1] = new Partition(left, right, Mathf.Lerp(top, bottom, splitPercent), bottom, Random.Range(0f, 1f));
        }

        return ret;
    }
}
