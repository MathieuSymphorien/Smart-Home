using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        // Charge la scène principale 
        SceneManager.LoadScene("GameScene"); // 
    }

    public void QuitGame()
    {
        // Quitte le jeu
        Debug.Log("Quitter le jeu !");
        Application.Quit();
    }
}
