using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BreakableTile : Tile {

   
  
   [SerializeField] private Sprite[] breakableSprites;
   [SerializeField] private Color initialColor;
   [SerializeField] private Color normalColor;

   // corresponds to how many times we can break the tile; each time we see a match, we trigger a break; convert to normal tile when at zero
   // todo maybe use an enum for this
   private int breakableValue;
   public int BreakableValue { get { return this.breakableValue; } }

   protected override void Awake() {
      base.Awake();
      Assert.IsNotNull(breakableSprites, "Missing breakableSprites[]; did you forget to add it to the inspector");
      Assert.IsTrue(breakableSprites.Length > 0, "Invalid breakableSprites[] length; did you forget to set length in inspector?");
      breakableValue = breakableSprites.Length - 1;
   }

   protected override void Start() {
      base.Start();
      this.tileType = TileType.Breakable;
      ChangeBorderColor(this.initialColor);
   }

   public override void Initialize(int x, int y, TileGridService tileGridService) {
      base.Initialize(x, y, tileGridService);
      RenderBreakingSprite();
   }

   public bool IsBreakable() {
      return breakableValue > 0 && breakableValue < breakableSprites.Length;
   }

   public void BreakTile() {
      if (!IsBreakable()) { return; }
      breakableValue--;
      StartCoroutine(BreakTileRoutine());
   }

   private void RenderBreakingSprite() {
      if (IsBreakable() && breakableSprites[breakableValue] != null) {
         this.spriteRenderer.sprite = breakableSprites[breakableValue];
         ChangeBorderColor(initialColor);
      } else {
         this.tileType = TileType.Normal;  // we are now a normal tile (other code can treat us as such)
         ChangeBorderColor(normalColor);
      }
   }

   IEnumerator BreakTileRoutine() { 
      yield return new WaitForSeconds(0.25f);
      RenderBreakingSprite();
   }
}
