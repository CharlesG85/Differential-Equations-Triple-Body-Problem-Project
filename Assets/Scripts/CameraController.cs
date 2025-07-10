using UnityEngine;
using System;

public class CameraController : MonoBehaviour
{
    public float followSpeed = 2.0f;   // Follow speed when tracking planets
    public float zoomSpeed = 2.0f;     // Zoom speed
    public float freecamMoveSpeed = 5.0f;  // Speed of freecam movement
    public float freecamZoomSpeed = 2.0f;  // Zoom speed in freecam mode
    public float minZoom = 5f;         // Minimum zoom level
    public float maxZoom = 20f;        // Maximum zoom level
    public float zoomPadding = 2f;     // Extra zoom space so planets don't touch edges

    private PhysicsBody[] planets;
    private Camera cam;
    private bool isPaused = false;
    private bool isFreecamMode = true; // Tracks if freecam is active
    private Vector3 freecamPosition;    // Stores position in freecam mode
    private bool controllable = true;

    void Start()
    {
        cam = Camera.main;
        planets = FindObjectsOfType<PhysicsBody>(); // Find all planets in the scene

        GameManager.OnSimulationPaused += Pause;
        GameManager.OnSimulationResumed += Play;

        // Start in follow mode
        freecamPosition = transform.position;
    }

    private void Pause() => isPaused = true;
    private void Play() => isPaused = false;

    void LateUpdate()
    {
        if (isPaused) return;

        if (isFreecamMode)
        {
            HandleFreecamMovement();
        }
        else
        {
            FollowPlanets();
        }
    }

    // Smoothly follow planets
    void FollowPlanets()
    {
        if (planets.Length == 0) return;

        Vector2 midpoint = CalculateMidpoint();
        float zoomLevel = CalculateZoom();

        // Smoothly move towards the midpoint
        transform.position = Vector3.Lerp(transform.position, new Vector3(midpoint.x, midpoint.y, -10f), followSpeed * Time.deltaTime);

        // Smoothly adjust zoom
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, zoomLevel, zoomSpeed * Time.deltaTime);
    }

    // Handles movement in freecam mode
    void HandleFreecamMovement()
    {
        if (controllable == false) return;

        float moveX = Input.GetAxis("Horizontal") * freecamMoveSpeed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * freecamMoveSpeed * Time.deltaTime;
        float scroll = Input.GetAxis("Mouse ScrollWheel") * freecamZoomSpeed;

        freecamPosition += new Vector3(moveX, moveY, 0);
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll, minZoom, maxZoom);

        transform.position = freecamPosition;
    }

    // Toggle between Follow Mode and Freecam Mode (call this from UI)
    public void SetFreecamMode(bool isEnabled)
    {
        isFreecamMode = isEnabled;

        if (isFreecamMode)
        {
            // Store the current position before switching
            freecamPosition = transform.position;
            Debug.Log("Freecam Activated");
        }
        else
        {
            Debug.Log("Planet Follow Mode Activated");
        }
    }

    // Smoothly return to the midpoint of planets (call this from UI)
    public void ReturnToMidpoint()
    {
        if (planets.Length == 0) return;
        if (isFreecamMode == false) return;

        Vector2 midpoint = CalculateMidpoint();
        StartCoroutine(SmoothMoveTo(midpoint));
    }

    // Coroutine for smooth transition to midpoint
    private System.Collections.IEnumerator SmoothMoveTo(Vector2 targetPos)
    {
        controllable = false;
        float duration = 1.5f; // Smooth transition time
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPosition = new Vector3(targetPos.x, targetPos.y, -10f);

        while (elapsedTime < duration)
        {
            Debug.Log("HEYY!!!");
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        freecamPosition = targetPosition;
        controllable = true;
    }

    // Calculate the midpoint of all planets
    Vector2 CalculateMidpoint()
    {
        Vector2 sum = Vector2.zero;
        foreach (var planet in planets)
        {
            sum += planet.GetPosition();
        }
        return sum / planets.Length;
    }

    // Calculate appropriate zoom based on planet distances
    float CalculateZoom()
    {
        float maxDistance = 0f;
        for (int i = 0; i < planets.Length; i++)
        {
            for (int j = i + 1; j < planets.Length; j++)
            {
                float distance = Vector2.Distance(planets[i].GetPosition(), planets[j].GetPosition());
                maxDistance = Mathf.Max(maxDistance, distance);
            }
        }
        return Mathf.Clamp(maxDistance + zoomPadding, minZoom, maxZoom);
    }
}
