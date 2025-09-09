using UnityEngine;

public class TowerDefenceProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 15f;
    public int damage = 30;
    public float lifetime = 3f;
    
    private Vector3 direction;
    private TowerDefencePlayer.WeaponType weaponType;
    private Rigidbody rb;
    private bool hasHitTarget;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }
    
    public void Initialize(Vector3 targetDirection, int weaponDamage, TowerDefencePlayer.WeaponType weapon)
    {
        direction = targetDirection.normalized;
        damage = weaponDamage;
        weaponType = weapon;
        
        ConfigureWeaponProperties();
        
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
    }
    
    void ConfigureWeaponProperties()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material mat = renderer.material;
        
        switch (weaponType)
        {
            case TowerDefencePlayer.WeaponType.BasicShot:
                speed = 20f;
                mat.color = Color.yellow;
                transform.localScale = Vector3.one * 0.3f;
                break;
                
            case TowerDefencePlayer.WeaponType.Laser:
                speed = 15f;
                mat.color = Color.cyan;
                transform.localScale = Vector3.one * 0.2f;
                lifetime = 2f; // Shorter range for balance
                break;
                
            case TowerDefencePlayer.WeaponType.Rocket:
                speed = 8f;
                mat.color = Color.red;
                transform.localScale = Vector3.one * 0.5f;
                damage = (int)(damage * 1.5f); // 50% more damage
                break;
        }
        
        renderer.material = mat;
    }
    
    void Update()
    {
        if (rb == null || rb.velocity.magnitude < speed * 0.5f)
        {
            transform.position += direction * speed * Time.deltaTime;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            TowerDefenceEnemy enemy = other.GetComponent<TowerDefenceEnemy>();
            if (enemy != null && !hasHitTarget)
            {
                enemy.TakeDamage(damage);
                
                HandleWeaponEffects(other);
                
                if (weaponType != TowerDefencePlayer.WeaponType.Laser)
                {
                    hasHitTarget = true;
                    Destroy(gameObject);
                }
            }
        }
        
        if (other.CompareTag("Player"))
            return;
            
        if (other.gameObject.layer != gameObject.layer && 
            !other.name.Contains("Waypoint") && 
            !other.name.Contains("Marker") &&
            !other.CompareTag("Projectile"))
        {
            Destroy(gameObject);
        }
    }
    
    void HandleWeaponEffects(Collider hitTarget)
    {
        switch (weaponType)
        {
            case TowerDefencePlayer.WeaponType.Rocket:
                HandleRocketExplosion(hitTarget.transform.position);
                break;
                
            case TowerDefencePlayer.WeaponType.Laser:
                break;
                
            case TowerDefencePlayer.WeaponType.BasicShot:
                break;
        }
    }
    
    void HandleRocketExplosion(Vector3 explosionCenter)
    {
        float explosionRadius = 3f;
        int explosionDamage = damage / 2; // Reduced AoE damage
        
        Collider[] enemiesInRange = Physics.OverlapSphere(explosionCenter, explosionRadius);
        
        foreach (Collider enemyCollider in enemiesInRange)
        {
            if (enemyCollider.CompareTag("Enemy") && enemyCollider.transform != transform)
            {
                TowerDefenceEnemy enemy = enemyCollider.GetComponent<TowerDefenceEnemy>();
                if (enemy != null)
                {
                    float distance = Vector3.Distance(explosionCenter, enemyCollider.transform.position);
                    float damageMultiplier = 1f - (distance / explosionRadius);
                    int finalDamage = Mathf.RoundToInt(explosionDamage * damageMultiplier);
                    
                    enemy.TakeDamage(finalDamage);
                }
            }
        }
        
        CreateExplosionEffect(explosionCenter);
    }
    
    void CreateExplosionEffect(Vector3 center)
    {
        for (int i = 0; i < 8; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            particle.name = "ExplosionParticle";
            particle.transform.position = center;
            particle.transform.localScale = Vector3.one * 0.2f;
            
            Renderer renderer = particle.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 0.5f, 0f, 0.8f); // Orange
            renderer.material = mat;
            
            Vector3 randomDirection = Random.insideUnitSphere;
            randomDirection.y = Mathf.Abs(randomDirection.y); // Keep particles above ground
            
            Rigidbody particleRb = particle.AddComponent<Rigidbody>();
            particleRb.velocity = randomDirection * Random.Range(5f, 10f);
            particleRb.useGravity = true;
            
            Destroy(particle, Random.Range(0.5f, 1.5f));
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (weaponType == TowerDefencePlayer.WeaponType.Rocket)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 3f);
        }
    }
}