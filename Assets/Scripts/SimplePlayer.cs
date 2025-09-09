using UnityEngine;
using System.Collections;

public class SimplePlayer : MonoBehaviour
{
    [Header("Player Stats")]
    public float health = 100f;
    public float speed = 10f;
    public float attackCooldown = 0.5f;
    public float attackRange = 15f;
    
    [Header("Projectile")]
    public GameObject projectilePrefab;
    
    private float maxHealth;
    private float lastAttackTime;
    private Transform nearestEnemy;
    
    void Start()
    {
        maxHealth = health;
        
        if (projectilePrefab == null)
            CreateSimpleProjectile();
            
        GetComponent<Renderer>().material.color = Color.blue;
        transform.localScale = Vector3.one * 2f;
        
    }
    
    void CreateSimpleProjectile()
    {
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "SimpleProjectile";
        projectile.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = projectile.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        renderer.material = mat;
        
        SimpleProjectile projScript = projectile.AddComponent<SimpleProjectile>();
        
        Rigidbody rb = projectile.AddComponent<Rigidbody>();
        rb.useGravity = false;
        
        SphereCollider collider = projectile.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        
        projectile.tag = "Projectile";
        projectile.SetActive(false);
        projectilePrefab = projectile;
    }
    
    void Update()
    {
        HandleMovement();
        HandleCombat();
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0, vertical) * speed * Time.deltaTime;
        transform.position += movement;
    }
    
    void HandleCombat()
    {
        FindNearestEnemy();
        
        if (nearestEnemy != null && Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }
    
    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        nearestEnemy = null;
        float closestDistance = attackRange;
        
        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
    }
    
    void Attack()
    {
        if (nearestEnemy == null || projectilePrefab == null) return;
        
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.SetActive(true);
        
        SimpleProjectile projScript = projectile.GetComponent<SimpleProjectile>();
        if (projScript != null)
        {
            Vector3 direction = (nearestEnemy.position - spawnPos).normalized;
            projScript.Initialize(direction);
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
        
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.2f);
        renderer.material.color = originalColor;
    }
    
    void Die()
    {
        Time.timeScale = 0f;
    }
    
    void OnGUI()
    {
        float healthPercent = health / maxHealth;
        float barWidth = 200f;
        float barHeight = 20f;
        
        Rect bgRect = new Rect(10, 10, barWidth, barHeight);
        Rect healthRect = new Rect(10, 10, barWidth * healthPercent, barHeight);
        
        GUI.color = Color.black;
        GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
        
        GUI.color = Color.Lerp(Color.red, Color.green, healthPercent);
        GUI.DrawTexture(healthRect, Texture2D.whiteTexture);
        
        GUI.color = Color.white;
        GUI.Label(new Rect(15, 12, 200, 20), $"Health: {health:F0}/{maxHealth:F0}");
        
        GUI.Label(new Rect(10, 40, 300, 20), "WASD: Move, Auto-attack nearest enemy");
        GUI.Label(new Rect(10, 60, 300, 20), "Space: Skip wave wait time");
    }
}