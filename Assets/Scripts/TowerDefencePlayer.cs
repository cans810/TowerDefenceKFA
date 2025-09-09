using UnityEngine;
using System.Collections;

public class TowerDefencePlayer : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    
    [Header("Combat")]
    public float attackRange = 12f;
    public float attackCooldown = 0.4f;
    public int weaponDamage = 30;
    public GameObject projectilePrefab;
    
    [Header("Health & I-Frames")]
    public int maxHealth = 100;
    public float iFrameDuration = 1f;
    public float respawnIFrameDuration = 2f;
    
    [Header("Weapons (Extra)")]
    public WeaponType currentWeapon = WeaponType.BasicShot;
    public float basicShotCooldown = 0.3f;
    public float laserCooldown = 0.6f;
    public float rocketCooldown = 1.2f;
    
    private int currentHealth;
    private float lastAttackTime;
    private bool inIFrames;
    private Transform nearestEnemy;
    private Rigidbody rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 respawnPosition;
    
    private Collider playerCollider;
    
    public enum WeaponType
    {
        BasicShot,
        Laser,
        Rocket
    }
    
    void Start()
    {
        InitializePlayer();
    }
    
    void InitializePlayer()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        playerCollider = GetComponent<Collider>();
        respawnPosition = transform.position;
        
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        
        if (projectilePrefab == null)
            CreateProjectilePrefab();
        
    }
    
    void CreateProjectilePrefab()
    {
        #if UNITY_EDITOR
        projectilePrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectiles/TowerDefenceProjectileBasic.prefab");
        
        if (projectilePrefab != null)
        {
            return;
        }
        #endif
        
        GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        projectile.name = "Projectile";
        projectile.transform.localScale = Vector3.one * 0.3f;
        
        Renderer renderer = projectile.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Glossiness", 0.8f);
        renderer.material = mat;
        
        projectile.AddComponent<TowerDefenceProjectile>();
        Rigidbody projRb = projectile.AddComponent<Rigidbody>();
        projRb.useGravity = false;
        
        SphereCollider collider = projectile.GetComponent<SphereCollider>();
        collider.isTrigger = true;
        
        projectile.tag = "Projectile";
        projectile.SetActive(false);
        projectilePrefab = projectile;
    }
    
    void Update()
    {
        HandleMovement();
        HandleWeaponSwitching();
        HandleCombat();
        
        UIManager.Instance?.UpdatePlayerHealth(currentHealth, maxHealth);
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0, vertical).normalized * moveSpeed;
        
        if (rb != null)
        {
            rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        }
        else
        {
            transform.position += movement * Time.deltaTime;
        }
    }
    
    void HandleWeaponSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SwitchWeapon(WeaponType.BasicShot);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SwitchWeapon(WeaponType.Laser);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SwitchWeapon(WeaponType.Rocket);
        
        if (Input.GetKeyDown(KeyCode.Q))
            CycleWeapon(-1);
        else if (Input.GetKeyDown(KeyCode.E))
            CycleWeapon(1);
    }
    
    void SwitchWeapon(WeaponType newWeapon)
    {
        if (currentWeapon != newWeapon)
        {
            currentWeapon = newWeapon;
        }
    }
    
    void CycleWeapon(int direction)
    {
        int weaponCount = System.Enum.GetValues(typeof(WeaponType)).Length;
        int currentIndex = (int)currentWeapon;
        int newIndex = (currentIndex + direction + weaponCount) % weaponCount;
        SwitchWeapon((WeaponType)newIndex);
    }
    
    void HandleCombat()
    {
        FindNearestEnemy();
        
        if (nearestEnemy != null && CanAttack())
        {
            Attack();
        }
    }
    
    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        nearestEnemy = null;
        float closestDistance = attackRange;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }
    }
    
    bool CanAttack()
    {
        float cooldown = GetCurrentWeaponCooldown();
        return Time.time - lastAttackTime >= cooldown;
    }
    
    float GetCurrentWeaponCooldown()
    {
        switch (currentWeapon)
        {
            case WeaponType.BasicShot: return basicShotCooldown;
            case WeaponType.Laser: return laserCooldown;
            case WeaponType.Rocket: return rocketCooldown;
            default: return attackCooldown;
        }
    }
    
    void Attack()
    {
        if (nearestEnemy == null || projectilePrefab == null) return;
        
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        Vector3 direction = (nearestEnemy.position - spawnPos).normalized;
        
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.SetActive(true);
        
        TowerDefenceProjectile projScript = projectile.GetComponent<TowerDefenceProjectile>();
        if (projScript != null)
        {
            projScript.Initialize(direction, weaponDamage, currentWeapon);
        }
        
        lastAttackTime = Time.time;
    }
    
    public void TakeDamage(int damage)
    {
        if (inIFrames) return;
        
        currentHealth -= damage;
        StartCoroutine(IFrameCoroutine(iFrameDuration));
        
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    void Die()
    {
        
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }
        else
        {
            StartCoroutine(RespawnCoroutine());
        }
    }
    
    IEnumerator RespawnCoroutine()
    {
        gameObject.SetActive(false);
        
        yield return new WaitForSeconds(1f);
        
        transform.position = respawnPosition;
        currentHealth = maxHealth;
        gameObject.SetActive(true);
        
        StartCoroutine(IFrameCoroutine(respawnIFrameDuration));
        
    }
    
    IEnumerator IFrameCoroutine(float duration)
    {
        inIFrames = true;
        
        float flashInterval = 0.1f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = spriteRenderer.color.a > 0.5f ? 
                    new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f) : 
                    originalColor;
            }
            
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        
        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
        
        inIFrames = false;
    }
    
    
    
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsInIFrames() => inIFrames;
    public WeaponType GetCurrentWeapon() => currentWeapon;
}