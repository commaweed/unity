using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePieceGridService  {

   private GameBoard board;

   public GamePieceGridService(GameBoard board) {
      if (board == null) {
         throw new System.ArgumentException("Invalid board; it cannot be null!");
      }
      this.board = board;
   }

   /// <summary>
   /// Returns a random game piece.
   /// </summary>
   /// <returns></returns>
   private GameObject GetRandomGamePiece() {
      int index = Random.Range(0, board.GamePiecePrefabs.Length);
      return board.GamePiecePrefabs[index];
   }

   /// <summary>
   /// Populate the game piece grid with all of the dots.  Position (x,y)=(0,0) is the bottom left corner.
   /// </summary>
   public void FillEmptyGamePieceGridSlots(int falseYOffset = 0, float moveTime = 0.1f) {
      for (int x = 0; x < board.GamePieceGrid.Width; x++) {
         for (int y = 0; y < board.GamePieceGrid.Height; y++) {
            if (board.GamePieceGrid.IsEmpty(x, y) && board.TileGrid.GetTileAt(x,y).TileType != TileType.Obstacle ) {
               GamePiece piece = CreateGamePieceAt(x, y, falseYOffset, moveTime);
               // avoid automatic matches with initial build
               int loopCounter = 0;
               while (board.MatchingService.HasMatchOnBuild(x, y)) {
                  board.ClearingService.ClearPieceAt(x, y);
                  piece = CreateGamePieceAt(x, y);
                  if (loopCounter++ > 100) {
                     Debug.LogWarning("Unable to counter matches on build, did you use one color?");
                     break;
                  }
               }
            }
         }
      }
   }

   /// <summary>
   /// Place the given game piece in the game piece grid at the given location.
   /// </summary>
   /// <param name="piece"></param>
   /// <param name="location"></param>
   public void PlaceGamePiece(GamePiece piece, Vector3 location) {
      int x = (int) location.x;
      int y = (int) location.y;
      piece.transform.position = location;   // a 2d position
      piece.transform.rotation = Quaternion.identity;    // no rotation
      piece.name = string.Format("Piece ({0},{1})", x, y);
      piece.SetCoordinates(x, y);
      piece.transform.parent = board.transform;
      board.GamePieceGrid.SetPieceAt(x, y, piece);
   }

   /// <summary>
   /// Place the given game piece in the game piece grid at the given (x,y) location.
   /// </summary>
   /// <param name="piece"></param>
   /// <param name="column"></param>
   /// <param name="y"></param>
   public void PlaceGamePiece(GamePiece piece, int x, int y) {
      PlaceGamePiece(piece, new Vector3(x, y, 0));
   }

   /// <summary>
   /// Creates a new GamePiece at the given location.  If falseYOffset is provided, it will be used to move the new pieces
   /// from above the grid.
   /// </summary>
   /// <param name="x"></param>
   /// <param name="y"></param>
   /// <param name="falseYOffset">0 means no offset</param>
   /// <returns></returns>
   private GamePiece CreateGamePieceAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f) {
      GamePiece result = null;
      if (board.GamePieceGrid.IsWithinBounds(x, y)) {
         Vector3 location = new Vector3(x, y, 0);
         GameObject pieceGameObject = MonoBehaviour.Instantiate(GetRandomGamePiece(), location, Quaternion.identity) as GameObject;
         GamePiece piece = pieceGameObject.GetComponent<GamePiece>();
         piece.Initialize(board);
         PlaceGamePiece(piece, location);

         if (falseYOffset != 0) {
            piece.transform.position = new Vector3(x, y + falseYOffset, 0);
            board.MovementService.Move(piece, x, y, moveTime);
         }

         result = piece;
      }
      return result;
   }

}
