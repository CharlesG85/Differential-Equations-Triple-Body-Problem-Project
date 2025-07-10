using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    public static event Action OnSimulationStarted;
    public static event Action OnSimulationPaused;
    public static event Action OnSimulationResumed;

    private bool isPaused = false;
    private bool isSimulationStarted = false;

    private void Start()
    {
        isPaused = false;
        isSimulationStarted = false;
    }

    private void OnDestroy()
    {
        ResetEvents();
    }

    public static void ResetEvents()
    {
        OnSimulationStarted = delegate { };
        OnSimulationPaused = delegate { };
        OnSimulationResumed = delegate { };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Detects key press ONCE
        {
            Debug.Log(isSimulationStarted);

            if (isSimulationStarted == false)
            {
                OnSimulationStarted?.Invoke();
                isSimulationStarted = true;
                return;
            }

            isPaused = !isPaused; // Toggle state

            if (isPaused)
            {
                OnSimulationPaused?.Invoke();
                Debug.Log("Simulation Paused!");
            }
            else
            {
                OnSimulationResumed?.Invoke();
                Debug.Log("Simulation Resumed!");
            }
        }

        if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.R)) // Check if Enter is pressed
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload current scene
        }
    }
}
