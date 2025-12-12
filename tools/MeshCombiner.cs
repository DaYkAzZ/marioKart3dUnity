using UnityEngine;

public class MeshCombiner : MonoBehaviour
{
    void Start()
    {
        CombineMeshes();
    }

    public void CombineMeshes()
    {
        // Collect all mesh filters from children of this GameObject
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            // Set the mesh and its transform relative to the parent object
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            i++;
        }

        // Add a MeshFilter and MeshRenderer to the parent object if they don't exist
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();

        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Create a new combined mesh
        meshFilter.mesh = new Mesh();
        // The 'true' parameter merges all meshes into a single sub-mesh (requires shared material)
        meshFilter.mesh.CombineMeshes(combine, true, true);

        // Optionally, disable the original child objects
        foreach (MeshFilter filter in meshFilters)
        {
            if (filter.gameObject != this.gameObject) // Don't disable the parent
            {
                filter.gameObject.SetActive(false);
            }
        }

        Debug.Log("Meshes combined!");
    }
}
