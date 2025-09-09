using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button playButton;
    public Button quitButton;
    
    void Start()
    {
        if (playButton != null)
            playButton.onClick.AddListener(PlayGame);
            
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }
    
    public void PlayGame()
    {
        int gameSceneIndex = SceneManager.GetActiveScene().buildIndex == 0 ? 1 : 0;
        SceneManager.LoadScene(gameSceneIndex);
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}