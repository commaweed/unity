using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the tiles grid which is the white grid in the background.  We set the alpha transparency to 0.
/// </summary>
public class TileGrid {

   private Tile[,] values;

   private int width;
   public int Width { get { return this.width; } }
   private int height;
   public int Height { get { return this.height; } }

   public TileGrid(int width, int height) {
      if (width < 1) {
         throw new System.ArgumentException("Invalid width; it must be > 0!");
      }
      if (height < 1) {
         throw new System.ArgumentException("Invalid height; it must be > 0!");
      }
      this.width = width;
      this.height = height;
      this.values = new Tile[width, height];
   }

   public bool IsEmpty(int x, int y) {
      return !IsWithinBounds(x, y) || values[x, y] == null;
   }

   public Tile GetTileAt(int x, int y) {
      return (IsWithinBounds(x, y)) ? values[x, y] : null;
   }

   public void SetTileAt(int x, int y, Tile tile) {
      if (IsWithinBounds(x, y)) {
         this.values[x, y] = tile;
      }
   }

   public bool IsWithinBounds(int x, int y) {
      return x >= 0 && x < width && y >= 0 && y < height;
   }

   /// <summary>
   /// Indicates whether or not the start tile is adjacent to the end tile horizontally or vertically.
   /// </summary>
   /// <param name="start"></param>
   /// <param name="end"></param>
   /// <returns></returns>
   public bool IsAdjacent(Tile start, Tile end) {
      return (Mathf.Abs(start.X - end.X) == 1 && start.Y == end.Y) ||
         (Mathf.Abs(start.Y - end.Y) == 1 && start.X == end.X);
   }

}
