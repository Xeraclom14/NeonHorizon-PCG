using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCGBackground : MonoBehaviour
{
    public Renderer mainRenderer;
    public Renderer voidRenderer;

    public Material[] easyMaterials;
    public Material[] normalMaterials;
    public Material[] hardMaterials;
    public Material[] hardcoreMaterials;

    public Light globalLight;

    public Color easyLightColor;
    public Color normalLightColor;
    public Color hardLightColor;
    public Color hardcoreLightColor;

    public AmbientColors easyAmbientColors;
    public AmbientColors normalAmbientColors;
    public AmbientColors hardAmbientColors;
    public AmbientColors hardcoreAmbientColors;

    public ReflectionProbe globalReflections;

    [System.Serializable]
    public struct AmbientColors
    {
        [ColorUsageAttribute(false, true)]
        public Color sky;
        [ColorUsageAttribute(false, true)]
        public Color equator;
        [ColorUsageAttribute(false, true)]
        public Color ground;
    }

    public void SetDifficultyMaterials(int level)
    {
        if (mainRenderer == null || voidRenderer == null || globalLight == null || globalReflections == null) Debug.LogError("Missing References");

        switch (level)
        {
            case 1:
                mainRenderer.sharedMaterial = normalMaterials[0];
                voidRenderer.sharedMaterial = normalMaterials[1];
                globalLight.color = normalLightColor;

                RenderSettings.ambientSkyColor = normalAmbientColors.sky;
                RenderSettings.ambientEquatorColor = normalAmbientColors.equator;
                RenderSettings.ambientGroundColor = normalAmbientColors.ground;
                break;

            case 2:
                mainRenderer.sharedMaterial = hardMaterials[0];
                voidRenderer.sharedMaterial = hardMaterials[1];
                globalLight.color = hardLightColor;

                RenderSettings.ambientSkyColor = hardAmbientColors.sky;
                RenderSettings.ambientEquatorColor = hardAmbientColors.equator;
                RenderSettings.ambientGroundColor = hardAmbientColors.ground;
                break;

            case 3:
                mainRenderer.sharedMaterial = hardcoreMaterials[0];
                voidRenderer.sharedMaterial = hardcoreMaterials[1];
                globalLight.color = hardcoreLightColor;

                RenderSettings.ambientSkyColor = hardcoreAmbientColors.sky;
                RenderSettings.ambientEquatorColor = hardcoreAmbientColors.equator;
                RenderSettings.ambientGroundColor = hardcoreAmbientColors.ground;
                break;

            default:
                mainRenderer.sharedMaterial = easyMaterials[0];
                voidRenderer.sharedMaterial = easyMaterials[1];
                globalLight.color = easyLightColor;

                RenderSettings.ambientSkyColor = easyAmbientColors.sky;
                RenderSettings.ambientEquatorColor = easyAmbientColors.equator;
                RenderSettings.ambientGroundColor = easyAmbientColors.ground;
                break;
        }

        globalReflections.RenderProbe();
    }
}
