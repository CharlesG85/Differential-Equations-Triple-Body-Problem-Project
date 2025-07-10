using UnityEngine;

public class GridDistortion : MonoBehaviour
{
    public Material gridMaterial;
    private PhysicsBody[] planets;
    private static readonly int PlanetCountID = Shader.PropertyToID("_PlanetCount");
    private static readonly int PlanetsID = Shader.PropertyToID("_Planets");

    void Start()
    {
        planets = FindObjectsOfType<PhysicsBody>();
    }

    void Update()
    {
        if (gridMaterial == null || planets.Length == 0) return;

        Vector4[] planetPositions = new Vector4[4];
        int count = Mathf.Min(planets.Length, 4);

        for (int i = 0; i < count; i++)
        {
            Vector3 worldPos = planets[i].transform.position;
            planetPositions[i] = new Vector4(worldPos.x, worldPos.y, 0, 0);
        }

        for (int i = count; i < 4; i++)
        {
            planetPositions[i] = new Vector4(9999, 9999, 0, 0);
        }

        // Force Unity to recognize _Planets as an array
        gridMaterial.SetFloat(PlanetCountID, count);
        gridMaterial.SetVectorArray(PlanetsID, planetPositions);
    }
}
