using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public class StartingTile {
   public GameObject tilePrefab;
   public int x;
   public int y;
   public int z;
}

/// <summary>
/// Represents the game board that our match three puzzle will be rendered on.
/// 
/// orthographic size = screen height / 2
/// when orthographic height = 1, this implies screen height will = 2
/// vertical:  orthographic size = (board width /2 + border size) / aspect ratio
/// horizontal:  orthographic size = board height /2 + border size
/// Max(vertical, horizontal) = what we should set our ortho size to using a script
/// 
/// aspect ratio = screen width / screen height, so 1080/1920 = 0.5625 aspect ratio
///              = screen width / 2 x orthographic size
///              
/// screen width = 2 * orthographic size * aspect ratio
/// screen height = 2 * orthographic size
/// 
/// 
/// </summary>
public class GameBoard : MonoBehaviour {

   [SerializeField] private bool displayDebugText;

   [SerializeField] private int width;
   [SerializeField] private int height;
   [SerializeField] private int borderSize;
   [SerializeField] [Range(0f, 3f)] private float swapTime = 0.5f;

   [SerializeField] private GameObject tileNormalPrefab;
   [SerializeField] private GameObject tileObstaclelPrefab;
   [SerializeField] private GameObject[] gamePiecePrefabs;

   [SerializeField] private GameObject clearFxPrefab;
   [SerializeField] private GameObject breakFxPrefab;
   [SerializeField] private GameObject doubleBreakFxPrefab;

   [SerializeField] private StartingTile[] startingTiles;

   private void Awake() {
      ValidateRequiredComponents();
   }

   // Use this for initialization
   private void Start() {
      CreateDependencies();
      InitializeGame();
   }

   // Update is called once per frame
   private void Update() {
   }

   private void ValidateRequiredComponents() {
      // prefabs
      Assert.IsNotNull(tileNormalPrefab, "Missing tileNormalPrefab prefab; did you forget to add it in the inspector?");
      Assert.IsNotNull(tileObstaclelPrefab, "Missing tileObstaclelPrefab prefab; did you forget to add it in the inspector?");
      Assert.IsNotNull(gamePiecePrefabs, "Missing GamePieces[] prefabs; did you forget to add them in the inspector?");
      Assert.IsTrue(gamePiecePrefabs.Length > 0, "Invalid GamePieces[] prefabs; did you forget to set length > 0 in inspector?");
      foreach (GameObject gamePiecePrefab in gamePiecePrefabs) {
         Assert.IsNotNull(gamePiecePrefab, "Invalid gamePiecePrefab found in gamePiecePrefabs[]; did you forget to add one to the inspected?");
      }
      Assert.IsNotNull(clearFxPrefab, "Missing clearFxPrefab prefab; did you forget to add it in the inspector?");
      Assert.IsNotNull(breakFxPrefab, "Missing breakFxPrefab prefab; did you forget to add it in the inspector?");
      Assert.IsNotNull(doubleBreakFxPrefab, "Missing doubleBreakFxPrefab prefab; did you forget to add it in the inspector?");

      // data
      Assert.IsTrue(width > 0, "Invalid width; did you forget to indicate a valid game board width in the inspected?");
      Assert.IsTrue(height > 0, "Invalid height; did you forget to indicate a valid game board width in the inspected?");
      Assert.IsTrue(borderSize > 0, "Invalid borderSize; did you forget to indicate a valid game board width in the inspected?");
      Assert.IsNotNull(startingTiles, "Missing startingTiles[]; did you forget to add them in the inspector?");
      Assert.IsTrue(startingTiles.Length > 0, "Invalid startingTiles[] length; did you forget to add a size in the inspector?");
      foreach (StartingTile startingTile in startingTiles) {
         Assert.IsNotNull(startingTile.tilePrefab, "Invalid startingTile.tilePrefab found in startingTiles[]; did you forget to add one to the inspected?");
      }
   }

   private void CreateDependencies() {
      this.TileGrid = new TileGrid(width, height);
      this.GamePieceGrid = new GamePieceGrid(width, height);
      this.SceneService = new SceneService(this);
      this.TileGridService = new TileGridService(this);
      this.GamePieceGridService = new GamePieceGridService(this);
      this.ClearingService = new ClearingService(this);
      this.MatchingService = new MatchingService(this);
      this.MovementService = new MovementService(this);
      this.ParticleService = new ParticleService(this);
   }

   private void InitializeGame() {
      SceneService.InitializeCamera();
      IsPlayerInputAllowed = true;
      TileGridService.PopulateTilesGrid();
      GamePieceGridService.FillEmptyGamePieceGridSlots(10, 0.5f); // they are all empty in the beginning
   }

   // data
   public TileGrid TileGrid { get; set; }
   public GamePieceGrid GamePieceGrid { get; set; }
   public int Width { get { return this.width; } }
   public int Height { get { return this.height; } }
   public int BorderSize { get { return this.borderSize; } }
   public float SwapTime { get { return this.swapTime; } }
   public bool IsPlayerInputAllowed { get; set; }
   public bool DisplayDebugText { get { return this.displayDebugText; } }
   public StartingTile[] StartingTiles { get { return this.startingTiles; } }

   // prefabs
   public GameObject TileNormalPrefab { get { return this.tileNormalPrefab; } }
   public GameObject TileObstaclelPrefab { get { return this.tileObstaclelPrefab; } }
   public GameObject[] GamePiecePrefabs { get { return this.gamePiecePrefabs; } }
   public GameObject ClearFxPrefab { get { return this.clearFxPrefab; } }
   public GameObject BreakFxPrefab { get { return this.breakFxPrefab; } }
   public GameObject DoubleBreakFxPrefab { get { return this.doubleBreakFxPrefab; } }

   // services
   public SceneService SceneService { get; set; }
   public TileGridService TileGridService { get; set; }
   public GamePieceGridService GamePieceGridService { get; set; }
   public ClearingService ClearingService { get; set; }
   public MatchingService MatchingService { get; set; }
   public MovementService MovementService { get; set; }
   public ParticleService ParticleService { get; set; }
}
