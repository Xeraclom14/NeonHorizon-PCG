using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public enum Difficulty
{
    easy = 0,
    normal = 1,
    hard = 2,
    hardcore = 3
}

// Clase auxiliar para almacenar una lista de superposición;
public class WFCSuperposition
{
    public HashSet<Piece> possibilities;
    public Piece colapsedAt = null;

    public float GetWeightSum()
    {
        float sum = 0f;
        foreach (Piece p in possibilities) sum += p.weight;

        return sum;
    }
}

public class WFCGenerator : MonoBehaviour
{
    public Difficulty difficulty;
    [SerializeField] public int3 size;
    public bool randomSeed;
    public int seed;
    public float scale;
    public float airNoiseScale = 0.15f;
    public int airHeight = 3;
    public WFCRuleCreator rules;
    public NoiseGenerator visualization;
    public WFCMinimapGenerator mapGenerator;
    public WFCPathfinding wfcPathfinder;
    public WFCEnemyPlacer enemyPlacer;
    public PCGBackground background;

    private WFCSuperposition[,,] grid;

    public void Start()
    {
        UpdatePathfindingGrids();
    }

    [ContextMenu("Generate Level")]
    public void Generate()
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        if (rules.modules[0].pZ_neighbours.Count == 0) rules.UpdatePieceNeighbours();

        transform.localScale = Vector3.one;

        if (size.x < 1 || size.y < 1 || size.z < 1)
        {
            UnityEngine.Debug.LogWarning("Error: Invalid Size.", gameObject);

            sw.Stop();
            UnityEngine.Debug.Log("Time elapsed: " + sw.Elapsed.Milliseconds + "ms");
            return;
        }

        for (int i = transform.childCount; i > 0; --i)
            DestroyImmediate(transform.GetChild(0).gameObject);

        if(randomSeed) seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        InitializeGrid();

        int iterations = 0;
        int maxIterations = 10000;

        while (!IsCollapsed())
        {
            Iterate();

            iterations++;
            if (iterations > maxIterations)
            {
                UnityEngine.Debug.LogWarning("Error: Max iterations exceeded.", gameObject);
                break;
            }

        }
        UnityEngine.Debug.Log("Total iterations: " + iterations);

        if (mapGenerator != null) mapGenerator.ResetTexture(new Vector2Int(size.x, size.z), size.y);

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    if (grid[i, j, k].possibilities.Count == 0 || grid[i, j, k].colapsedAt == null)
                    {
                        UnityEngine.Debug.LogWarning("Error: No solution found at coordinates (" + i + ", " + j + ", " + k + ").");
                        continue;
                    }

                    Piece instantiate = grid[i, j, k].colapsedAt;
                    WFCPiece instantiatedPiece = null;

                    if (instantiate.prefab == null) continue;

#if UNITY_EDITOR
                    if (Application.isEditor && !Application.isPlaying)
                    {
                        GameObject instantiatedPieceObj = UnityEditor.PrefabUtility.InstantiatePrefab(instantiate.prefab, transform) as GameObject;
                        instantiatedPieceObj.transform.position = transform.position + new Vector3(i, j, k) * 4f + new Vector3(size.x, size.y, size.z) * -2f + Vector3.one * 2f;
                        instantiatedPieceObj.transform.rotation = Quaternion.Euler(0f, instantiate.rotationIndex * 90f, 0f);

                        instantiatedPiece = instantiatedPieceObj.GetComponent<WFCPiece>();
                    }
                    else
                    {
                        instantiatedPiece = Instantiate(instantiate.prefab,
                            transform.position + new Vector3(i, j, k) * 4f + new Vector3(size.x, size.y, size.z) * -2f + Vector3.one * 2f,
                            Quaternion.Euler(0f, instantiate.rotationIndex * 90f, 0f), transform)
                            .GetComponent<WFCPiece>();

                        instantiatedPiece.id = instantiate;
                        instantiatedPiece.SetDifficultyMaterials((int)difficulty);

                        if (mapGenerator != null) mapGenerator.DrawTexture(i, j, k, instantiate);
                    }

                    instantiatedPiece.id = instantiate;
                    instantiatedPiece.SetDifficultyMaterials((int)difficulty);

                    if (mapGenerator != null) mapGenerator.DrawTexture(i, j, k, instantiate);

#else
                    Piece instantiate = grid[i, j, k].colapsedAt;
                    if (instantiate.prefab == null) continue;

                    WFCPiece instantiatedPiece = Instantiate(instantiate.prefab,
                        transform.position + new Vector3(i, j, k) * 4f + new Vector3(size.x, size.y, size.z) * -2f + Vector3.one * 2f,
                        Quaternion.Euler(0f, instantiate.rotationIndex * 90f, 0f), transform)
                        .GetComponent<WFCPiece>();

