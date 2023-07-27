using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCMinimapGenerator : MonoBehaviour
{
    public Texture2D mapTexture;
    public Texture2D lookUpTexture;
    public Renderer texRenderer;


    private Color[] _colorMap;
    private Vector2Int _mapSize;
    private int height = 1;

    public void ResetTexture(Vector2Int size, int height)
    {
        _mapSize = size * 16;
        this.height = height - 1;

        mapTexture.Resize(_mapSize.x, _mapSize.y);
        mapTexture.filterMode = FilterMode.Point;
        mapTexture.wrapMode = TextureWrapMode.Clamp;

        _colorMap = new Color[_mapSize.x * _mapSize.y];
    }

    public void DrawTexture(int x, int y, int z, Piece p)
    {
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 16; j++)
            {
                Color col = SampleLookUp(i, j, p);

                if (col == Color.black)
                    continue;
                /*else if(col == Color.red)
                {
                    float gray = Mathf.Lerp(0f, .66f, y / (float)height);

                    _colorMap[(j + z * 16) * _mapSize.x + (i + x * 16)] = new Color(gray, gray, gray, .75f);
                }*/
                else
                {
                    Color previous = _colorMap[(j + z * 16) * _mapSize.x + (i + x * 16)];

                    float r = Mathf.Clamp01(previous.r + col.r);
                    float b = Mathf.Clamp01(previous.r + col.r) > .5f ? y / (float)height : 0f;
                    float g = previous.b <= b ? col.g : previous.g;

                    _colorMap[(j + z * 16) * _mapSize.x + (i + x * 16)] = new Color(r,g,b);

                }
            }
        }

    }

    public void UpdateTexture()
    {
        mapTexture.SetPixels(_colorMap);
        mapTexture.Apply();

        if (texRenderer != null) texRenderer.sharedMaterial.mainTexture = mapTexture;

        transform.localScale = new Vector3(_mapSize.x / 2f, _mapSize.y / 2f, 1f);
    }

    private Color SampleLookUp(int x, int y, Piece p)
    {
        y = 15 - y;

        if (p.mapTileIndex == -1) return Color.black;
        else if (p.mapTileIndex == -2) return Color.red;
        else
        {
            int xCoord = x;
            int yCoord = y;

            switch (p.GetMapTileRotation())
            {
                case 1: // 90
                    xCoord = y;
                    yCoord = -x + 15;
                    break;

                case 2: // 180
                    xCoord = -x + 15;
                    yCoord = -y + 15;
                    break;

                case 3: // 270
                    xCoord = -y + 15;
                    yCoord = x;
                    break;
            }

            return lookUpTexture.GetPixel(xCoord, lookUpTexture.height - 1 - (yCoord + p.mapTileIndex * 16));
        }
    }
}
