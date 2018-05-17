using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum TileType {
   Normal, Breakable, Obstacle
}

/// <summary>
/// This should be a component of the TileNormal Prefab.  It represents the white square box and starts at (0,0).
/// We set the alpha to zero to make it transparent.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour {

   [SerializeField]
   protected TileType tileType = TileType.Normal;

   private int x;
   private int y;

   protected SpriteRenderer spriteRenderer;

   protected TileGridService tileGridService;

   protected virtual void Awake() {
      this.spriteRenderer = GetComponent<SpriteRenderer>();
      Assert.IsNotNull(spriteRenderer, "Missing spriteRenderer on GameObject; did you add this script to a GameObject that has no SpriteRenderer?");
   }

   protected virtual void Start() { }

   /// <summary>
   /// Initialize the tile with the given values.  (x,y) represent its location
   /// </summary>
   /// <param name="x">(x,y) represent its location in the tiles grid</param>
   /// <param name="y">(x,y) represent its location in the tiles grid</param>
   /// <param name="board">A reference to the parent game board</param>
   public virtual void Initialize(int x, int y, TileGridService tileGridService) {
      if (tileGridService == null) {
         throw new System.ArgumentException("Invalid tileGridService; it cannot be null!");
      }
      this.x = x;
      this.y = y;
      this.tileGridService = tileGridService;
   }

   protected virtual void OnMouseDown() {
      tileGridService.OnClickTile(this);
   }

   protected virtual void OnMouseEnter() {
      tileGridService.OnTileHoverEnter(this);
   }

   protected virtual void OnMouseExit() {
      tileGridService.OnTileHoverExit(this);
   }

   protected virtual void OnMouseUp() {
      tileGridService.OnTileDragRelease();
   }

   public void ChangeBorderColor(Color color) {
      this.spriteRenderer.color = color;
   }

   public Color GetBorderColor() {
      return this.spriteRenderer.color;
   }

   public int X { get { return this.x; } }
   public int Y { get { return this.y; } }
   public TileType TileType { get { return this.tileType; } }
}
