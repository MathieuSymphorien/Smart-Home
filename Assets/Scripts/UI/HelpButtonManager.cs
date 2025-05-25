
using UnityEngine;

public class HelpButtonManager : MonoBehaviour
{
    [Header("Canvas References")]
    [Tooltip("Canvas that contains the main menu UI elements.")]
    [SerializeField] private Canvas menuCanvas;

    [Tooltip("Canvas that contains the help page UI elements.")]
    [SerializeField] private Canvas helpCanvas;

    private void Awake()
    {
        // Ensure a clean initial state (Main Menu visible, Help hidden).
        ShowMenu();
    }

    /// <summary>
    /// Show the Help canvas and hide the Main Menu canvas.
    /// </summary>
    public void ShowHelp()
    {
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(false);
        if (helpCanvas != null) helpCanvas.gameObject.SetActive(true);
    }

    /// <summary>
    /// Show the Main Menu canvas and hide the Help canvas.
    /// </summary>
    public void ShowMenu()
    {
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(true);
        if (helpCanvas != null) helpCanvas.gameObject.SetActive(false);
    }

    /// <summary>
    /// Toggle between the two canvases. Useful if you want a single button to both open and close Help.
    /// </summary>
    public void ToggleHelp()
    {
        bool currentlyShowingHelp = helpCanvas != null && helpCanvas.gameObject.activeSelf;
        if (menuCanvas != null) menuCanvas.gameObject.SetActive(currentlyShowingHelp);
        if (helpCanvas != null) helpCanvas.gameObject.SetActive(!currentlyShowingHelp);
    }
}
