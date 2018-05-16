using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InterpolationType {
   Linear, EaseOut, EaseIn, SmoothStep, SmootherStep
}

public enum MatchType {
   Yellow, Blue, Magenta, Indigo, Green, Teal, Red, Cyan, Wild
}

/// <summary>
/// The script that manages that Dot game pieces.  It should be a component of all the dots that are in the game.
/// </summary>
public class GamePiece : MonoBehaviour {

   private int x;
   public int X { get { return this.x; } }
   private int y;
   public int Y { get { return this.y; } }

   private GameBoard board;

   private TextMesh textMesh;

   [SerializeField] private MatchType matchType;
   public MatchType MatchType { get { return this.matchType; }  }

   // we do not want to move again if we are already in a move state
   public bool IsMoving { get; set; }

   public InterpolationType interpolation = InterpolationType.SmootherStep;

	// Use this for initialization
	void Start () {
      EnableDebugText(board.DisplayDebugText);
   }
	
	// Update is called once per frame
	void Update () {
      EnableDebugText(board.DisplayDebugText);
   }

   /// <summary>
   /// Turns on/off the debug text (x,y) on the game pieces.
   /// </summary>
   /// <param name="value"></param>
   private void EnableDebugText(bool value) {
      if (this.textMesh != null)
         this.textMesh.gameObject.SetActive(value);
   }

   /// <summary>
   /// Initialize the tile with the given values. 
   /// </summary>
   /// <param name="board">A reference to the parent game board</param>
   public void Initialize(GameBoard board) {
      if (board == null) {
         throw new System.ArgumentException("Invalid game board given to Tile; did you forget to add it in the inspector?");
      }

      // this assumes the text mesh element is the first component
      if (transform.childCount > 0) {
         this.textMesh = transform.GetChild(0).GetComponent<TextMesh>(); 
      }

      this.board = board;
   }

   /// <summary>
   /// Initialize with the given coordinates for where the game piece lives in the grid.
   /// </summary>
   /// <param name="x"></param>
   /// <param name="y"></param>
   public void SetCoordinates(int x, int y) {
      this.x = x;
      this.y = y;
      if (textMesh != null) textMesh.text = string.Format("({0},{1})", x, y);
   }

   public void SetCoordinates(float x, float y) {
      this.SetCoordinates((int) x, (int) y);
   }

   public void SetCoordinates(Vector3 position) {
      this.SetCoordinates(position.x, position.y);
   }

   public string ToString {
      get {
         return string.Format("({0},{1})", x, y);
      }
   }
}
