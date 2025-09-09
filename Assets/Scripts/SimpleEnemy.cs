using UnityEngine;

public class SimpleEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float health = 100f;
    public float speed = 5f;
    public float damage = 10f;
    
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private SimpleWaveSpawner spawner;
    private float maxHealth;
    
    void Start()
    {
        maxHealth = health;
    }
    
    public void Initialize(Transform[] waypointPath, SimpleWaveSpawner waveSpawner)
    {
        waypoints = waypointPath;
        spawner = waveSpawner;
        currentWaypointIndex = 0;
    }
    
    void Update()
    {
        MoveAlongPath();
    }
    
    void MoveAlongPath()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachEnd();
            return;
        }
        
        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;
        
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance < 0.5f)
        {
            currentWaypointIndex++;
        }
    }
    
    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        
        StartCoroutine(FlashRed());
        
        if (health <= 0)
        {
            Die();
        }
    }
    
    System.Collections.IEnumerator FlashRed()
    {
        Renderer renderer = GetComponent<Renderer>();
        Color originalColor = renderer.material.color;
        
        renderer.material.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        renderer.material.color = originalColor;
    }
    
    void Die()
    {
        if (spawner != null)
            spawner.OnEnemyDied();
            
        Destroy(gameObject);
    }
    
    void ReachEnd()
    {
        if (spawner != null)
            spawner.OnEnemyReachedEnd();
            
        Destroy(gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            SimpleProjectile projectile = other.GetComponent<SimpleProjectile>();
            if (projectile != null)
            {
                TakeDamage(projectile.damage);
                Destroy(other.gameObject);
            }
        }
        
        if (other.CompareTag("Player"))
        {
            SimplePlayer player = other.GetComponent<SimplePlayer>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
    
    void OnGUI()
    {
        if (Camera.main == null) return;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);
        if (screenPos.z > 0)
        {
            screenPos.y = Screen.height - screenPos.y;
            
            float healthPercent = health / maxHealth;
            float barWidth = 50f;
            float barHeight = 5f;
            
            Rect bgRect = new Rect(screenPos.x - barWidth/2, screenPos.y - barHeight/2, barWidth, barHeight);
            Rect healthRect = new Rect(screenPos.x - barWidth/2, screenPos.y - barHeight/2, barWidth * healthPercent, barHeight);
            
            GUI.color = Color.black;
            GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
            
            GUI.color = Color.Lerp(Color.red, Color.green, healthPercent);
            GUI.DrawTexture(healthRect, Texture2D.whiteTexture);
            
            GUI.color = Color.white;
        }
    }
}