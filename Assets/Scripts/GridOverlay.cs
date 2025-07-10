using UnityEngine;

public class GridOverlay : MonoBehaviour
{
    public float gridScale = 1f;  // Scale of the grid
    public Material gridMaterial; // Reference to the grid material
    private Vector2 lastCameraPos;

    void Start()
    {
        if (Camera.main != null)
            lastCameraPos = Camera.main.transform.position;
    }

    void LateUpdate()
    {
        if (Camera.main == null || gridMaterial == null) return;

        // Get camera position
        Vector2 camPos = Camera.main.transform.position;

        // Calculate how much the camera has moved
        Vector2 delta = (camPos - lastCameraPos) / gridScale;

        // Offset the texture UVs
        gridMaterial.mainTextureOffset += delta;

        // Update last position
        lastCameraPos = camPos;
    }
}
