using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenManager : MonoBehaviour
{
    // Name of main game scene
    public string mainSceneName = "Main";

    public void StartGame()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    // Optional: Quit button
    public void QuitGame()
    {
        Application.Quit();
    }
}
