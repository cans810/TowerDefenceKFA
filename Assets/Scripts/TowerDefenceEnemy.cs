using UnityEngine;
using System.Collections;

public class TowerDefenceEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public EnemyType enemyType = EnemyType.Basic;
    public int maxHealth = 100;
    public float moveSpeed = 3f;
    public int damage = 10;
    
    [Header("Path Following")]
    public Transform[] waypoints;
    public float waypointThreshold = 0.8f;
    
    private int currentHealth;
    private int currentWaypointIndex = 0;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private TowerDefenceWaveManager waveManager;
    
    public enum EnemyType
    {
        Basic,     
        Fast,       
        Tank,       
        Elite,    
        Boss        
    }
    
    private static readonly EnemyStats[] enemyStatsTable = {
        new EnemyStats { health = 80,  speed = 4f,  damage = 15, color = Color.red },      
        new EnemyStats { health = 40,  speed = 8f,  damage = 10, color = Color.blue },    
        new EnemyStats { health = 200, speed = 2f,  damage = 25, color = Color.green },   
        new EnemyStats { health = 120, speed = 5f,  damage = 30, color = Color.magenta }, 
        new EnemyStats { health = 500, speed = 3f,  damage = 50, color = Color.yellow }    
    };
    
    [System.Serializable]
    public struct EnemyStats
    {
        public int health;
        public float speed;
        public int damage;
        public Color color;
    }
    
    void Start()
    {
        InitializeEnemy();
    }
    
    void InitializeEnemy()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ApplyEnemyTypeStats();
        
        currentHealth = maxHealth;
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        
    }
    
    void ApplyEnemyTypeStats()
    {
        EnemyStats stats = enemyStatsTable[(int)enemyType];
        
        maxHealth = stats.health;
        moveSpeed = stats.speed;
        damage = stats.damage;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = stats.color;
        }
        else
        {
            Renderer meshRenderer = GetComponent<Renderer>();
            if (meshRenderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = stats.color;
                meshRenderer.material = mat;
            }
        }
        
        if (enemyType == EnemyType.Boss)
        {
            transform.localScale *= 1.5f;
        }
    }
    
    public void SetEnemyType(EnemyType type)
    {
        enemyType = type;
        ApplyEnemyTypeStats();
    }
    
    public void SetWaypoints(Transform[] pathWaypoints, TowerDefenceWaveManager manager)
    {
        waypoints = pathWaypoints;
        waveManager = manager;
        currentWaypointIndex = 0;
    }
    
    void Update()
    {
        FollowPath();
    }
    
    void FollowPath()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        
        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachEndOfPath();
            return;
        }
        
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null) return;
        
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
        
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distance < waypointThreshold)
        {
            currentWaypointIndex++;
        }
    }
    
    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        
        if (spriteRenderer != null)
            StartCoroutine(FlashWhite());
        
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    IEnumerator FlashWhite()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = originalColor;
        }
    }
    
    void Die()
    {
        if (waveManager != null)
            waveManager.OnEnemyKilled(this);
        
        if (enemyType == EnemyType.Boss)
        {
            CreateBossDeathEffect();
        }
        
        Destroy(gameObject);
    }
    
    void CreateBossDeathEffect()
    {
        for (int i = 0; i < 15; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.name = "BossDeathParticle";
            particle.transform.position = transform.position + Vector3.up;
            particle.transform.localScale = Vector3.one * 0.3f;
            
            Renderer renderer = particle.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 1f, 0f, 0.8f); 
            renderer.material = mat;
            
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = Mathf.Abs(randomDirection.y);
            
            Rigidbody particleRb = particle.AddComponent<Rigidbody>();
            particleRb.velocity = randomDirection * Random.Range(8f, 15f);
            particleRb.useGravity = true;
            
            Destroy(particle, Random.Range(1f, 3f));
        }
    }
    
    void ReachEndOfPath()
    {
        TowerDefencePlayer player = FindObjectOfType<TowerDefencePlayer>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
        
        if (waveManager != null)
            waveManager.OnEnemyReachedEnd(this);
        
        Destroy(gameObject);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TowerDefencePlayer player = other.GetComponent<TowerDefencePlayer>();
            if (player != null && !player.IsInIFrames())
            {
                player.TakeDamage(damage);
            }
        }
    }
    
    
    public EnemyType GetEnemyType() => enemyType;
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public float GetMoveSpeed() => moveSpeed;
    public int GetDamage() => damage;
}