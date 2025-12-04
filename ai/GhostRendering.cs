using UnityEngine;

public class OpacityChanger : MonoBehaviour
{
    public float targetAlpha = 0.5f; // Desired opacity (0.0 for fully transparent, 1.0 for fully opaque)

    void Start()
    {
        // Get the Renderer component of the GameObject
        Renderer objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            // Get the material's current color
            Color currentColor = objectRenderer.material.color;

            // Create a new color with the desired alpha
            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, targetAlpha);

            // Apply the new color to the material
            objectRenderer.material.color = newColor;

            // Ensure the material's rendering mode is set for transparency (if not already)
            // This might be necessary if the material's rendering mode wasn't set to Fade or Transparent in the Inspector
            if (objectRenderer.material.renderQueue >= 2000 && objectRenderer.material.renderQueue < 3000) // Check if not opaque
            {
                objectRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                objectRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                objectRenderer.material.SetInt("_ZWrite", 0);
                objectRenderer.material.DisableKeyword("_ALPHATEST_ON");
                objectRenderer.material.EnableKeyword("_ALPHABLEND_ON");
                objectRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                objectRenderer.material.renderQueue = 3000; // Transparent render queue
            }
        }
        else
        {
            Debug.LogWarning("No Renderer component found on this GameObject.");
        }
    }
}