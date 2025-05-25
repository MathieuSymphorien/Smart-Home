using UnityEngine;
using TMPro;                    // ou UnityEngine.UI si vous restez en <Text>

public class HelpPopup : MonoBehaviour
{
    [Header("Références UI")]
    [SerializeField] private GameObject panel;   // Panel ou CanvasGroup de la pop-up
    [SerializeField] private TMP_Text   message; // Le champ texte


    private void Awake()
    {
        if (panel != null) panel.SetActive(false);   // caché au départ
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (panel != null)
                panel.SetActive(!panel.activeSelf); // toggle visibilité
        }
    }
}