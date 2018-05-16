using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileGridService {

   private GameBoard board;

   private Tile clickedTile;
   private Tile targetTile;

   public TileGridService(GameBoard board) {
      if (board == null) {
         throw new System.ArgumentException("Invalid board; it cannot be null!");
      }
      this.board = board;
   }

   /// <summary>
   /// Populate the tiles grid with all of the tiles.  Position (x,y)=(0,0) is the bottom left corner.
   /// </summary>
   public void PopulateTilesGrid() {
      foreach (StartingTile startingTile in this.board.StartingTiles) {
         if (startingTile != null) {
            CreateTile(startingTile.tilePrefab, startingTile.x, startingTile.y, startingTile.z);
         }
      }

      for (int x = 0; x < board.TileGrid.Width; x++) {
         for (int y = 0; y < board.TileGrid.Height; y++) {
            if (board.TileGrid.IsEmpty(x, y)) { // we may have added some starting tiles
               CreateTile(board.TileNormalPrefab, x, y);
            } 
         }
      }
   }

   /// <summary>
   /// Create a single tile at the given coordinates.
   /// </summary>
   /// <param name="tilePrefab"></param>
   /// <param name="x"></param>
   /// <param name="y"></param>
   /// <param name="z">z coordinate if you want to pull it towards or away from camera</param>
   public void CreateTile(GameObject tilePrefab, int x, int y, int z = 0) {
      if (tilePrefab != null) {
         // create a new tile from the provided prefab with 2d transform and no rotation
         GameObject tile = MonoBehaviour.Instantiate(tilePrefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;

         // give the tile a name (it is visible in the scene view when the tiles grid is added to the scene
         tile.name = string.Format("Tile ({0},{1})", x, y);

         // the Tile script was added to the TileNormal prefab; add it to the grid
         board.TileGrid.SetTileAt(x, y, tile.GetComponent<Tile>());

         // initialize after parenting so that the first element is a child of the board
         board.TileGrid.GetTileAt(x, y).Initialize(x, y, this);

         // cause our tile to be a child of the game board
         tile.transform.parent = board.transform;
      }
   }

   /// <summary>
   /// Highlights a tile border with the given color that is given or with the default color.  It can
   /// also be turned off.
   /// </summary>
   /// <param name="x"></param>
   /// <param name="y"></param>
   /// <param name="on"></param>
   public void HighlightTile(int x, int y, bool on = true, Color? color = null) {
      SpriteRenderer r = board.TileGrid.GetTileAt(x, y).GetComponent<SpriteRenderer>();
      Color colorToUse = (color != null) ? (Color) color : r.color;
      r.color = new Color(colorToUse.r, colorToUse.g, colorToUse.b, on ? 1f : 0f);
   }

   public void HightlightTilesForPieces(List<GamePiece> gamePieces) {
      foreach (GamePiece piece in gamePieces) {
         if (piece != null) {
            HighlightTile(piece.X, piece.Y, true, piece.GetComponent<SpriteRenderer>().color);
         }
      }
   }

   /// <summary>
   /// Highlights a game piece so that a user gets a visible cue.
   /// </summary>
   /// <param name="tile"></param>
   /// <param name="on"></param>
   public void HighlightGamePiece(Tile tile, bool on = true) {
      GamePiece piece = board.GamePieceGrid.GetPieceAt(tile.X, tile.Y);
      if (piece != null) {
         Color tmp = piece.GetComponent<SpriteRenderer>().color;
         tmp.a = on ? 0.5f : 1f;
         piece.GetComponent<SpriteRenderer>().color = tmp;
      }
   }

   public void OnClickTile(Tile tile) {
      if (this.clickedTile == null) {
         this.clickedTile = tile;
         HighlightGamePiece(clickedTile);
      }
   }

   public void OnTileHoverEnter(Tile tile) {
      if (this.clickedTile != null && board.TileGrid.IsAdjacent(tile, this.clickedTile)) {
         this.targetTile = tile;
         HighlightGamePiece(targetTile);
      }
   }

   public void OnTileHoverExit(Tile tile) {
      if (clickedTile != null && this.clickedTile != tile) {
         HighlightGamePiece(tile, false);
      }
   }

   public void OnTileDragRelease() {
      if (this.clickedTile != null) {
         if (this.targetTile != null) {
            SwapTiles(this.clickedTile, this.targetTile);
            HighlightGamePiece(clickedTile, false);
            HighlightGamePiece(targetTile, false);
         } else {
            // ensure the clicked tile has alpha at full
            HighlightGamePiece(clickedTile, false);
         }
      }

      this.clickedTile = null;
      this.targetTile = null;
   }

   public void SwapTiles(Tile clickedTile, Tile targetTile) {
      board.StartCoroutine(SwapTilesRoutine(clickedTile, targetTile));
   }

   IEnumerator SwapTilesRoutine(Tile clickedTile, Tile targetTile) {
      if (board.IsPlayerInputAllowed) {
         // get the pieces to swap
         GamePiece clickedPiece = board.GamePieceGrid.GetPieceAt(clickedTile.X, clickedTile.Y);
         GamePiece targetPiece = board.GamePieceGrid.GetPieceAt(targetTile.X, targetTile.Y);

         if (targetPiece != null && clickedPiece != null && !clickedPiece.IsMoving && !targetPiece.IsMoving) {
            // move them 
            board.MovementService.Move(clickedPiece, targetTile.X, targetTile.Y, board.SwapTime);
            board.MovementService.Move(targetPiece, clickedTile.X, clickedTile.Y, board.SwapTime);

            // seems a race condition was happening sometimes (piece didn't finish move before findmatchat) 
            // so adding a little time to allow for it to finish
            yield return new WaitForSeconds(board.SwapTime + 0.05f);

            // swap back if no match
            List<GamePiece> clickedPieceMatches = board.MatchingService.FindMatchesAt(clickedTile.X, clickedTile.Y);
            List<GamePiece> targetPieceMatches = board.MatchingService.FindMatchesAt(targetTile.X, targetTile.Y);
            if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0) {
               board.MovementService.Move(clickedPiece, clickedTile.X, clickedTile.Y, board.SwapTime);
               board.MovementService.Move(targetPiece, targetTile.X, targetTile.Y, board.SwapTime);
            } else {
               yield return new WaitForSeconds(board.SwapTime);

               board.ClearingService.ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
            }
         }
      }

   }

}
