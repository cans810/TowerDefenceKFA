using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverTitle;
    public Button tryAgainButton;
    public Button mainMenuButton;
    
    private static GameOverManager instance;
    public static GameOverManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<GameOverManager>();
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        if (tryAgainButton != null)
            tryAgainButton.onClick.AddListener(TryAgain);
            
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(MainMenu);
    }
    
    public void ShowGameOver()
    {
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        TowerDefencePlayer player = FindObjectOfType<TowerDefencePlayer>();
        if (player != null)
        {
            player.gameObject.SetActive(false);
        }
    }
    
    public void TryAgain()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void MainMenu()
    {
        
        SceneManager.LoadScene(0);
    }
}