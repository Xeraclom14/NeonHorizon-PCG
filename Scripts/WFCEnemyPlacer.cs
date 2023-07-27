using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCEnemyPlacer : MonoBehaviour
{
    public Vector2 worldSize;
    public int enemies = 3;
    [Range(0f, 1f)]
    public float angleOffset = 0f;
    public float distance = 3f;
    public bool snapToGrid = true;

    public EnemyGroup encounter1;
    public EnemyGroup encounter2;

    public float spawnOffset;

    Vector3[] positions;

    public EnemyWave[] easyWaves;
    public EnemyWave[] normalWaves;
    public EnemyWave[] hardWaves;
    public EnemyWave[] hardcoreWaves;

    public void OnValidate()
    {
        if (enemies < 1) enemies = 1;
    }

    public void StartGroups()
    {
        encounter1.StartGroup();
        encounter2.StartGroup();
    }

    public void PopulateEnemyGroups(int waves, Difficulty difficulty)
    {
        int waves1 = waves / 2;
        int waves2 = waves - waves1;

        PopulateEnemyGroup(encounter1, waves1, difficulty);
        PopulateEnemyGroup(encounter2, waves2, difficulty);
    }

    public void PopulateEnemyGroup(EnemyGroup encounter, int waves, Difficulty difficulty)
    {
        for (int i = encounter.transform.childCount; i > 0; --i)
            DestroyImmediate(encounter.transform.GetChild(0).gameObject);

        encounter.waves = new Transform[waves];

        for (int i = 0; i < waves; i++)
        {
            Transform wave = new GameObject("Wave" + (i+1)).transform;

            wave.parent = encounter.transform;

            EnemyWave randomGroup = GetRandomGroup(difficulty);

            List<Vector3> positions = new List<Vector3>(PlaceEnemies(randomGroup.groupSpawners.Length));
            for (int j = 0; j < positions.Count; j++)
            {
                Vector3 temp = positions[j];
                int rand = Random.Range(j, positions.Count);
                positions[j] = positions[rand];
                positions[rand] = temp;
            }

            for (int j = 0; j < positions.Count; j++)
            {
                GameObject randEnemy = randomGroup.groupSpawners[j];

                EnemySpawner spwn = Instantiate(randEnemy, positions[j] + Vector3.up * spawnOffset, Quaternion.identity, wave).GetComponent<EnemySpawner>();
                spwn.spawnDelay = .2f * j;
            }

            encounter.waves[i] = wave;
        }
    }

    public EnemyWave GetRandomGroup(Difficulty difficulty)
    {
        switch (difficulty)
        {
            case Difficulty.normal: return normalWaves[Random.Range(0, normalWaves.Length)];
            case Difficulty.hard: return hardWaves[Random.Range(0, hardWaves.Length)];
            case Difficulty.hardcore: return hardcoreWaves[Random.Range(0, hardcoreWaves.Length)];

            default: return easyWaves[Random.Range(0, easyWaves.Length)];
        }
    }

    public Vector3[] PlaceEnemies(int numEnemies)
    {
        enemies = numEnemies;

        float rangeX = (worldSize.x / 2f) - (distance + Mathf.Sqrt(enemies));
        float rangeY = (worldSize.y / 2f) - (distance + Mathf.Sqrt(enemies));

        for(int i = 0; i < 10000; i++)
        {
            transform.position = Vector3.up * transform.position.y + new Vector3(Random.Range(-rangeX, rangeX), 0, Random.Range(-rangeY, rangeY));
            angleOffset = Random.Range(0f, 1f);

            if (TryPlace())
            {
                Debug.Log("Tries: " + (i+1));
                break;
            }
        }

        return positions;
    }

    public bool TryPlace()
    {
        positions = new Vector3[enemies];

        int pointOffset = 2;

        for (int i = 0; i < enemies; i++)
        {
            positions[i] = Vector3.negativeInfinity;

            while(pointOffset < 2000)
            {
                float angle = 2 * Mathf.PI * (0.618034f * (i + pointOffset) + angleOffset);
                float currentDistance = Mathf.Sqrt((i + pointOffset) / (float)(enemies + pointOffset)) * distance * Mathf.Sqrt(enemies + pointOffset);

                Vector3 dir = Vector3.forward * Mathf.Sin(angle) + Vector3.right * Mathf.Cos(angle);
                Vector3 checkPos = new Vector3(transform.position.x, 100f, transform.position.z) + dir * currentDistance;

                if (snapToGrid) checkPos = SnapToGrid(checkPos);

                if (Physics.Raycast(checkPos, Vector3.down, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask("Default"), QueryTriggerInteraction.Ignore)){
                    positions[i] = hit.point;
                    break;
                }
                else
                {
                    pointOffset++;
                }
            }

            if (pointOffset >= 10000) return false;

        }

        return true;
    }

    public Vector3 SnapToGrid(Vector3 point)
    {
        float xPos = Mathf.Floor(point.x) + .5f;
        float zPos = Mathf.Floor(point.z) + .5f;

        return new Vector3(xPos, point.y, zPos);
    }

    public void OnDrawGizmos()
    {
        if (TryPlace())
        {
            for (int i = 0; i < enemies; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(positions[i], .5f);
            }
        }
    }
}
