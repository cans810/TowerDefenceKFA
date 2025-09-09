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
        StartCoroutine(WaveLoop());
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
    
}