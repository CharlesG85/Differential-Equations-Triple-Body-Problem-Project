using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    public PhysicsBody[] planets;
    [HideInInspector] public PhysicsBodyState[] planetStates;
    public float G = 10;
    public float speedMultiplier = 0.5f;

    private bool isPaused = false;

    private void Start()
    {
        InitializePlanets();

        GameManager.OnSimulationPaused += Pause;
        GameManager.OnSimulationResumed += Play;
        GameManager.OnSimulationStarted += OnSimulationStarted;

        this.enabled = false;
    }

    private void OnDestroy()
    {
        GameManager.OnSimulationPaused -= Pause;
        GameManager.OnSimulationResumed -= Play;
        GameManager.OnSimulationStarted -= OnSimulationStarted;
    }

    private void OnSimulationStarted()
    {
        this.enabled = true;
        UpdatePlanetData();
    }

    private void Pause()
    {
        isPaused = true;
    }

    private void Play()
    {
        isPaused = false;
    }

    private void InitializePlanets()
    {
        planetStates = new PhysicsBodyState[planets.Length];

        for (int i = 0; i < planets.Length; i++)
        {
            planetStates[i] = new PhysicsBodyState();
            planetStates[i].CloneState(planets[i]);
        }
    }

    private void UpdatePlanetData()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            planetStates[i].CloneState(planets[i]);
        }
    }

    private void FixedUpdate()
    {
        if (isPaused)
        {
            return;
        }

        // Iterate through each planet
        for (int i = 0; i < planetStates.Length; i++)
        {
            RungeKuttaStep(planetStates[i], Time.fixedDeltaTime);
            Debug.Log(planetStates[i]);
        }

        // Iterate through each planet
        for (int i = 0; i < planetStates.Length; i++)
        {
            planetStates[i].UpdatePositionAndVelocity();
            planets[i].SetPosition(planetStates[i].GetPosition());
        }

        UpdatePlanetRotations();
    }

    private void UpdatePlanetRotations()
    {
        for (int i = 0; i < planets.Length; i++)
        {
            planets[i].transform.Rotate(new Vector3(0, 0, 1), planets[i].angularVelocity * Time.fixedDeltaTime);
        }
    }

    private Vector2 CalculateAccelerationAtPosition(PhysicsBodyState planetState, Vector2 position)
    {
        Vector2 acceleration = Vector2.zero;
        Vector2 relativePosition;

        // Iterate through each OTHER planet
        for (int i = 0; i < planets.Length; i++)
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
}
