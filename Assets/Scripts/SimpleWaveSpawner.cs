using UnityEngine;
using System.Collections;

public class SimpleWaveSpawner : MonoBehaviour
{
    [Header("Wave Settings")]
    public int enemiesPerWave = 10;
    public float spawnDelay = 1f;
    public float timeBetweenWaves = 5f;
    
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform[] waypoints;
    
    private int currentWave = 1;
    private int enemiesAlive = 0;
    private bool isSpawning = false;
    
    void Start()
    {
        if (enemyPrefab == null)
            CreateSimpleEnemyPrefab();
            
        if (spawnPoint == null)
            CreateSpawnPoint();
            
        if (waypoints == null || waypoints.Length == 0)
            CreateSimpleWaypoints();
            
        StartCoroutine(WaveLoop());
    }
    
    void CreateSimpleEnemyPrefab()
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = "SimpleEnemy";
        enemy.transform.localScale = Vector3.one * 2f;
        
        Renderer renderer = enemy.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        renderer.material = mat;
        
        SimpleEnemy enemyScript = enemy.AddComponent<SimpleEnemy>();
        
        Rigidbody rb = enemy.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        
        BoxCollider collider = enemy.GetComponent<BoxCollider>();
        collider.isTrigger = true;
        
        enemy.tag = "Enemy";
        enemy.SetActive(false);
        enemyPrefab = enemy;
        
    }
    
    void CreateSpawnPoint()
    {
        GameObject spawn = new GameObject("SpawnPoint");
        spawn.transform.position = new Vector3(-20, 1, 0);
        spawnPoint = spawn.transform;
        
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "SpawnMarker";
        marker.transform.SetParent(spawn.transform);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = marker.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.green;
        renderer.material = mat;
        
    }
    
    void CreateSimpleWaypoints()
    {
        Vector3[] points = {
            new Vector3(-20, 1, 0),   // Start
            new Vector3(-10, 1, 0),
            new Vector3(-10, 1, 10),
            new Vector3(10, 1, 10),
            new Vector3(10, 1, -10),
            new Vector3(20, 1, -10)   // End
        };
        
        waypoints = new Transform[points.Length];
        GameObject waypointParent = new GameObject("Waypoints");
        
        for (int i = 0; i < points.Length; i++)
        {
            GameObject waypoint = new GameObject($"Waypoint_{i}");
            waypoint.transform.position = points[i];
            waypoint.transform.SetParent(waypointParent.transform);
            waypoints[i] = waypoint.transform;
            
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "WaypointMarker";
            marker.transform.SetParent(waypoint.transform);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = new Vector3(1, 0.1f, 1);
            
            Renderer renderer = marker.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.yellow;
            renderer.material = mat;
        }
        
    }
    
    IEnumerator WaveLoop()
    {
        while (true)
        {
            
            yield return StartCoroutine(SpawnWave());
            
            yield return new WaitUntil(() => enemiesAlive == 0);
            
            currentWave++;
            
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
    
    IEnumerator SpawnWave()
    {
        isSpawning = true;
        
        for (int i = 0; i < enemiesPerWave; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
        
        isSpawning = false;
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoint == null) return;
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
        enemy.SetActive(true);
        
        SimpleEnemy enemyScript = enemy.GetComponent<SimpleEnemy>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(waypoints, this);
        }
        
        enemiesAlive++;
    }
    
    public void OnEnemyDied()
    {
        enemiesAlive--;
    }
    
    public void OnEnemyReachedEnd()
    {
        enemiesAlive--;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isSpawning)
        {
            StopAllCoroutines();
            StartCoroutine(WaveLoop());
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i + 1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
                }
            }
        }
        
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 1f);
        }
    }
}