using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Coordinate {
   public int x;
   public int y;

   public Coordinate(int x, int y) {
      this.x = x;
      this.y = y;
   }

   public static bool operator ==(Coordinate c1, Coordinate c2) {
      return c1.Equals(c2);
   }

   public static bool operator !=(Coordinate c1, Coordinate c2) {
      return !c1.Equals(c2);
   }

   public override bool Equals(System.Object obj) {
      if (obj == null || GetType() != obj.GetType()) {
         return false;
      }
      Coordinate other = (Coordinate) obj;
      return this.x == other.x && this.y == other.y;
   }

   public override int GetHashCode() {
      return string.Format("({0},{1})", x, y).GetHashCode();
   }
}

[System.Serializable]
public class MapSize {
   [Range(1, 25)] public int width;
   [Range(1, 25)] public int height;
}

[System.Serializable]
public class Map {
   public MapSize mapSize;
   [Range(0f, 1f)] public float obstaclePercent;
   public int seed;
   [Range(0.1f,5f)] public float minObstacleHeight;
   [Range(0.1f,5f)] public float maxObstacleHeight;
   public Color foregroundColor;
   public Color backgroundColor;

   public Coordinate MapCenter { get { return new Coordinate(mapSize.width / 2, mapSize.height / 2); } }
   public System.Random RandomNumberGenerator { get { return new System.Random(this.seed); } }
}

public class MapGenerator : MonoBehaviour {

   private const string GENERATED_MAP_NAME = "GeneratedMap";

   [SerializeField] private Transform tilePrefab;
   [SerializeField] private Transform obstaclePrefab;
   [SerializeField] private Transform navMeshFloorPrefab;
   [SerializeField] private Transform backgroundFloorPrefab;
   [SerializeField] private Transform navMeshEdgePrefab;  // blocks the edge of the grid so agents don't fall off

   // for the navMeshFloor (we can only bake at the beginning, so making floor large as biggest map)
   [SerializeField] private MapSize maxMapSize;

   [SerializeField][Range(0.1f, 10f)] private float tileSize = 1f;
   [SerializeField][Range(0f, 1f)] private float outlinePercent;

   [SerializeField] private Map[] maps;
   [SerializeField] private int mapIndex;

   private Map currentMap;
   private List<Coordinate> allTileLocations;
   private Queue<Coordinate> shuffledTileLocations;
   private Queue<Coordinate> shuffledBlankTileLocations;
   private Transform[,] tileMap;

   private void Start() {
      // register with the event
      FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
   }

   private void Update() {
 
   }

   public void GenerateMap() {
      InitializeMapState();
      CreateCoordinateContainers();
      DestroyOldGeneratedMap();
      Transform newMapTransform = CreateNewGeneratedMap();
      CreateRandomObstacles(newMapTransform);
      CreateHorizontalNavMeshEdgeMask(Vector3.left, newMapTransform);
      CreateHorizontalNavMeshEdgeMask(Vector3.right, newMapTransform);
      CreateVerticalNavMeshEdgeMask(Vector3.forward, newMapTransform);
      CreateVerticalNavMeshEdgeMask(Vector3.back, newMapTransform);
   }

   private void InitializeMapState() {
      currentMap = maps[mapIndex];
      this.navMeshFloorPrefab.localScale = new Vector3(maxMapSize.width, maxMapSize.height) * tileSize;
      this.backgroundFloorPrefab.localScale = new Vector3(currentMap.mapSize.width * tileSize, currentMap.mapSize.height * tileSize);
   }

   private void CreateCoordinateContainers() {
      this.allTileLocations = new List<Coordinate>();
      for (int x = 0; x < currentMap.mapSize.width; x++) {
         for (int y = 0; y < currentMap.mapSize.height; y++) {
            allTileLocations.Add(new Coordinate(x, y));
         }
      }

      this.shuffledTileLocations = new Queue<Coordinate>(Utility.ShuffleArray(allTileLocations.ToArray(), currentMap.seed));
   }

   private void DestroyOldGeneratedMap() {
      Transform oldMapTransform = transform.Find(GENERATED_MAP_NAME);
      if (oldMapTransform != null) {
         DestroyImmediate(oldMapTransform.gameObject); // used by editor so immediately is required
      }
   }

