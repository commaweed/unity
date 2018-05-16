using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

    #region Singleton
    public static SpawnManager Instance;
    public void Awake() {
        Instance = this;
    }
    #endregion 

    public enum PoolType { enemy, shieldPowerUp, speedPowerUp, tripleShotPowerUp }

    /// <summary>
    /// A spawn pool.  Each item will be populated via the IDE.
    /// </summary>
    [System.Serializable]
    private class Pool {
        public PoolType type;
        public GameObject prefab;
        public int count;            // the max number of items that can be present at one time
    }

    // Create all the pools via the IDE
    [SerializeField]
    private List<Pool> pools;

    private Dictionary<PoolType, Queue<GameObject>> spawnPools;

    private bool spawnerOn;

    /// <summary>
    /// Spawn an object from the given pool at the given position.
    /// </summary>
    /// <param name="type">The type of pool.</param>
    /// <param name="position">The position to spawn at.</param>
    /// <returns>The object that was returned from the spawn pool.</returns>
    public void Spawn(PoolType type, Vector3 position) {
        if (spawnPools[type].Count == 0 || !this.spawnerOn) {
            return;   
        }

        GameObject nextItem = spawnPools[type].Dequeue();
        nextItem.SetActive(true);
        nextItem.transform.position = position;

        spawnPools[type].Enqueue(nextItem); // readd to the queue 
    }

    /// <summary>
    /// Hides the item and readds it to the spawn pool.
    /// </summary>
    /// <param name="type">The pool type.</param>
    /// <param name="item">The item to despawn.</param>
    public void Despawn(PoolType type, GameObject item) {
        if (item == null) {
            Debug.Log("Unable to add null GameObject to the spawn pool: " + type);
            return;
        }

        item.SetActive(false);
        spawnPools[type].Enqueue(item);
    }

    /// <summary>
    /// Handles the spawning of enemies.
    /// </summary>
    /// <returns></returns>
    private IEnumerator EnemySpawnerRoutine() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(1.5F, 3.0F));
            Spawn(
                PoolType.enemy,
                new Vector3(
                    Random.Range(-1 * Enemy.xThreshold, Enemy.xThreshold),
                    Enemy.yThreshold,
                    0
                )
            );
        }
    }

    /// <summary>
    /// Handles the spawning of power ups.
    /// </summary>
    /// <returns></returns>
    private IEnumerator PowerUpSpawnerRoutine() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(5.0F, 10.0F));

            PoolType randomPoolType = (PoolType) Random.Range(
                (int) PoolType.shieldPowerUp,
                (int) PoolType.tripleShotPowerUp + 1
            );

            Spawn(
                randomPoolType,
                new Vector3(
                    Random.Range(-1 * PowerUp.xThreshold, PowerUp.xThreshold),
                    Enemy.yThreshold,
                    0
                )
            );
        }
    }

    /// <summary>
    /// Enables the spawners.
    /// </summary>
    public void EnableSpawners() {
        this.spawnerOn = true;
    }

    /// <summary>
    /// Disables the spawners by hiding them.
    /// </summary>
    public void DisableSpawners() {
        this.spawnerOn = false;
        foreach (var item in spawnPools) {
            Queue<GameObject> spawnPool = item.Value;
            for (int i = 0; i < spawnPool.Count; i++) {
                this.Despawn(item.Key, spawnPool.Dequeue());
            }
        }
    }

    void Start() {
        // instantiate an populate the dictionary of all the spawn pools
        spawnPools = new Dictionary<PoolType, Queue<GameObject>>();
        foreach (Pool spawnPool in pools) {
            Queue<GameObject> objectSpawner = new Queue<GameObject>();

            // instantiate hidden pools objects for each spawn pool 
            for (int i = 0; i < spawnPool.count; i++) {
                GameObject prefab = Instantiate(spawnPool.prefab);
                prefab.SetActive(false);
                objectSpawner.Enqueue(prefab);
            }

            spawnPools.Add(spawnPool.type, objectSpawner);
        }

        StartCoroutine(EnemySpawnerRoutine());
        StartCoroutine(PowerUpSpawnerRoutine());
    }
}
