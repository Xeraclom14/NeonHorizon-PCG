using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HorizontalSocket
{
    public string id;
    public bool symmetric;
    public bool flipped;
}

[System.Serializable]
public class VerticalSocket
{
    public string id;
    public int rotationIndex;
}

[CreateAssetMenu(fileName = "New_Piece", menuName = "WFC Piece")]
public class Piece : ScriptableObject
{
    public float weight = 1f;
    public int rotationIndex;
    public GameObject prefab;
    public int mapTileIndex;
    public int mapTileRotation;
    public HorizontalSocket pZ;
    public HorizontalSocket nZ;
    public HorizontalSocket pX;
    public HorizontalSocket nX;
    public VerticalSocket pY;
    public VerticalSocket nY;
    public Piece[] constraints;

    [SerializeField] public HashSet<Piece> pZ_neighbours = new HashSet<Piece>();
    [SerializeField] public HashSet<Piece> nZ_neighbours = new HashSet<Piece>();
    [SerializeField] public HashSet<Piece> pX_neighbours = new HashSet<Piece>();
    [SerializeField] public HashSet<Piece> nX_neighbours = new HashSet<Piece>();
    [SerializeField] public HashSet<Piece> pY_neighbours = new HashSet<Piece>();
    [SerializeField] public HashSet<Piece> nY_neighbours = new HashSet<Piece>();

    public HorizontalSocket GetPositiveZ()
    {
        switch (rotationIndex)
        {
            default: return pZ;
            case 1: return nX;
            case 2: return nZ;
            case 3: return pX;
        }
    }

    public HorizontalSocket GetNegativeZ()
    {
        switch (rotationIndex)
        {
            default: return nZ;
            case 1: return pX;
            case 2: return pZ;
            case 3: return nX;
        }
    }

    public HorizontalSocket GetPositiveX()
    {
        switch (rotationIndex)
        {
            default: return pX;
            case 1: return pZ;
            case 2: return nX;
            case 3: return nZ;
        }
    }

    public HorizontalSocket GetNegativeX()
    {
        switch (rotationIndex)
        {
            default: return nX;
            case 1: return nZ;
            case 2: return pX;
            case 3: return pZ;
        }
    }

    public VerticalSocket GetPositiveY()
    {
        return pY;
    }

    public int GetPositiveYRotation()
    {
        if (pY.rotationIndex < 0) return -1;

        return (pY.rotationIndex + rotationIndex) % 4;
    }

    public VerticalSocket GetNegativeY()
    {
        return nY;
    }

    public int GetNegativeYRotation()
    {
        if (nY.rotationIndex < 0) return -1;

        return (nY.rotationIndex + rotationIndex) % 4;
    }

    public void InstantiatePrefabAtCoords(Vector3 pos)
    {
        if (prefab == null) return;

        Instantiate(prefab, pos, Quaternion.Euler(0f, 90f * rotationIndex, 0f)).GetComponent<WFCPiece>().id = this;
    }

    public int GetMapTileRotation()
    {
        return (rotationIndex + mapTileRotation) % 4;
    }
}