                    instantiatedPiece.id = instantiate;
                    instantiatedPiece.SetDifficultyMaterials((int)difficulty);

                    if (mapGenerator != null) mapGenerator.DrawTexture(i, j, k, instantiate);
#endif
                }
            }
        }

        if (mapGenerator != null) mapGenerator.UpdateTexture();

        transform.localScale = Vector3.one * scale;

        sw.Stop();
        UnityEngine.Debug.Log("Time elapsed: " + sw.Elapsed.Milliseconds + "ms");

        background.SetDifficultyMaterials((int)difficulty);
    }

    [ContextMenu("Update Pathfinding Grids")]
    public void UpdatePathfindingGrids() //Actualizar grillas de pathfinding
    {
        wfcPathfinder.UpdateGrids(size.y - 1, new Vector3(transform.position.x, transform.position.y - (size.y - 1) * 2 * scale, transform.position.z), new Vector2(size.x, size.z), scale);
    }

    public void UpdateEnemyPlacement(int waves) //Actualizar posicionamiento de enemigos
    {
        enemyPlacer.worldSize = new Vector2((size.x - 3) * 4 * scale, (size.z - 3) * 4 * scale);
        enemyPlacer.PopulateEnemyGroups(waves, difficulty);
    }

    private void InitializeGrid() //Inicializar matriz de listas de superposicion
    {
        grid = new WFCSuperposition[size.x, size.y, size.z];

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    grid[i, j, k] = new WFCSuperposition();
                    grid[i, j, k].possibilities = new HashSet<Piece>(rules.modules);
                }
            }
        }

        int[,] airLevel = new int[size.x,size.z];
        for (int i = 0; i < size.x; i++)
        {
            for (int k = 0; k < size.z; k++)
            {
                airLevel[i,k] = size.y - 1 - FilteredNoise(i, k);
            }
        }

        if(visualization != null)
        {
            visualization.seed = seed;
            visualization.sizeX = size.x;
            visualization.sizeY = size.z;
            visualization.steps = airHeight;

            visualization.DrawTexture();
        }

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    if (i == 0 || k == 0 || i == size.x-1 || k == size.z-1 || j >= airLevel[i, k])
                    {
                        CollapseAndPropagate(new int3(i, j, k), rules.modules[0]);
                    }
                }
            }
        }
    }

    private int FilteredNoise(int x, int y)
    {
        float offset = seed % 50;

        int value = Mathf.FloorToInt(Mathf.Clamp01(Mathf.PerlinNoise(offset + airNoiseScale * x, offset + airNoiseScale * y)) * airHeight);

        return value;
    }

    private bool IsCollapsed()
    {
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    if (grid[i, j, k].colapsedAt == null || grid[i, j, k].possibilities.Count > 1)
                    {
                        return false;
                    }
                    else if (grid[i, j, k].possibilities.Count == 0)
                    {
                        Restart();
                        return false;
                    }
                }
            }
        }

        return true;
    }

    private void Restart()
    {
        UnityEngine.Debug.Log("Restarting. Invalid seed: " + seed, gameObject);

        seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        UnityEngine.Random.InitState(seed);

        InitializeGrid();
    }

    private void Iterate()
    {
        int3 minEntropyCoords = GetMinimalEntropyCoordinates();

        CollapseAt(minEntropyCoords);
        Propagate(minEntropyCoords);
    }

    private void CollapseAndPropagate(int3 coords, Piece p)
    {
        WFCSuperposition superposition = grid[coords.x, coords.y, coords.z];

        superposition.possibilities.Clear();
        superposition.possibilities.Add(p);

        superposition.colapsedAt = p;

        Propagate(coords);
    }

    private int3 GetMinimalEntropyCoordinates()
    {
        int minEntropy = rules.modules.Length;
        List<int3> minEntropyCoords = new List<int3>();

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                for (int k = 0; k < size.z; k++)
                {
                    if (grid[i, j, k].possibilities.Count < 2 || grid[i, j, k].colapsedAt != null) continue;

                    if(grid[i,j,k].possibilities.Count < minEntropy)
                    {
                        minEntropy = grid[i, j, k].possibilities.Count;
                        minEntropyCoords.Clear();
                    }

                    if(grid[i, j, k].possibilities.Count == minEntropy)
                    {
                        minEntropyCoords.Add(new int3(i, j, k));
                    }
                }
            }
        }
        
        return minEntropyCoords[UnityEngine.Random.Range(0, minEntropyCoords.Count)];
    }

    private void CollapseAt(int3 coords)
    {
        if (grid[coords.x, coords.y, coords.z].possibilities.Count == 0) return;

        WFCSuperposition superposition = grid[coords.x, coords.y, coords.z];

        Piece selectedPiece = WeightedRandomSelection(superposition);

        superposition.possibilities.Clear();
        superposition.possibilities.Add(selectedPiece);

        superposition.colapsedAt = selectedPiece;
    }

    private Piece WeightedRandomSelection(WFCSuperposition superposition)
    {
        float sum = superposition.GetWeightSum();

        float random = UnityEngine.Random.Range(0f, sum);

        float currentIndexSum = 0f;
        foreach(Piece p in superposition.possibilities)
        {
            if (random > currentIndexSum && random < currentIndexSum + p.weight) return p;

            currentIndexSum += p.weight;
        }

        return rules.modules[0];
    }

    private void Propagate(int3 coords)
    {
        Stack<int3> stack = new Stack<int3>();
        stack.Push(coords);

        while(stack.Count > 0)
        {
            int3 currentCoordinates = stack.Pop();

            foreach(int3 d in GetNeighbourDirections(currentCoordinates))
            {
                int3 neighbourCoordinates = currentCoordinates + d;

                WFCSuperposition neighbourSuperposition = grid[neighbourCoordinates.x, neighbourCoordinates.y, neighbourCoordinates.z];
                if (neighbourSuperposition.colapsedAt != null) continue;

                HashSet<Piece> neighbourPossibilities = new HashSet<Piece>(neighbourSuperposition.possibilities);
                HashSet<Piece> validNeighbours = GetPossibleNeighbourPieces(currentCoordinates, d);

                foreach(Piece p in neighbourPossibilities)
                {
                    if (!validNeighbours.Contains(p))
                    {
                        ConstrainPieceAtCoords(neighbourCoordinates, p);
                        if (!stack.Contains(neighbourCoordinates)) stack.Push(neighbourCoordinates);
                    }
                }
            }
        }
    }

    private HashSet<Piece> GetPossibleNeighbourPieces(int3 curCoords, int3 d)
    {
        HashSet<Piece> list = new HashSet<Piece>();
        HashSet<Piece> currentPossibilities = grid[curCoords.x, curCoords.y, curCoords.z].possibilities;

        if (d.x == 1 && d.y == 0 && d.z == 0) // check X+
        {
            foreach(Piece p in currentPossibilities)
            {
                list.UnionWith(p.pX_neighbours);
            }
        }
        else if(d.x == -1 && d.y == 0 && d.z == 0) // check X-
        {
            foreach (Piece p in currentPossibilities)
            {
                list.UnionWith(p.nX_neighbours);
            }
        }
        else if (d.x == 0 && d.y == 1 && d.z == 0) // check Y+
        {
            foreach (Piece p in currentPossibilities)
            {
                list.UnionWith(p.pY_neighbours);
            }
        }
        else if (d.x == 0 && d.y == -1 && d.z == 0) // check Y-
        {
            foreach (Piece p in currentPossibilities)
            {
                list.UnionWith(p.nY_neighbours);
            }
        }
        else if (d.x == 0 && d.y == 0 && d.z == 1) // check Z+
        {
            foreach (Piece p in currentPossibilities)
            {
                list.UnionWith(p.pZ_neighbours);
            }
        }
        else if (d.x == 0 && d.y == 0 && d.z == -1)// check Z-
        {
            foreach (Piece p in currentPossibilities)
            {
                list.UnionWith(p.nZ_neighbours);
            }
        }

        return list;
    }

    private List<int3> GetNeighbourDirections(int3 coords)
    {
        List<int3> list = new List<int3>();

        if (coords.x + 1 < size.x) list.Add(new int3(1, 0, 0));
        if (coords.x - 1 >= 0) list.Add(new int3(-1, 0, 0));
        if (coords.y + 1 < size.y) list.Add(new int3(0, 1, 0));
        if (coords.y - 1 >= 0) list.Add(new int3(0, -1, 0));
        if (coords.z + 1 < size.z) list.Add(new int3(0, 0, 1));
        if (coords.z - 1 >= 0) list.Add(new int3(0,0,-1));

        return list;
    }

    private void ConstrainPieceAtCoords(int3 coords, Piece p)
    {
        WFCSuperposition superposition = grid[coords.x, coords.y, coords.z];

        superposition.possibilities.Remove(p);

        if(superposition.possibilities.Count == 1) superposition.colapsedAt = superposition.possibilities.ElementAt(0);
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, .25f);
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, size.z) * 4f * scale);
    }

    public void OnValidate()
    {
        transform.localScale = Vector3.one * scale;
    }
}
