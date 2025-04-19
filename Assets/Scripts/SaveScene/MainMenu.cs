using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public SaveManager saveManager;
    public TMP_Dropdown savesDropdown;

    private void Start()
    {
        RefreshDropdown();
    }

    public void RefreshDropdown()
    {
        savesDropdown.ClearOptions();
        var saveNames = saveManager.GetAllSaveNames();
        savesDropdown.AddOptions(saveNames.ToList());
    }

     public void OnClickLoad()
    {
        // On récupère le nom de la sauvegarde sélectionnée
        int index = savesDropdown.value;
        string selectedSaveName = savesDropdown.options[index].text;

        // On le stocke dans SaveGameHolder
        SaveGameHolder.saveNameToLoad = selectedSaveName;

        // Et on charge la scène du jeu
        SceneManager.LoadScene("GameScene");
    }
}
