using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SceneService {

   private GameBoard board;

   public SceneService(GameBoard board) {
      if (board == null) {
         throw new System.ArgumentException("Invalid board; it cannot be null!");
      }
      this.board = board;
   }
 
   /// <summary>
   /// Initialize the camera so that our tiles grid will fit with a border perfectly within the window.  I think we
   /// are assuming a 1080x1920 screen.
   /// </summary>
   private void InitializeCamera() {
      Assert.IsNotNull(Camera.main, "Missing main camera; did you add a camera to the scene and Tag it as 'MainCamera'!");

      float ASPECT_RATIO = Screen.width / (float) Screen.height;

      // center the camera at world position 0 (10 units out on z axis)
      Camera.main.transform.position = new Vector3((board.Width - 1f) / 2f, (board.Height - 1f) / 2f, -10);

      float verticalOrthoSize = (float) board.Height / 2f + (float) board.BorderSize;
      float horizontalOrthoSize = ((float) board.Width / 2f + (float) board.BorderSize) / ASPECT_RATIO;

      Camera.main.orthographicSize = Mathf.Max(verticalOrthoSize, horizontalOrthoSize);
   }

}
