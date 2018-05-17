using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MatchDirection {
   Left, Right, Up, Down
}

public class MatchingService  {

   private const int MIX_LENGTH = 3;

   private GameBoard board;

   public MatchingService(GameBoard board) {
      if (board == null) {
         throw new System.ArgumentException("Invalid board; it cannot be null!");
      }
      this.board = board;
   }

   public bool HasMatchOnBuild(int x, int y, int minLength = MIX_LENGTH) {
      // array (0,0) is bottom left so fills right then up (don't have to recheck what we already checked)
      List<GamePiece> leftMatches = FindMatches(x, y, MatchDirection.Left, minLength);
      if (leftMatches.Count < minLength) leftMatches.Clear();
      List<GamePiece> downMatches = FindMatches(x, y, MatchDirection.Down, minLength);
      if (downMatches.Count < minLength) downMatches.Clear();
      return (leftMatches.Count >= minLength || downMatches.Count >= minLength); // (leftMatches.Count > 0 || downMatches.Count > 0)
   }

   public List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = MIX_LENGTH) {
      List<GamePiece> matches = new List<GamePiece>();
      foreach (GamePiece piece in gamePieces) {
         matches = matches.Union(FindMatchesAt(piece.X, piece.Y, minLength)).ToList();
      }
      return matches;
   }

   public List<GamePiece> FindMatchesAt(int startX, int startY, int minLength = MIX_LENGTH) {
      List<GamePiece> hMatches = FindHorizontalMatches(startX, startY, minLength);
      List<GamePiece> vMatches = FindVerticalMatches(startX, startY, minLength);
      return hMatches.Union(vMatches).ToList();
   }

   private List<GamePiece> FindMatches(int startX, int startY, MatchDirection matchDirection, int minLength = MIX_LENGTH) {
      List<GamePiece> matches = new List<GamePiece>();

      GamePiece startPiece = board.GamePieceGrid.GetPieceAt(startX, startY);
      if (startPiece != null) {
         Vector2 searchDirection = GetSearchDirection(matchDirection);
         matches.Add(startPiece);

         int maxValue = ComputeTotalSearchCount(board.GamePieceGrid.Width, board.GamePieceGrid.Height, startX, startY, matchDirection);
         for (int i = 0; i < maxValue; i++) {
            int nextX = startX + (int) Mathf.Clamp(searchDirection.x, -1, 1) * (i + 1);
            int nextY = startY + (int) Mathf.Clamp(searchDirection.y, -1, 1) * (i + 1);
            if (!board.GamePieceGrid.IsWithinBounds(nextX, nextY)) {
               break;
            }

            GamePiece nextPiece = board.GamePieceGrid.GetPieceAt(nextX, nextY);
            if (nextPiece != null && nextPiece.MatchType == startPiece.MatchType && !matches.Contains(nextPiece)) {
               matches.Add(nextPiece);
            } else {
               break;
            }
         }

         //if (matches.Count < minLength)
         //   matches.Clear();
      }

      return matches;
   }

   private List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = MIX_LENGTH) {
      List<GamePiece> right = FindMatches(startX, startY, MatchDirection.Right, 2);
      List<GamePiece> left = FindMatches(startX, startY, MatchDirection.Left, 2);
      List<GamePiece> matches = right.Union(left).ToList();
      if (matches.Count < minLength)
         matches.Clear();
      return matches;
   }

   private List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = MIX_LENGTH) {
      List<GamePiece> up = FindMatches(startX, startY, MatchDirection.Up, 2);
      List<GamePiece> down = FindMatches(startX, startY, MatchDirection.Down, 2);
      List<GamePiece> matches = up.Union(down).ToList();
      if (matches.Count < minLength)
         matches.Clear();
      return matches;
   }

   public List<GamePiece> FindAllMatches() {
      List<GamePiece> matches = new List<GamePiece>();
      for (int x = 0; x < board.GamePieceGrid.Width; x++) {
         for (int y = 0; y < board.GamePieceGrid.Height; y++) {
            matches = matches.Union(FindMatchesAt(x, y)).ToList();
         }
      }
      return matches;
   }

   /// <summary>
   /// 
   /// </summary>
   /// <param name="matchDirection"></param>
   /// <returns></returns>
   public static Vector2 GetSearchDirection(MatchDirection matchDirection) {
      switch (matchDirection) {
         case MatchDirection.Up:
            return new Vector2(0, 1);
         case MatchDirection.Down:
            return new Vector2(0, -1);
         case MatchDirection.Left:
            return new Vector2(-1, 0);
         case MatchDirection.Right:
            return new Vector2(1, 0);
      }
      throw new System.ArgumentException("Received unhandled matchDirection " + matchDirection);
   }

   /// <summary>
   /// Determine the number of searches needed according to the given direction and coordinates.  By doing this
   /// we can compute the exact amount of searches to make.
   /// </summary>
   /// <param name="height"></param>
   /// <param name="width"></param>
   /// <param name="x"></param>
   /// <param name="y"></param>
   /// <param name="matchDirection"></param>
   /// <returns></returns>
   public static int ComputeTotalSearchCount(int width, int height, int x, int y, MatchDirection matchDirection) {
      switch (matchDirection) {
         case MatchDirection.Up:
            return height - y;
         case MatchDirection.Down:
            return y;
         case MatchDirection.Left:
            return x;
         case MatchDirection.Right:
            return width - x;
      }
      throw new System.ArgumentException("Received unhandled matchDirection " + matchDirection);
   }

   //public void HightlightMatchesAt(int x, int y) {
   //   board.TileGridService.HighlightTile(x, y, false); // turn off highlight (is this necessary)
   //   foreach (GamePiece piece in board.MatchingService.FindMatchesAt(x, y)) {
   //      // turn on highlight using the game piece color
   //      board.TileGridService.HighlightTile(piece.X, piece.Y, true, piece.GetComponent<SpriteRenderer>().color);
   //   }
   //}

   //public void HighlightAllMatches() {
   //   for (int x = 0; x < board.GamePieceGrid.Width; x++) {
   //      for (int y = 0; y < board.GamePieceGrid.Height; y++) {
   //         HightlightMatchesAt(x, y);
   //      }
   //   }
   //}

}
