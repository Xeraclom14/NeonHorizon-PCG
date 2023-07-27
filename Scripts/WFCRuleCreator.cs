using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCRuleCreator : MonoBehaviour
{
    public Piece[] modules;

    [ContextMenu("Update Piece Neighbours")]
    public void UpdatePieceNeighbours()
    {
        foreach(Piece p in modules)
        {
            // Check Z+ Neighbours
            p.pZ_neighbours.Clear();
            foreach(Piece check in modules)
            {
                if (Contains(p.constraints, check)) continue;

                if(CheckHorizontalSockets(p.GetPositiveZ(), check.GetNegativeZ()))
                {
                    p.pZ_neighbours.Add(check);
                }
            }

            // Check Z- Neighbours
            p.nZ_neighbours.Clear();
            foreach (Piece check in modules)
            {
                if (Contains(p.constraints, check)) continue;

                if (CheckHorizontalSockets(p.GetNegativeZ(), check.GetPositiveZ()))
                {
                    p.nZ_neighbours.Add(check);
                }
            }

            // Check X+ Neighbours
            p.pX_neighbours.Clear();
            foreach (Piece check in modules)
            {
                if (Contains(p.constraints, check)) continue;

                if (CheckHorizontalSockets(p.GetPositiveX(), check.GetNegativeX()))
                {
                    p.pX_neighbours.Add(check);
                }
            }

            // Check X+ Neighbours
            p.nX_neighbours.Clear();
            foreach (Piece check in modules)
            {
                if (Contains(p.constraints, check)) continue;

                if (CheckHorizontalSockets(p.GetNegativeX(), check.GetPositiveX()))
                {
                    p.nX_neighbours.Add(check);
                }
            }

            // Check Y+ Neighbours
            p.pY_neighbours.Clear();
            foreach (Piece check in modules)
            {
                if (Contains(p.constraints, check)) continue;

                if (CheckPositiveY(p, check))
                {
                    p.pY_neighbours.Add(check);
                }
            }

            // Check Y- Neighbours
            p.nY_neighbours.Clear();
            foreach (Piece check in modules)
            {
                if (Contains(p.constraints, check)) continue;

                if (CheckNegativeY(p, check))
                {
                    p.nY_neighbours.Add(check);
                }
            }
        }
    }

    public bool CheckHorizontalSockets(HorizontalSocket a, HorizontalSocket b)
    {
        if (a.id != b.id) return false;

        if ((a.symmetric && b.symmetric) || (a.flipped != b.flipped)) return true;

        return false;
    }

    public bool CheckPositiveY(Piece a, Piece b)
    {
        if (a.pY.id != b.nY.id) return false;

        if (a.GetPositiveYRotation() == b.GetNegativeYRotation()) return true;
        return false;
    }

    public bool CheckNegativeY(Piece a, Piece b)
    {
        if (a.nY.id != b.pY.id) return false;
        
        if (a.GetNegativeYRotation() == b.GetPositiveYRotation()) return true;
        return false;
    }

    public bool Contains(Piece[] list, Piece p)
    {
        if (list == null) return false;

        foreach (Piece check in list) if (check == p) return true;

        return false;
    }
}
