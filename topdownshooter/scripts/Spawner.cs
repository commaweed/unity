using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Wave {
   [SerializeField] private bool infinite;
   public bool Infinite { get { return this.infinite; } }

   [SerializeField] private int enemyCount;
   public int EnemyCount { get { return this.enemyCount; } }

   [SerializeField] private float timeBetweenSpawns;
   public float TimeBetweenSpawns { get { return this.timeBetweenSpawns; } }

   [SerializeField] private float moveSpeed;
   public float MoveSpeed { get { return this.moveSpeed; } }

   [SerializeField] private int hitsToKillPlayer;
   public int HitsToKillPlayer { get { return this.hitsToKillPlayer; } }

   [SerializeField] private float enemyHealth;
   public float EnemyHealth { get { return this.enemyHealth; } }

   [SerializeField] private Color skinColor;
   public Color SkinColor { get { return this.skinColor; } }
}

public class Spawner : MonoBehaviour {

   [SerializeField] private bool debugMode;
   [SerializeField] private Enemy enemy;
   [SerializeField] private Wave[] waves;

   private Wave currentWave;
   private int currentWaveNumber;
   private int enemiesRemainingToSpawn;
   private int enemiesRemainingAlive;
   private float nextSpawnTime;
   private MapGenerator map;
   private LivingEntity playerEntity;
   private Transform playerTransform;
   private bool isDisabled;

   // want to track when a player sits in one spot for too long (spawn on top of them)
   float timeBetweenCampingChecks = 2;
   float campThresholdDistance = 1.5f;
   float nextCampCheckTime;
   Vector3 campPositionOld;
   bool isCamping;

   // store the index of the map
   public event System.Action<int> OnNewWave;

   private void Start() {
      playerEntity = FindObjectOfType<LivingEntity>();
      playerEntity.OnDeath += OnPlayerDeath;
      playerTransform = playerEntity.transform;
      nextCampCheckTime = timeBetweenCampingChecks + Time.time;
      campPositionOld = playerTransform.position;
      map = FindObjectOfType<MapGenerator>();

      NextWave();
   }

   private void Update() {
      if (this.isDisabled) return;
      HandlePlayerCamping();
      SpawnEnemies();

      if (debugMode) {
         if (Input.GetKeyDown(KeyCode.Return)) {
            StopCoroutine("SpawnNewEnemyRoutine");

            // destroy any enemies that are currently alive in the scene
            foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
               GameObject.Destroy(enemy.gameObject);
            }

            NextWave();
         }
      }

   }

   private void ResetPlayerPosition() {
      // have player fall from sky
      playerTransform.position = map.GetTileFromPosition(Vector3.zero).position + (Vector3.up * 3);
   }

   private void HandlePlayerCamping() {
      if (Time.time > nextCampCheckTime) {
         nextCampCheckTime = Time.time + timeBetweenCampingChecks;
         isCamping = Vector3.Distance(playerTransform.position, campPositionOld) < campThresholdDistance;
         campPositionOld = playerTransform.position;
      }
   }

   private void SpawnEnemies() {
      if ((enemiesRemainingToSpawn > 0 || currentWave.Infinite) && Time.time > nextSpawnTime) {
         enemiesRemainingToSpawn--;
         nextSpawnTime = Time.time + currentWave.TimeBetweenSpawns;

         StartCoroutine("SpawnNewEnemyRoutine"); // set to string so we can stop it
      }
   }

   IEnumerator SpawnNewEnemyRoutine() {
      float spawnDelay = 1f;
      float tileFlashSpeed = 4f;

      Transform spawnTileLocation = GetSpawnTileLocation();
      Material tileMaterial = spawnTileLocation.GetComponent<Renderer>().material;
      Color initialColor = Color.white;
      Color flashColor = Color.red;
      float spawnTimer = 0;

      while (spawnTimer < spawnDelay) {
         // from 0 to 1 and back 4 times (i.e. tileflashspeed)
         tileMaterial.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));

         spawnTimer += Time.deltaTime;
         yield return null;
      }

      Enemy spawnedEnemy = Instantiate(enemy, spawnTileLocation.position + Vector3.up, Quaternion.identity) as Enemy;

      // setup callback to the OnDeath event
      spawnedEnemy.OnDeath += OnEnemyDeath;

      spawnedEnemy.SetCharacteristics(currentWave);
   }

   private Transform GetSpawnTileLocation() {
      Transform spawnLocation = map.GetRandomBlankTile();
      if (isCamping) {
         spawnLocation = map.GetTileFromPosition(playerTransform.position);
      }
      return spawnLocation;
   }

   private void NextWave() {
      currentWaveNumber++;
      if (currentWaveNumber - 1 < waves.Length) {
         currentWave = waves[currentWaveNumber - 1];
         enemiesRemainingToSpawn = currentWave.EnemyCount;
         enemiesRemainingAlive = enemiesRemainingToSpawn;

         // fire the nextwave event
         if (OnNewWave != null) {
            OnNewWave(currentWaveNumber);
         }

         ResetPlayerPosition();
      }
   }

   // callback that is notified when the OnDeath event is fired (happens when LivingEntity dies)
   private void OnEnemyDeath() {
      enemiesRemainingAlive--;

      if (enemiesRemainingAlive == 0) {
         NextWave();
      }
   }

   private void OnPlayerDeath() {
      this.isDisabled = true;
   }

}
