using UnityEngine;

public class TrackLightingFix : MonoBehaviour
{
    public Material trackSkybox;

    void Start()
    {
        RenderSettings.skybox = trackSkybox;
        DynamicGI.UpdateEnvironment();
    }
}
