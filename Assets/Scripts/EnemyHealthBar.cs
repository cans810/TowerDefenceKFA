using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Components")]
    public Canvas healthCanvas;
    public Image healthFillImage;
    public Image healthBackgroundImage;
    
    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 0.8f, 0);
    public float barWidth = 1f;
    public float barHeight = 0.15f;
    
    private Camera mainCamera;
    private TowerDefenceEnemy enemyScript;
    
    void Start()
    {
        mainCamera = Camera.main;
        enemyScript = GetComponentInParent<TowerDefenceEnemy>();
        CreateHealthBar();
    }
    
    void CreateHealthBar()
    {
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        
        if (transform.parent != null)
        {
            Vector3 enemyPos = transform.parent.position;
            canvasObj.transform.position = new Vector3(enemyPos.x, enemyPos.y + offset.y, enemyPos.z);
        }
        else
        {
            canvasObj.transform.localPosition = offset;
        }
        
        healthCanvas = canvasObj.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        healthCanvas.worldCamera = mainCamera;
        
        RectTransform canvasRect = healthCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1.5f, 0.15f);
        canvasObj.transform.localScale = new Vector3(0.0000003f, 0.0000003f, 0.0000003f);
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.scaleFactor = 1;
        
        GameObject bgObj = new GameObject("HealthBackground");
        bgObj.transform.SetParent(canvasObj.transform);
        healthBackgroundImage = bgObj.AddComponent<Image>();
        healthBackgroundImage.color = Color.black;
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;
        
        GameObject fillObj = new GameObject("HealthFill");
        fillObj.transform.SetParent(canvasObj.transform);
        healthFillImage = fillObj.AddComponent<Image>();
        healthFillImage.color = Color.green;
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;
    }
    
    void Update()
    {
        UpdateHealthBar();
        FaceCamera();
        UpdatePosition();
    }
    
    void UpdatePosition()
    {
        if (healthCanvas != null && transform.parent != null)
        {
            Vector3 enemyPos = transform.parent.position;
            healthCanvas.transform.position = new Vector3(enemyPos.x, enemyPos.y + offset.y, enemyPos.z);
        }
    }
    
    void UpdateHealthBar()
    {
        if (enemyScript != null && healthFillImage != null)
        {
            float healthPercent = (float)enemyScript.GetCurrentHealth() / enemyScript.GetMaxHealth();
            
            healthFillImage.rectTransform.anchorMax = new Vector2(healthPercent, 1);
            
            healthFillImage.color = Color.Lerp(Color.red, Color.green, healthPercent);
        }
    }
    
    void FaceCamera()
    {
        if (mainCamera != null && healthCanvas != null)
        {
            healthCanvas.transform.LookAt(healthCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                        mainCamera.transform.rotation * Vector3.up);
        }
    }
    
    void OnDestroy()
    {
        if (healthCanvas != null && healthCanvas.gameObject != null)
        {
            Destroy(healthCanvas.gameObject);
        }
    }
}