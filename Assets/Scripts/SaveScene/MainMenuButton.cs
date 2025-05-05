using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenu : MonoBehaviour
{
    public SaveManager saveManager;
    public TMP_Dropdown savesDropdown;
    public TMP_InputField newGameNameField; 
    public TMP_Dropdown importsDropdown;

    private void Start()
    {
        RefreshDropdown();
        RefreshImports(); 
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

    public void OnClickDelete()
    {
        if (savesDropdown.options.Count == 0)                     // rien à supprimer
            return;

        int idx = savesDropdown.value;
        string nameToDelete = savesDropdown.options[idx].text;

        if (saveManager.DeleteSave(nameToDelete))
            RefreshDropdown();                                    // MAJ UI
    }


    public void OnClickCreateGame()
    {
        string saveName = newGameNameField.text.Trim();
        if (string.IsNullOrEmpty(saveName)) return;

        // crée un fichier vide pour réserver le nom
        // saveManager.CreateNewSave(saveName);       
        saveManager.CreateEmptySave(saveName);
        SaveGameHolder.saveNameToLoad = saveName;

        SceneManager.LoadScene("GameScene");           // et on lance la scène
    }

    /* ---------- Export ---------- */
public void OnClickExport()
{
    int idx = savesDropdown.value;
    string name = savesDropdown.options[idx].text;
    saveManager.ExportSaveBare(name);
}

/* ---------- Import ---------- */
public void OnClickImport()
{
    int idx = importsDropdown.value;
    string fileName = importsDropdown.options[idx].text;
    if (saveManager.ImportSaveBare(fileName))
    {
        RefreshDropdown();   // maj des parties
        RefreshImports();    // retire éventuellement le fichier d’Imports si vous le déplacez
    }
}
public void RefreshImports()
{
    importsDropdown.ClearOptions();
    string[] files = Directory.GetFiles(SaveManager.ImportsDir, "*.json")
                              .Select(Path.GetFileName)  // on affiche juste le nom
                              .ToArray();
    importsDropdown.AddOptions(files.ToList());
}

}
