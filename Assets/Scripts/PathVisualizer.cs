using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    public PhysicsManager physicsManager;
    public int numSteps = 300;

    private PhysicsBody[] planets;
    private float G;
    private bool isActive = true;

    private Dictionary<PhysicsBody, List<Vector3>> planetTrajectories = new Dictionary<PhysicsBody, List<Vector3>>();
    private LineRenderer[] lines;

    private void Start()
    {
        physicsManager = FindObjectOfType<PhysicsManager>(); // Get reference to PhysicsManager
        planets = physicsManager.planets; // Do NOT clone objects, just reference them
        G = physicsManager.G; // Get the same gravitational constant

        InitializeTrajectoryLines();

        GameManager.OnSimulationStarted += () => { isActive = false; };
    }

    private void InitializeTrajectoryLines()
    {
        lines = new LineRenderer[planets.Length];

        for (int i = 0; i < planets.Length; i++)
        {
            planetTrajectories[planets[i]] = new List<Vector3>();

            lines[i] = new GameObject($"Trajectory_{planets[i].name}").AddComponent<LineRenderer>();
            lines[i].transform.parent = transform;
            lines[i].startWidth = 0.05f;
            lines[i].endWidth = 0.05f;
            lines[i].positionCount = numSteps;
            lines[i].material = new Material(Shader.Find("Sprites/Default"));
            lines[i].material.color = planets[i].GetComponent<SpriteRenderer>().color;
        }
    }

    private void FixedUpdate()
    {
        if (!isActive) return;

        CalculateTrajectories();
    }

    private void CalculateTrajectories()
    {
        // Clear old trajectory data
        foreach (var key in planetTrajectories.Keys)
        {
            planetTrajectories[key].Clear();
        }

        // Create temporary copies of positions and velocities to avoid modifying real objects
        Vector2[] positions = new Vector2[planets.Length];
        Vector2[] velocities = new Vector2[planets.Length];

        for (int i = 0; i < planets.Length; i++)
        {
            positions[i] = planets[i].GetPosition();
            velocities[i] = planets[i].GetVelocity();
        }

        // Simulate numSteps into the future
        for (int step = 0; step < numSteps; step++)
        {
            // Store the positions before updating
            for (int i = 0; i < planets.Length; i++)
            {
                planetTrajectories[planets[i]].Add(new Vector3(positions[i].x, positions[i].y, 0));
            }

            // Apply Runge-Kutta to update positions and velocities for ALL planets at once
            for (int i = 0; i < planets.Length; i++)
            {
                RungeKuttaStep(i, positions, velocities, Time.fixedDeltaTime);
            }
        }

        DrawTrajectories();
    }

    private void DrawTrajectories()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            lines[i].positionCount = planetTrajectories[planets[i]].Count;
            lines[i].SetPositions(planetTrajectories[planets[i]].ToArray());
        }
    }

    private Vector2 CalculateAccelerationAtPosition(int index, Vector2[] positions, Vector2 position)
    {
        Vector2 acceleration = Vector2.zero;
        Vector2 relativePosition;

        // Iterate through each OTHER planet
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == index) continue;

            relativePosition = positions[i] - position; // Use position passed to function
            float dist = Mathf.Max(relativePosition.magnitude, 0.01f);
            acceleration += G * planets[i].mass * relativePosition / Mathf.Pow(dist, 3);
        }

        return acceleration;
    }

    private void RungeKuttaStep(int index, Vector2[] positions, Vector2[] velocities, float stepSize)
    {
        Vector2 k_1r, k_2r, k_3r, k_4r;
        Vector2 k_1v, k_2v, k_3v, k_4v;

        // Use positions[index] directly instead of storing a stagnant "initialPosition"
        k_1r = stepSize * velocities[index];
        k_1v = stepSize * CalculateAccelerationAtPosition(index, positions, positions[index]);

        k_2r = stepSize * (velocities[index] + k_1v / 2);
        k_2v = stepSize * CalculateAccelerationAtPosition(index, positions, positions[index] + k_1r / 2);

        k_3r = stepSize * (velocities[index] + k_2v / 2);
        k_3v = stepSize * CalculateAccelerationAtPosition(index, positions, positions[index] + k_2r / 2);

        k_4r = stepSize * (velocities[index] + k_3v);
        k_4v = stepSize * CalculateAccelerationAtPosition(index, positions, positions[index] + k_3r);

        // Update the planet's position and velocity dynamically over time
        positions[index] += (k_1r + 2 * k_2r + 2 * k_3r + k_4r) / 6;
        velocities[index] += (k_1v + 2 * k_2v + 2 * k_3v + k_4v) / 6;
    }

}
