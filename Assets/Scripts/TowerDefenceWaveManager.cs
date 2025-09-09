using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerDefenceWaveManager : MonoBehaviour
{
    [Header("Wave Configuration")]
    public int totalWaves = 20;
    public float timeBetweenWaves = 8f;
    public float spawnDelay = 1f;
    public int baseEnemiesPerWave = 10;
    
    [Header("Boss Waves (Extra)")]
    public int[] bossWaves = { 5, 10, 15, 20 };
    public float bossWaveDelay = 12f;
    
    [Header("Spawn Settings")]
    public Transform spawnPoint;
    public Transform[] waypoints;
    public GameObject enemyPrefab;
    
    [Header("Performance")]
    public int maxSimultaneousEnemies = 2500;
    
    private int currentWave = 1;
    private int enemiesAlive = 0;
    private int enemiesSpawned = 0;
    private bool isSpawning = false;
    private bool gameActive = true;
    private List<TowerDefenceEnemy> activeEnemies = new List<TowerDefenceEnemy>();
    private Coroutine waveCoroutine;
    
    void Start()
    {
        InitializeWaveManager();
        StartCoroutine(WaveLoop());
    }
    
    void InitializeWaveManager()
    {
        SetupSpawnAndPath();
        CreateEnemyPrefab();
    }
    
    void SetupSpawnAndPath()
    {
        if (spawnPoint == null)
            CreateSpawnPoint();
            
        if (waypoints == null || waypoints.Length == 0)
            CreateWaypoints();
    }
    
    void CreateSpawnPoint()
    {
        GameObject existingSpawn = GameObject.Find("EnemySpawnPoint");
        
        if (existingSpawn != null)
        {
            spawnPoint = existingSpawn.transform;
            return;
        }
        
        
        GameObject spawn = new GameObject("EnemySpawnPoint");
        spawn.transform.position = new Vector3(-25, 1, 0);
        spawnPoint = spawn.transform;
        
        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "SpawnMarker";
        marker.transform.SetParent(spawn.transform);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localScale = Vector3.one * 0.8f;
        
        Renderer renderer = marker.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.red;
        renderer.material = mat;
        
    }
    
    void CreateWaypoints()
    {
        GameObject waypointParent = GameObject.Find("EnemyWaypoints");
        
        if (waypointParent != null && waypointParent.transform.childCount > 0)
        {
            waypoints = new Transform[waypointParent.transform.childCount];
            for (int i = 0; i < waypoints.Length; i++)
            {
                waypoints[i] = waypointParent.transform.GetChild(i);
            }
            return;
        }
        
        
        Vector3[] pathPoints = {
            new Vector3(-25, 1, 0),
            new Vector3(-15, 1, 0),
            new Vector3(-15, 1, 15),
            new Vector3(0, 1, 15),
            new Vector3(0, 1, -15),
            new Vector3(15, 1, -15),
            new Vector3(15, 1, 5),
            new Vector3(25, 1, 5)
        };
        
        if (waypointParent == null)
        {
            waypointParent = new GameObject("EnemyWaypoints");
        }
        
        waypoints = new Transform[pathPoints.Length];
        
        for (int i = 0; i < pathPoints.Length; i++)
        {
            GameObject waypoint = new GameObject($"Waypoint_{i:D2}");
            waypoint.transform.position = pathPoints[i];
            waypoint.transform.SetParent(waypointParent.transform);
            waypoints[i] = waypoint.transform;
            
            Transform existingMarker = waypoint.transform.Find("PathMarker");
            if (existingMarker == null)
            {
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                marker.name = "PathMarker";
                marker.transform.SetParent(waypoint.transform);
                marker.transform.localPosition = Vector3.zero;
                marker.transform.localScale = new Vector3(1.5f, 0.1f, 1.5f);
                
                Renderer renderer = marker.GetComponent<Renderer>();
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = Color.yellow;
                renderer.material = mat;
            }
        }
        
    }
    
    void CreateEnemyPrefab()
    {
        if (enemyPrefab != null) return;
        
        #if UNITY_EDITOR
        enemyPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/TowerDefenceEnemyBasic.prefab");
        
        if (enemyPrefab != null)
        {
            return;
        }
        #endif
        
        enemyPrefab = new GameObject("TowerDefenceEnemy");
        enemyPrefab.tag = "Enemy";
        
        enemyPrefab.AddComponent<TowerDefenceEnemy>();
        
        Rigidbody rb = enemyPrefab.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        
        BoxCollider collider = enemyPrefab.AddComponent<BoxCollider>();
        collider.size = new Vector3(1.2f, 1.5f, 1.2f);
        collider.isTrigger = true;
        
        GameObject spriteChild = new GameObject("EnemySprite");
        spriteChild.transform.SetParent(enemyPrefab.transform);
        spriteChild.transform.localPosition = Vector3.zero;
        spriteChild.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        
        SpriteRenderer spriteRenderer = spriteChild.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateEnemySprite();
        spriteRenderer.sortingOrder = 5;
        
        spriteChild.AddComponent<BillboardSprite>();
        
        enemyPrefab.SetActive(false);
    }
    
    Sprite CreateEnemySprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.red;
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);
    }
    
    IEnumerator WaveLoop()
    {
        float initialDelay = 2f;
        float elapsed = 0f;
        
        while (elapsed < initialDelay)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        while (currentWave <= totalWaves && gameActive)
        {
            bool isBossWave = System.Array.IndexOf(bossWaves, currentWave) >= 0;
            
            AnnounceWave(isBossWave);
            
            yield return StartCoroutine(WaitForWaveCall());
            
            waveCoroutine = StartCoroutine(SpawnWave(isBossWave));
            yield return waveCoroutine;
            
            currentWave++;
            
            if (isBossWave)
            {
            }
        }
        
        gameActive = false;
    }
    
    void AnnounceWave(bool isBossWave)
    {
        string announcement = isBossWave ? 
            $"BOSS WAVE {currentWave} INCOMING!" : 
            $"Wave {currentWave}";
            
    }
    
    IEnumerator SpawnWave(bool isBossWave)
    {
        isSpawning = true;
        enemiesSpawned = 0;
        
        int enemiesToSpawn = CalculateEnemyCount(isBossWave);
        
        for (int i = 0; i < enemiesToSpawn && gameActive; i++)
        {
            if (enemiesAlive >= maxSimultaneousEnemies)
            {
                yield return new WaitUntil(() => enemiesAlive < maxSimultaneousEnemies);
            }
            
            SpawnEnemy(isBossWave, i, enemiesToSpawn);
            enemiesSpawned++;
            
            yield return StartCoroutine(WaitForSpawnDelay());
        }
        
        isSpawning = false;
    }
    
    IEnumerator WaitForSpawnDelay()
    {
        float elapsed = 0f;
        
        while (elapsed < spawnDelay)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                yield break;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
    
    int CalculateEnemyCount(bool isBossWave)
    {
        if (isBossWave)
        {
            return Random.Range(2, 5);
        }
        else
        {
            return baseEnemiesPerWave + (currentWave - 1) * 2;
        }
    }
    
    void SpawnEnemy(bool isBossWave, int enemyIndex, int totalEnemies)
    {
        if (enemyPrefab == null || spawnPoint == null) return;
        
        Vector2 randomOffset = Random.insideUnitCircle * 2f;
        Vector3 spawnPos = spawnPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);
        spawnPos.y = 1f;
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        enemy.SetActive(true);
        
        TowerDefenceEnemy enemyScript = enemy.GetComponent<TowerDefenceEnemy>();
        if (enemyScript != null)
        {
            TowerDefenceEnemy.EnemyType enemyType = DetermineEnemyType(isBossWave, enemyIndex, totalEnemies);
            
            enemyScript.SetEnemyType(enemyType);
            enemyScript.SetWaypoints(waypoints, this);
            
            if (enemy.GetComponentInChildren<EnemyHealthBar>() == null)
            {
                GameObject healthBarObj = new GameObject("HealthBar");
                healthBarObj.transform.SetParent(enemy.transform);
                healthBarObj.AddComponent<EnemyHealthBar>();
            }
        }
        
        activeEnemies.Add(enemyScript);
        enemiesAlive++;
        
    }
    
    TowerDefenceEnemy.EnemyType DetermineEnemyType(bool isBossWave, int enemyIndex, int totalEnemies)
    {
        if (isBossWave)
        {
            return TowerDefenceEnemy.EnemyType.Boss;
        }
        
        if (currentWave <= 2)
        {
            return TowerDefenceEnemy.EnemyType.Basic;
        }
        else if (currentWave <= 4)
        {
            float rand = Random.value;
            if (rand < 0.7f) return TowerDefenceEnemy.EnemyType.Basic;
            else return TowerDefenceEnemy.EnemyType.Fast;
        }
        else if (currentWave <= 7)
        {
            float rand = Random.value;
            if (rand < 0.5f) return TowerDefenceEnemy.EnemyType.Basic;
            else if (rand < 0.8f) return TowerDefenceEnemy.EnemyType.Fast;
            else return TowerDefenceEnemy.EnemyType.Tank;
        }
        else if (currentWave <= 10)
        {
            float rand = Random.value;
            if (rand < 0.4f) return TowerDefenceEnemy.EnemyType.Basic;
            else if (rand < 0.65f) return TowerDefenceEnemy.EnemyType.Fast;
            else if (rand < 0.85f) return TowerDefenceEnemy.EnemyType.Tank;
            else return TowerDefenceEnemy.EnemyType.Elite;
        }
        else
        {
            float rand = Random.value;
            if (rand < 0.25f) return TowerDefenceEnemy.EnemyType.Basic;
            else if (rand < 0.45f) return TowerDefenceEnemy.EnemyType.Fast;
            else if (rand < 0.70f) return TowerDefenceEnemy.EnemyType.Tank;
            else return TowerDefenceEnemy.EnemyType.Elite;
        }
    }
    
    public void OnEnemyKilled(TowerDefenceEnemy enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
            
        enemiesAlive--;
    }
    
    public void OnEnemyReachedEnd(TowerDefenceEnemy enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
            
        enemiesAlive--;
    }
    
    void Update()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
        
        UpdateUIDisplay();
    }
    
    void UpdateWaveUI()
    {
    }
    
    
    void UpdateUIDisplay()
    {
        bool isBossWave = System.Array.IndexOf(bossWaves, currentWave) >= 0;
        UIManager.Instance?.UpdateWaveInfo(currentWave, totalWaves, enemiesAlive, isSpawning, isBossWave);
    }
    
    void OnDrawGizmosSelected()
    {
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
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
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 2f);
        }
    }
    
    IEnumerator WaitForWaveCall()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                yield break;
            }
            
            if (enemiesAlive == 0)
            {
                yield break;
            }
            
            yield return null;
        }
    }
    
    public int GetCurrentWave() => currentWave;
    public int GetTotalWaves() => totalWaves;
    public int GetEnemiesAlive() => enemiesAlive;
    public bool IsGameActive() => gameActive;
}