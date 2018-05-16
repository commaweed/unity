using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;


public class GamePieceGrid {

   private GamePiece[,] values;

   private int width;
   public int Width { get { return this.width; } }
   private int height;
   public int Height { get { return this.height; } }

   /// <summary>
   /// Initialize.
   /// </summary>
   /// <param name="gamePiecePrefabs"></param>
   /// <param name="width"></param>
   /// <param name="height"></param>
   public GamePieceGrid(int width, int height) {
      if (width < 1) {
         throw new System.ArgumentException("Invalid width; it must be > 0!");
      }
      if (height < 1) {
         throw new System.ArgumentException("Invalid height; it must be > 0!");
      }
      this.width = width;
      this.height = height;
      this.values = new GamePiece[width, height];
   }

   public bool IsWithinBounds(int x, int y) {
      return x >= 0 && x < width && y >= 0 && y < height;
   }

   public bool IsEmpty(int x, int y) {
      return !IsWithinBounds(x, y) || values[x, y] == null;
   }

   public GamePiece GetPieceAt(int x, int y) {
      return IsWithinBounds(x, y) ? values[x, y] : null;
   }

   public void SetPieceAt(int x, int y, GamePiece piece) {
      if (IsWithinBounds(x, y)) {
         this.values[x, y] = piece;
      }
   }

   public static List<int> GetColumns(List<GamePiece> gamePieces) {
      List<int> columns = new List<int>();
      foreach (GamePiece piece in gamePieces) {
         if (!columns.Contains(piece.X)) {
            columns.Add(piece.X);
         }
      }
      return columns;
   }
}