   private Transform CreateNewGeneratedMap() {
      Transform newMapTransform = new GameObject(GENERATED_MAP_NAME).transform;
      newMapTransform.parent = this.transform;
      this.tileMap = new Transform[currentMap.mapSize.width, currentMap.mapSize.height];

      for (int x = 0; x < currentMap.mapSize.width; x++) {
         for (int y = 0; y < currentMap.mapSize.height; y++) {
            Vector3 tilePosition = ComputeCoordinatePosition(x, y);

            // rotate by 90 degrees so it lies flat
            Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90));

            // child to Map
            newTile.parent = newMapTransform;

            // when 1, entire thing will be outlined
            newTile.localScale = Vector3.one * (1f - outlinePercent) * tileSize;

            tileMap[x, y] = newTile;
         }
      }

      return newMapTransform;
   }

   private Vector3 ComputeCoordinatePosition(int x, int y) {
      // shift by 0.5f to put the edge of the tile at the position
     return new Vector3(-currentMap.mapSize.width / 2f + 0.5f + x, 0f, -currentMap.mapSize.height / 2f + 0.5f + y) * tileSize;
   }

   public Coordinate GetRandomTileLocation() {
      Coordinate randomCoordinate = shuffledTileLocations.Dequeue();
      shuffledTileLocations.Enqueue(randomCoordinate);
      return randomCoordinate;
   }

   public Transform GetRandomBlankTile() {
      Coordinate randomLocation = this.shuffledBlankTileLocations.Dequeue();
      this.shuffledBlankTileLocations.Enqueue(randomLocation);
      return tileMap[randomLocation.x, randomLocation.y];
   }

   public Transform GetTileFromPosition(Vector3 position) {
      int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.width - 1) / 2f);
      int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.height - 1) / 2f);

      // protect against out of bounds
      x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
      y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);
      return tileMap[x, y];
   }

   private void CreateObstacle(float currentHeight, Coordinate location, Transform mapTransform) {
      float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, currentHeight);
      Vector3 obstaclePosition = ComputeCoordinatePosition(location.x, location.y);

      Transform newObstacle = Instantiate(
         obstaclePrefab,
         obstaclePosition + Vector3.up * obstacleHeight / 2f,
         Quaternion.identity
      ) as Transform;

      newObstacle.parent = mapTransform;
      newObstacle.localScale = new Vector3(
         (1 - outlinePercent) * tileSize,
         obstacleHeight,
          (1 - outlinePercent) * tileSize
      );

      ColorizeObstacle(newObstacle, location);
   }

   private void CreateRandomObstacles(Transform newMapTransform) {
      System.Random random = currentMap.RandomNumberGenerator; 
      bool[,] obstacleMap = new bool[(int) currentMap.mapSize.width, (int) currentMap.mapSize.height];
      int obstacleCount = (int) ((currentMap.mapSize.width * currentMap.mapSize.height) * currentMap.obstaclePercent);
      int currentObstacleCount = 0;
      List<Coordinate> blankTileLocations = new List<Coordinate>(this.allTileLocations); // start off with all tiles present

      for (int i=0; i < obstacleCount; i++) {
         Coordinate randomTileLocation = GetRandomTileLocation();
         obstacleMap[randomTileLocation.x, randomTileLocation.y] = true;
         currentObstacleCount++;
         if (randomTileLocation != currentMap.MapCenter && IsMapTraversable(obstacleMap, currentObstacleCount)) {
            CreateObstacle((float) random.NextDouble(), randomTileLocation, newMapTransform);
            blankTileLocations.Remove(randomTileLocation); // we put an obstacle on this location, so remove it from list
         } else {
            obstacleMap[randomTileLocation.x, randomTileLocation.y] = false;
            currentObstacleCount--;
         }
      }

      this.shuffledBlankTileLocations = new Queue<Coordinate>(Utility.ShuffleArray(blankTileLocations.ToArray(), currentMap.seed));
   }

   private void ColorizeObstacle(Transform obstacle, Coordinate location) {
      Renderer obstacleRenderer = obstacle.GetComponent<Renderer>();
      Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
      float colorPercent = ((float) location.y / currentMap.mapSize.height);
      obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
      obstacleRenderer.sharedMaterial = obstacleMaterial;
   }

   bool IsMapTraversable(bool[,] obstacleMap, int currentObstacleCount) {
      bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
      Queue<Coordinate> queue = new Queue<Coordinate>();
      queue.Enqueue(currentMap.MapCenter);
      mapFlags[currentMap.MapCenter.x, currentMap.MapCenter.y] = true;

      int accessibleTileCount = 1;

      while (queue.Count > 0) {
         Coordinate tile = queue.Dequeue();

         // look at all adjacent tiles (not diagonals)
         for (int x = -1; x <= 1; x++) {
            for (int y = -1; y <= 1; y++) {
               int neighborx = tile.x + x;
               int neighbory = tile.y + y;
               // is not a diagonal
               if (x == 0 || y == 0) {
                  // is inside mapFlags bounds
                  if (
                     neighborx >= 0 && neighborx < obstacleMap.GetLength(0) &&
                      neighbory >= 0 && neighbory < obstacleMap.GetLength(1)
                  ) {
                     // we have not already checked this tile and its not an obstacle tile
                     if (!mapFlags[neighborx, neighbory] && !obstacleMap[neighborx, neighbory]) {
                        mapFlags[neighborx, neighbory] = true;  // now we have checked this tile

                        // add this neighboring tile to the queue so that we can later look at all of its neighbors
                        queue.Enqueue(new Coordinate(neighborx, neighbory));
                        accessibleTileCount++;
                     }
                  }
               }
            }
         }
      }

      // so how many tiles should there be
      int targetAccessibleTileCount = (int) (currentMap.mapSize.width * currentMap.mapSize.height - currentObstacleCount);

      return targetAccessibleTileCount == accessibleTileCount;
   }

   private void CreateHorizontalNavMeshEdgeMask(Vector3 direction, Transform mainMapTransform) {
      Transform newEdgeMaskObject = Instantiate(
         navMeshEdgePrefab,
         direction * ((currentMap.mapSize.width + maxMapSize.width) / 4f) * tileSize, 
         Quaternion.identity
      ) as Transform;
      newEdgeMaskObject.parent = mainMapTransform;
      newEdgeMaskObject.localScale = new Vector3((maxMapSize.width - currentMap.mapSize.width) / 2f, 1f, currentMap.mapSize.height) * tileSize; 
   }

   private void CreateVerticalNavMeshEdgeMask(Vector3 direction, Transform mainMapTransform) {
      Transform newEdgeMaskObject = Instantiate(
         navMeshEdgePrefab,
         direction * ((currentMap.mapSize.height + maxMapSize.height) / 4f) * tileSize,
         Quaternion.identity
      ) as Transform;
      newEdgeMaskObject.parent = mainMapTransform;
      newEdgeMaskObject.localScale = new Vector3(maxMapSize.width, 1f, (maxMapSize.height - currentMap.mapSize.height) / 2f) * tileSize;
   }

   private void OnNewWave(int waveNumber) {
      mapIndex = waveNumber - 1;
      GenerateMap();
   }

}
