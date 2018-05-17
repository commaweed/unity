using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleService {

   private GameBoard board;

   public ParticleService(GameBoard board) {
      if (board == null) {
         throw new System.ArgumentException("Invalid board; it cannot be null!");
      }

      this.board = board;
   }

   private GameObject CreateParticleEffect(GameObject prefab, int x, int y, int z = 0) {
      return GameObject.Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
   }

   public void PlayEffectAt(GameObject prefab, int x, int y, int z = 0) {
      if (prefab != null) {
         GameObject fx = CreateParticleEffect(prefab, x, y, z);
         ParticlePlayer player = fx.GetComponent<ParticlePlayer>();
         if (player) {
            player.Play();
         }
      }
   }

   public void ClearPieceFxAt(int x, int y, int z = 0) {
      PlayEffectAt(board.ClearFxPrefab, x, y, z);
   }

   public void BreakTileFxAt(int x, int y, int z = 0) {
      PlayEffectAt(board.BreakFxPrefab, x, y, z);
   }

   public void BreakDoubleTileFxAt(int x, int y, int z = 0) {
      PlayEffectAt(board.DoubleBreakFxPrefab, x, y, z);
   }
}
