using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SaveButton : MonoBehaviour
{
    public TMP_InputField saveNameField; // champ texte UI pour entrer le nom de la sauvegarde
    public SaveManager saveManager;

    public void OnClickSave()
    {
        string saveName = saveNameField.text;
        if (!string.IsNullOrEmpty(saveName))
        {
            saveManager.CreateNewSave(saveName);
            Debug.Log("Sauvegarde créée sous le nom: " + saveName);
        }
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}