using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SidePanelController : MonoBehaviour
{
    public RectTransform panel; // Assign in Inspector
    public float hiddenX;       // Off-screen position
    public float visibleX;      // On-screen position
    public float slideSpeed = 5f;
    private bool isOpen = false;

    public void TogglePanel()
    {
        isOpen = !isOpen;
        StopAllCoroutines();
        StartCoroutine(MovePanel(isOpen ? visibleX : hiddenX));
    }

    private System.Collections.IEnumerator MovePanel(float targetX)
    {
        while (Mathf.Abs(panel.anchoredPosition.x - targetX) > 0.1f)
        {
            panel.anchoredPosition = new Vector2(
                Mathf.Lerp(panel.anchoredPosition.x, targetX, Time.deltaTime * slideSpeed),
                panel.anchoredPosition.y
            );
            yield return null;
        }
        panel.anchoredPosition = new Vector2(targetX, panel.anchoredPosition.y);
    }
}