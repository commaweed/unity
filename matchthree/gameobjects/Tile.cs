using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType {
   Normal, Obstacle
}

/// <summary>
/// This should be a component of the TileNormal Prefab.  It represents the white square box and starts at (0,0).
/// We set the alpha to zero to make it transparent.
/// </summary>
public class Tile : MonoBehaviour {

   [SerializeField]
   private TileType tileType = TileType.Normal;

   private int x;
   private int y;

   private TileGridService tileGridService;

   /// <summary>
   /// Initialize the tile with the given values.  (x,y) represent its location
   /// </summary>
   /// <param name="x">(x,y) represent its location in the tiles grid</param>
   /// <param name="y">(x,y) represent its location in the tiles grid</param>
   /// <param name="board">A reference to the parent game board</param>
   public void Initialize(int x, int y, TileGridService tileGridService) {
      if (tileGridService == null) {
         throw new System.ArgumentException("Invalid tileGridService; it cannot be null!");
      }
      this.x = x;
      this.y = y;
      this.tileGridService = tileGridService;
   }

   private void OnMouseDown() {
      tileGridService.OnClickTile(this);
   }

   private void OnMouseEnter() {
      tileGridService.OnTileHoverEnter(this);
   }

   private void OnMouseExit() {
      tileGridService.OnTileHoverExit(this);
   }

   private void OnMouseUp() {
      tileGridService.OnTileDragRelease();
   }

   public int X { get { return this.x; } }
   public int Y { get { return this.y; } }
   public TileType TileType { get { return this.tileType; } }
}
