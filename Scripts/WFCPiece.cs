using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WFCPiece : MonoBehaviour
{
    public Piece id;

    public MeshRenderer render;

    public Material[] easyMaterials;
    public Material[] normalMaterials;
    public Material[] hardMaterials;
    public Material[] hardcoreMaterials;

    [ContextMenu("Help")]
    public void LoadIntoEasy()
    {
        if (render == null) render = GetComponent<MeshRenderer>();

        hardMaterials = render.sharedMaterials;
        hardcoreMaterials = render.sharedMaterials;
    }

    public void SetDifficultyMaterials(int level)
    {
        if (render == null) render = GetComponent<MeshRenderer>();

        switch (level)
        {
            case 1: render.sharedMaterials = normalMaterials; break;
            case 2: render.sharedMaterials = hardMaterials; break;
            case 3: render.sharedMaterials = hardcoreMaterials; break;
            default: render.sharedMaterials = easyMaterials; break;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        GUI.color = new Color(0, 1, 0, .75f);

        if (id != null)
        {
            //Z+
            Handles.Label(transform.position + Vector3.forward * 1.75f, id.GetPositiveZ().id + (id.GetPositiveZ().flipped ? "f" : "") + (id.GetPositiveZ().symmetric ? "s" : ""));

            //Z-
            Handles.Label(transform.position + Vector3.forward * -1.75f, id.GetNegativeZ().id + (id.GetNegativeZ().flipped ? "f" : "") + (id.GetNegativeZ().symmetric ? "s" : ""));

            //X+
            Handles.Label(transform.position + Vector3.right * 1.75f, id.GetPositiveX().id + (id.GetPositiveX().flipped ? "f" : "") + (id.GetPositiveX().symmetric ? "s" : ""));

            //X-
            Handles.Label(transform.position + Vector3.right * -1.75f, id.GetNegativeX().id + (id.GetNegativeX().flipped ? "f" : "") + (id.GetNegativeX().symmetric ? "s" : ""));

            //Y+
            Handles.Label(transform.position + Vector3.up * 1.75f, "v" + id.GetPositiveY().id + (id.GetPositiveYRotation() < 0 ? "i" : "_" + id.GetPositiveYRotation().ToString()));

            //Y-
            Handles.Label(transform.position + Vector3.up * -1.75f, "v" + id.GetNegativeY().id + (id.GetNegativeYRotation() < 0 ? "i" : "_" + id.GetNegativeYRotation().ToString()));
        }

    }
    /*
    private void OnDrawGizmos()
    {

        Gizmos.color = new Color(0, 1, 0, .75f);
        //Gizmos.DrawWireCube(transform.position, transform.localScale * 4f);
    }*/
#endif
}
