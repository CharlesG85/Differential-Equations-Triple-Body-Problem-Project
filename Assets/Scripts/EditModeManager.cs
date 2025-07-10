using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EditModeManager : MonoBehaviour
{
    public PhysicsManager physicsManager;

    public Camera cam;
    public Slider massSlider;
    public Slider speedMultiplierSlider;
    public TMP_InputField massInputField;
    public TMP_Text massText;
    public TMP_Text speedMultiplierText;
    public TMP_InputField XVel;
    public TMP_InputField YVel;
    public TMP_InputField XPos;
    public TMP_InputField YPos;
    public GameObject settingsPanel; // UI Panel for planet settings
    public LineRenderer velocityArrow; // LineRenderer for velocity visualization
    public float maxMass = 20f;
    public float minMass = 0.01f;
    public float minSpeed = -5f;
    public float maxSpeed = 5f;



    private PhysicsBody selectedPlanet;
    private bool isEditing = true;
    private bool isDragging = false;
    private bool isSettingVelocity = false;
    private Vector2 dragOffset;
    private Vector2 velocityStartPos;

    void Start()
    {
        GameManager.OnSimulationStarted += DisableEditing;
        settingsPanel.SetActive(false);
        velocityArrow.gameObject.SetActive(false); // Hide velocity arrow initially
    }

    void Update()
    {
        if (!isEditing) return;

        if (Input.GetMouseButtonDown(0)) // Left click
        {
            if (!IsPointerOverUI())
            {
                if (Input.GetKey(KeyCode.LeftShift)) // Shift + Click → Start Velocity Editing
                {
                    StartSettingVelocity();
                }
                else // Normal Click → Select Planet
                {
                    SelectPlanet();
                }
            }
        }
        else if (Input.GetMouseButton(0) && selectedPlanet && isDragging) // Dragging a planet
        {
            MovePlanet();
        }
        else if (Input.GetMouseButton(0) && selectedPlanet && isSettingVelocity) // Dragging to set velocity
        {
            UpdateVelocity();
        }
        else if (Input.GetMouseButtonUp(0)) // Release Left Click
        {
            isDragging = false;
            isSettingVelocity = false;
            velocityArrow.gameObject.SetActive(false);
        }
    }

    private void SelectPlanet()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null)
        {
            selectedPlanet = hit.GetComponent<PhysicsBody>();
            if (selectedPlanet)
            {
                settingsPanel.SetActive(true);

                // Set offset for dragging
                dragOffset = (Vector2)selectedPlanet.transform.position - mousePos;
                isDragging = true;

                // Update Velocity Text
                float xVel = selectedPlanet.initialVelocity.x;
                float yVel = selectedPlanet.initialVelocity.y;

                // Update Mass Text
                massSlider.value = (selectedPlanet.mass - minMass) / (maxMass - minMass);
                massText.text = $"Mass: {selectedPlanet.mass:F2}";

                XVel.text = xVel.ToString();
                YVel.text = yVel.ToString();
            }
        }
        else
        {
            selectedPlanet = null;
            settingsPanel.SetActive(false);
        }
    }

    private void MovePlanet()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        selectedPlanet.transform.position = mousePos + dragOffset;

        AdjustTextFromPosition();
    }

    private void StartSettingVelocity()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit != null)
        {
            selectedPlanet = hit.GetComponent<PhysicsBody>();
            if (selectedPlanet)
            {
                isSettingVelocity = true;
                velocityStartPos = selectedPlanet.transform.position;
                velocityArrow.gameObject.SetActive(true);
                velocityArrow.SetPosition(0, velocityStartPos);
                velocityArrow.SetPosition(1, velocityStartPos);
            }
        }
    }

    private void UpdateVelocity()
    {
        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 newVelocity = (mousePos - velocityStartPos) * 2f; // Scale velocity for better control
        selectedPlanet.SetInitialVelocity(newVelocity);

        // Update velocity arrow in real-time
        velocityArrow.SetPosition(1, mousePos);

        AdjustTextFromVelocity();
    }

    public void AdjustMassSlider()
    {
        if (selectedPlanet)
        {
            float newMass = massSlider.value * (maxMass - minMass) + minMass;
            selectedPlanet.mass = newMass;
            float radius = Mathf.Sqrt(newMass);
            selectedPlanet.transform.localScale = new Vector3(radius, radius, 1) * 0.3f;
            massText.text = $"Mass: {newMass:F2}";
        }
    }

    public void AdjustSpeedMultiplierSlider()
    {
        float newSpeed = speedMultiplierSlider.value * (maxSpeed - minSpeed) + minSpeed;
        physicsManager.speedMultiplier = newSpeed;
        speedMultiplierText.text = $"Multiplier: {newSpeed:F2}";
    }

    public void AdjustMassInput()
    {
        if (selectedPlanet)
        {
            float newMass = Mathf.Clamp(float.Parse(massInputField.text), minMass, maxMass);
            massSlider.value = (newMass - minMass) / (maxMass - minMass);
            selectedPlanet.mass = newMass;
            float radius = Mathf.Sqrt(newMass);
            selectedPlanet.transform.localScale = new Vector3(radius, radius, 1) * 0.3f;
            massText.text = $"Mass: {newMass:F2}";
        }
    }

    public void ClampMassText()
    {
        float newMass = Mathf.Clamp(float.Parse(massInputField.text), minMass, maxMass);
        massInputField.text = newMass.ToString();
    }

    public void AdjustTextFromVelocity()
    {
        if (selectedPlanet)
        {
            float xVel = selectedPlanet.initialVelocity.x;
            float yVel = selectedPlanet.initialVelocity.y;

            XVel.text = xVel.ToString();
            YVel.text = yVel.ToString();
        }
    }

    public void AdjustTextFromPosition()
    {
        if (selectedPlanet)
        {
            float xPos = selectedPlanet.GetPosition().x;
            float yPos = selectedPlanet.GetPosition().y;

            XPos.text = xPos.ToString();
            YPos.text = yPos.ToString();
        }
    }

    public void AdjustVelocityFromText()
    {
        if (selectedPlanet)
        {
            float xValue;
            float yValue;

            Vector2 newVelocity = new Vector2(0, 0);

            if (float.TryParse(XVel.text, out xValue))
            {
                newVelocity.x = xValue;
            }

            if (float.TryParse(YVel.text, out yValue))
            {
                newVelocity.y = yValue;
            }

            selectedPlanet.SetInitialVelocity(newVelocity);
        }
    }

    public void AdjustPositionFromText()
    {
        if (selectedPlanet)
        {
            float xValue;
            float yValue;

            Vector2 newPosition = new Vector2(0, 0);

            if (float.TryParse(XPos.text, out xValue))
            {
                newPosition.x = xValue;
            }

            if (float.TryParse(YPos.text, out yValue))
            {
                newPosition.y = yValue;
            }

            selectedPlanet.SetPosition(newPosition);
        }
    }

    private void DisableEditing()
    {
        settingsPanel.SetActive(false);
        isEditing = false;
        this.enabled = false;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
