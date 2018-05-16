using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ClearingService {

   private GameBoard board;

   public ClearingService(GameBoard board) {
      if (board == null) {
         throw new System.ArgumentException("Invalid board; it cannot be null!");
      }

      this.board = board;
   }

   public void ClearPieceAt(int x, int y) {
      GamePiece piece = board.GamePieceGrid.GetPieceAt(x, y);
      if (piece != null) {
         board.GamePieceGrid.SetPieceAt(x, y, null);
         MonoBehaviour.Destroy(piece.gameObject);
      }
      board.TileGridService.HighlightTile(x, y, false);
   }

   public void ClearPieceAt(List<GamePiece> gamePieces) {
      foreach (GamePiece piece in gamePieces) {
         ClearPieceAt(piece.X, piece.Y);
      }
   }

   public void ClearGrid() {
      for (int x = 0; x < board.GamePieceGrid.Width; x++) {
         for (int y = 0; y < board.GamePieceGrid.Height; y++) {
            ClearPieceAt(x, y);
         }
      }
   }

   public void ClearAndRefillBoard(List<GamePiece> gamePieces) {
      board.StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
   }

   private IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces) {
      board.IsPlayerInputAllowed = false; // we don't want our players changing things will clear/fill is active
      List<GamePiece> matches = gamePieces;

      do {
         // clear and collapse
         // wait for Coroutine to finish before continuing on (i.e. yield return in front of it)
         yield return board.StartCoroutine(ClearAndCollapseRoutine(gamePieces));
         yield return null;

         //refill
         yield return board.StartCoroutine(RefillEmptyRoutine());
         matches = board.MatchingService.FindAllMatches();

         yield return new WaitForSeconds(0.5f); // a small wait after we find our matches
      } while (matches.Count != 0);

      board.IsPlayerInputAllowed = true;
   }

   IEnumerator RefillEmptyRoutine() {
      board.GamePieceGridService.FillEmptyGamePieceGridSlots(10, 0.5f);
      yield return null;
   }

   private IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces) {

      List<GamePiece> movingPieces = new List<GamePiece>();
      List<GamePiece> matches = new List<GamePiece>();

      board.TileGridService.HightlightTilesForPieces(gamePieces);

      yield return new WaitForSeconds(0.5f);

      while (true) {
         ClearPieceAt(gamePieces);
         yield return new WaitForSeconds(0.25f);
         movingPieces = CollapseColumn(gamePieces);

         while (!IsCollapsed(movingPieces)) {
            yield return null;
         }

         yield return new WaitForSeconds(0.1f);
         matches = board.MatchingService.FindMatchesAt(movingPieces);
         if (matches.Count == 0) {
            break;
         } else {
            yield return board.StartCoroutine(ClearAndCollapseRoutine(matches));
         }
      }

      yield return null;
   }

   bool IsCollapsed(List<GamePiece> gamePieces) {
      foreach (GamePiece piece in gamePieces) {
         if (piece != null) {
            if (piece.transform.position.y - (float) piece.Y > Mathf.Epsilon) {
               return false;
            }
         }
      }
      return true;
   }

   private List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f) {
      List<GamePiece> movingPieces = new List<GamePiece>(); // the pieces that will need to move down

      for (int i = 0; i < board.GamePieceGrid.Height - 1; i++) {
         GamePiece missingPiece = board.GamePieceGrid.GetPieceAt(column, i);
         if (missingPiece == null && board.TileGrid.GetTileAt(column, i).TileType != TileType.Obstacle) {
            for (int j = i + 1; j < board.GamePieceGrid.Height; j++) {
               // the top most non-null piece will become the missing piece so we can slide it down
               GamePiece piece = board.GamePieceGrid.GetPieceAt(column, j); 
               if (piece != null) {
                  missingPiece = piece;
                  if (!movingPieces.Contains(missingPiece)) {
                     missingPiece.SetCoordinates(column, i);
                     board.GamePieceGrid.SetPieceAt(column, j, null);
                     board.MovementService.Move(missingPiece, column, i, collapseTime * (j - i));
                     movingPieces.Add(missingPiece);
                  } else {
                     Debug.Log("found " + missingPiece.ToString); // not sure the are ever already there
                  }

                  break;
               }
            }
         }
      }

      return movingPieces;
   }

   public List<GamePiece> CollapseColumn(List<GamePiece> gamePieces) {
      List<GamePiece> movingPieces = new List<GamePiece>();

      List<int> columnsToCollapse = GamePieceGrid.GetColumns(gamePieces);
      foreach (int column in columnsToCollapse) {
         movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
      }

      return movingPieces;
   }
}
