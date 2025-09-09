using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<UIManager>();
            return instance;
        }
    }
    
    private TextMeshProUGUI healthText;
    private Image healthFillImage;
    private TextMeshProUGUI waveText;
    private TextMeshProUGUI controlsText;
    private TextMeshProUGUI statusText;
    
    void Awake()
    {
        instance = this;
        FindUIElements();
    }
    
    void FindUIElements()
    {
        Transform healthTextTrans = transform.Find("PlayerHealthBar/HealthText");
        if (healthTextTrans != null)
            healthText = healthTextTrans.GetComponent<TextMeshProUGUI>();
            
        Transform healthFillTrans = transform.Find("PlayerHealthBar/HealthFill");
        if (healthFillTrans != null)
            healthFillImage = healthFillTrans.GetComponent<Image>();
            
        Transform waveTextTrans = transform.Find("WaveInfo/WaveText");
        if (waveTextTrans != null)
            waveText = waveTextTrans.GetComponent<TextMeshProUGUI>();
            
        Transform controlsTextTrans = transform.Find("ControlsInfo/ControlsText");
        if (controlsTextTrans != null)
            controlsText = controlsTextTrans.GetComponent<TextMeshProUGUI>();
            
        Transform statusTextTrans = transform.Find("SetupStatus/StatusText");
        if (statusTextTrans != null)
            statusText = statusTextTrans.GetComponent<TextMeshProUGUI>();
    }
    
    public void UpdatePlayerHealth(int current, int max)
    {
        if (healthText != null)
            healthText.text = $"Health: {current}/{max}";
            
        if (healthFillImage != null)
        {
            float percent = (float)current / max;
            healthFillImage.rectTransform.anchorMax = new Vector2(percent, 1);
            healthFillImage.color = Color.Lerp(Color.red, Color.green, percent);
        }
    }
    
    public void UpdateWaveInfo(int currentWave, int totalWaves, int enemiesAlive, bool isSpawning, bool isBossWave)
    {
        if (waveText != null)
        {
            string info = $"Wave: {currentWave}/{totalWaves}\nEnemies Alive: {enemiesAlive}";
            if (isSpawning)
                info += "\nSpawning enemies...";
            if (isBossWave)
                info += "\n<color=red>BOSS WAVE!</color>";
            waveText.text = info;
        }
    }
    
    public void UpdateControlsText(string text)
    {
        if (controlsText != null)
            controlsText.text = text;
    }
}