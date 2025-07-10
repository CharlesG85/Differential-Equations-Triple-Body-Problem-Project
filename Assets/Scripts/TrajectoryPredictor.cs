using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    private PhysicsBodyState[] planetStates;
    public PhysicsManager physicsManager;
    public Material trajectoryMaterial;
    private LineRenderer[] trajectoryLines;

    private int relativeBodyIndex;
    public Transform targetLockIcon;

    private Vector2[,] positions;
    private Vector2[,] velocities;

    private float G;
    private float speedMultiplier;
    public int numSteps;

    private bool isSimulationStarted = false;

    private void Start()
    {
        GameManager.OnSimulationStarted += () => { isSimulationStarted = true; };

        // Initialize Physics Body States
        planetStates = new PhysicsBodyState[physicsManager.planets.Length];

        for (int i = 0; i < physicsManager.planets.Length; i++)
        {
            planetStates[i] = new PhysicsBodyState();
            planetStates[i].CloneState(physicsManager.planets[i]);
        }

        G = physicsManager.G;
        speedMultiplier = physicsManager.speedMultiplier;

        positions = new Vector2[planetStates.Length, numSteps];
        velocities = new Vector2[planetStates.Length, numSteps];

        trajectoryLines = new LineRenderer[planetStates.Length];

        // Initialize Trajectory Lines
        for (int i = 0; i < planetStates.Length; i++)
        {
            LineRenderer line = new GameObject($"Trajectory_{i}").AddComponent<LineRenderer>();
            line.transform.parent = transform;
            line.startWidth = 0.02f;
            line.endWidth = 0.02f;
            line.positionCount = numSteps;
            line.material = new Material(trajectoryMaterial);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(planetStates[i].color, 0f), // Color at start
                    new GradientColorKey(planetStates[i].color, 1f)  // Color at end
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),  // Transparent at start
                    new GradientAlphaKey(0f, 1f)   // Fully visible at end
                }
            );

            line.colorGradient = gradient;
            line.sortingOrder = -1000;
            trajectoryLines[i] = line;
        }

        // Default to global trajectories
        relativeBodyIndex = planetStates.Length;
        targetLockIcon.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        G = physicsManager.G;
        speedMultiplier = physicsManager.speedMultiplier;

        // Update Physics Body States
        if (isSimulationStarted)
        {
            for (int i = 0; i < physicsManager.planets.Length; i++)
            {
                planetStates[i].CloneState(physicsManager.planetStates[i]);
            }
        }
        else
        {
            for (int i = 0; i < physicsManager.planets.Length; i++)
            {
                planetStates[i].CloneState(physicsManager.planets[i]);
            }
        }

        if (relativeBodyIndex >= planetStates.Length)
        {
            SetGlobalTrajectories();
        }
        else
        {
            SetLocalTrajectories();
            targetLockIcon.transform.position = positions[relativeBodyIndex, 0];
        }
    }

    private void SetLocalTrajectories()
    {
        for (int i = 0; i < numSteps; i++)
        {
            // Iterate through each planet
            for (int j = 0; j < planetStates.Length; j++)
            {
                RungeKuttaStep(planetStates[j], Time.fixedDeltaTime);
            }

            // Iterate through each planet
            for (int j = 0; j < planetStates.Length; j++)
            {
                planetStates[j].UpdatePositionAndVelocity();
                positions[j, i] = planetStates[j].GetPosition();
                velocities[j, i] = planetStates[j].GetVelocity();
            }
        }

        Vector2[,] rowPositions = new Vector2[numSteps, planetStates.Length];

        for (int i = 0; i < planetStates.Length; i++)
        {
            for (int j = 0; j < numSteps; j++)
            {
                rowPositions[j, i] = positions[i, j];
            }
        }

        for (int i = 0; i < planetStates.Length; i++)
        {
            Vector3[] targetLinePositions = new Vector3[numSteps];

            if (i == relativeBodyIndex)
            {
                for (int j = 0; j < numSteps; j++)
                {
                    targetLinePositions[j] = rowPositions[j, i];
                }

                trajectoryLines[i].SetPositions(targetLinePositions);
                continue;
            }

            for (int j = 0; j < numSteps; j++)
            {
                rowPositions[j, i] = (rowPositions[j, i] - positions[relativeBodyIndex, j]) + positions[relativeBodyIndex, 0];
                targetLinePositions[j] = rowPositions[j, i];
            }

            trajectoryLines[i].SetPositions(targetLinePositions);
        }
    }

    private void SetGlobalTrajectories()
    {
        for (int i = 0; i < numSteps; i++)
        {
            // Iterate through each planet
            for (int j = 0; j < planetStates.Length; j++)
            {
                RungeKuttaStep(planetStates[j], Time.fixedDeltaTime);
            }

            // Iterate through each planet
            for (int j = 0; j < planetStates.Length; j++)
            {
                planetStates[j].UpdatePositionAndVelocity();
                positions[j, i] = planetStates[j].GetPosition();
                velocities[j, i] = planetStates[j].GetVelocity();
            }
        }

        Vector3[] rowPositions = new Vector3[numSteps];

        for (int i = 0; i < planetStates.Length; i++)
        {
            for (int j = 0; j < numSteps; j++)
            {
                rowPositions[j] = positions[i, j];
            }

            trajectoryLines[i].SetPositions(rowPositions);
        }
    }

    private Vector2 CalculateAccelerationAtPosition(PhysicsBodyState planetState, Vector2 position)
    {
        Vector2 acceleration = Vector2.zero;
        Vector2 relativePosition;

        // Iterate through each OTHER planet
        for (int i = 0; i < planetStates.Length; i++)
        {
            if (planetStates[i] == planetState)
            {
                continue;
            }

            relativePosition = planetStates[i].GetPosition() - position;
            acceleration += G * planetStates[i].mass * (relativePosition) / Mathf.Pow(Mathf.Abs(Mathf.Max(relativePosition.magnitude, 0.01f)), 3);
        }

        return acceleration;
    }

    // Calculate Position and Velocity Using Runge-Kutta
    private void RungeKuttaStep(PhysicsBodyState planetState, float stepSize)
    {
        stepSize *= speedMultiplier;

        Vector2 k_1r, k_2r, k_3r, k_4r;
        Vector2 k_1v, k_2v, k_3v, k_4v;

        Vector2 initialPosition = planetState.GetPosition();
        Vector2 initialVelocity = planetState.GetVelocity();

        k_1r = stepSize * initialVelocity;
        k_1v = stepSize * CalculateAccelerationAtPosition(planetState, initialPosition);

        k_2r = stepSize * (initialVelocity + k_1v / 2);
        k_2v = stepSize * CalculateAccelerationAtPosition(planetState, initialPosition + k_1r / 2);

        k_3r = stepSize * (initialVelocity + k_2v / 2);
        k_3v = stepSize * CalculateAccelerationAtPosition(planetState, initialPosition + k_2r / 2);

        k_4r = stepSize * (initialVelocity + k_3v);
        k_4v = stepSize * CalculateAccelerationAtPosition(planetState, initialPosition + k_3r);

        planetState.SetTargetPosition(initialPosition + (k_1r + 2 * k_2r + 2 * k_3r + k_4r) / 6);
        planetState.SetTargetVelocity(initialVelocity + (k_1v + 2 * k_2v + 2 * k_3v + k_4v) / 6);
    }

    public void IncrementRelativeBody()
    {
        relativeBodyIndex += 1;
        if(relativeBodyIndex > planetStates.Length)
        {
            relativeBodyIndex = 0;
            targetLockIcon.gameObject.SetActive(true);
        }
        if (relativeBodyIndex < planetStates.Length)
        {
            Debug.Log(planetStates[relativeBodyIndex].GetPosition());
        }

        if (relativeBodyIndex == planetStates.Length)
        {
            targetLockIcon.gameObject.SetActive(false);
        }
    }
}
