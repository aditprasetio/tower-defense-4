using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class LevelManager : MonoBehaviour
{
    // Fungsi Singleton
    private static LevelManager _instance = null;

    public static LevelManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LevelManager>();
            }

            return _instance;
        }
    }

    [SerializeField] private int maxLives = 3;
    [SerializeField] private int totalEnemy = 15;

    [SerializeField] private GameObject endPanel;
    [SerializeField] private Text statusInfo;
    [SerializeField] private Text livesInfo;
    [SerializeField] private Text enemyInfo;

    [SerializeField] private Transform towerUIParent;
    [SerializeField] private GameObject towerUIPrefab;
    
    [SerializeField] private Tower[] towerPrefabs;
    [SerializeField] private Enemy[] enemyPrefabs;

    [SerializeField] private Transform[] enemyPaths;
    [SerializeField] private float spawnDelay = 5f;

    private List<Tower> _spawnedTowers = new List<Tower>();
    private List<Enemy> _spawnedEnemies = new List<Enemy>();
    private List<Bullet> _spawnedBullets = new List<Bullet>();
    
    private float _runningSpawnDelay;
    private int _currentLives;
    private int _currentEnemyCount;
    
    public bool IsOver { get; set; }

    private void Start()
    {
        SetCurrentLives(maxLives);
        SetTotalEnemy(totalEnemy);
        InstantiateAllTowerUI();
    }

    private void Update()
    {
        // Jika menekan tombol R, fungsi restart akan terpanggil
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        // Counter untuk spawn enemy dalam jeda waktu yang ditentukan
        // Time.unscaledDeltaTime adalah deltaTime yang independent, tidak terpengaruh oleh apapun kecuali game object itu sendiri,
        // jadi bisa digunakan sebagai penghitung waktu
        _runningSpawnDelay -= Time.unscaledDeltaTime;
        if (_runningSpawnDelay <= 0f)
        {
            SpawnEnemy();
            _runningSpawnDelay = spawnDelay;
        }

        
        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (!enemy.gameObject.activeSelf)
            {
                continue;
            }
            // nilainya 0.1? Karena untuk lebih mentoleransi perbedaan posisi,
            // akan terlalu sulit jika perbedaan posisinya harus 0 atau sama persis

            if (Vector2.Distance(enemy.transform.position, enemy.TargetPosition) < 0.1f)
            {
                enemy.SetCurrentPathIndex(enemy.CurrentPathIndex+1);
                if (enemy.CurrentPathIndex < enemyPaths.Length)
                {
                    enemy.SetTargetPosition(enemyPaths[enemy.CurrentPathIndex].position);
                }
                else
                {
                    enemy.gameObject.SetActive(false);
                    ReduceLives(--_currentLives);
                }
            }
            else
            {
                enemy.MoveToTarget();
            }
        }

        if (IsOver)
        {
            return;
        }
        foreach (Tower tower in _spawnedTowers)
        {
            tower.CheckNearestEnemy(_spawnedEnemies);
            tower.SeekTarget();
            tower.ShootTarget();
        }
    }

    private void InstantiateAllTowerUI()
    {
        foreach (var tower in towerPrefabs)
        {
            GameObject newTowerUIObj = Instantiate(towerUIPrefab.gameObject, towerUIParent);
            TowerUI newTowerUI = newTowerUIObj.gameObject.GetComponent<TowerUI>();

            newTowerUI.SetTowerPrefab(tower);
            newTowerUI.transform.name = tower.name;
        }
    }

    public void RegisterSpawnedTower(Tower tower)
    {
        _spawnedTowers.Add(tower);
    }

    private void SpawnEnemy()
    {
        SetTotalEnemy(--_currentEnemyCount);
        if (_currentEnemyCount <= 0)
        {
            bool isAllEnemyDestroyed = _spawnedEnemies.Find(e => e.gameObject.activeSelf) == null;
            if (isAllEnemyDestroyed)
            {
                SetGameOver(true);
            }
            
            return;
        }
        
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        string enemyIndexString = (randomIndex + 1).ToString();

        GameObject newEnemyObj = _spawnedEnemies.Find(
            e => !e.gameObject.activeSelf && e.name.Contains(enemyIndexString)
        )?.gameObject;

        if (newEnemyObj == null)
        {
            newEnemyObj = Instantiate(enemyPrefabs[randomIndex].gameObject);
        }

        Enemy newEnemy = newEnemyObj.GetComponent<Enemy>();
        if (!_spawnedEnemies.Contains(newEnemy))
        {
            _spawnedEnemies.Add(newEnemy);
        }

        newEnemy.transform.position = enemyPaths[0].position;
        newEnemy.SetTargetPosition(enemyPaths[1].position);
        newEnemy.SetCurrentPathIndex(1);
        newEnemy.gameObject.SetActive(true);
    }

    public Bullet GetBulletFromPool(Bullet bullet)
    {

        GameObject newBulletObj = _spawnedBullets.Find(
            e => !e.gameObject.activeSelf && e.name.Contains(bullet.name)
        )?.gameObject;

        if (newBulletObj == null)
        {
            newBulletObj = Instantiate(bullet.gameObject);
        }

        Bullet newBullet = newBulletObj.GetComponent<Bullet>();
        if (!_spawnedBullets.Contains(newBullet))
        {
            _spawnedBullets.Add(newBullet);
        }

        return newBullet;
    }

    public void ExplodeAt(Vector2 point, float radius, int damage)
    {
        foreach (Enemy enemy in _spawnedEnemies)
        {
            if (gameObject.activeSelf)
            {
                if (Vector3.Distance(enemy.transform.position, point) <= radius)
                {
                    enemy.ReduceEnemyHealth(damage);
                }
            }
        }
    }

    public void ReduceLives(int value)
    {
        Debug.Log(value);
        SetCurrentLives(value);
        if (_currentLives <= 0)
        {
            SetGameOver(false);
        }
    }

    public void SetCurrentLives(int currentLives)
    // Mathf.Max fungsi nya adalah mengambil angka terbesar
    // sehingga _currentLives di sini tidak akan lebih kecil dari 0
    {
        _currentLives = Mathf.Max(currentLives, 0);
        livesInfo.text = $"Lives : {_currentLives}";
    }

    public void SetTotalEnemy(int totalEnemies)
    {
        _currentEnemyCount = totalEnemies;
        enemyInfo.text = $"Enemy Left : {_currentEnemyCount}";
    }

    public void SetGameOver(bool isWin)
    {
        IsOver = true;

        statusInfo.text = isWin ? "YOU WIN!" : "YOU LOSE!";
        endPanel.gameObject.SetActive(true);
    }

    // Untuk menampilkan garis penghubung dalam window Scene
    // tanpa harus di-Play terlebih dahulu
    private void OnDrawGizmos()
    {
        for (int i = 0; i < enemyPaths.Length-1; i++)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(enemyPaths[i].position, enemyPaths[i+1].position);
        }
    }
}
