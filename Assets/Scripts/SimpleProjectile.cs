using UnityEngine;

public class SimpleProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 20f;
    public float damage = 25f;
    public float lifetime = 5f;
    
    private Vector3 direction;
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }
    
    public void Initialize(Vector3 targetDirection)
    {
        direction = targetDirection.normalized;
        
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }
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
            Destroy(gameObject);
        }
        
        if (other.CompareTag("Player"))
            return;
            
        if (!other.CompareTag("Projectile") && !other.name.Contains("Marker") && !other.name.Contains("Waypoint"))
        {
            Destroy(gameObject);
        }
    }
}