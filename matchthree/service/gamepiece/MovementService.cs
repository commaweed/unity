using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementService  {

   private GameBoard board;

   public MovementService(GameBoard board) {
      if (board == null) {
         throw new System.ArgumentException("Invalid board; it cannot be null!");
      }
      this.board = board;
   }

   /// <summary>
   /// Move this game piece to the new (x,y) location in the amount of time given.
   /// </summary>
   /// <param name="x"></param>
   /// <param name="y"></param>
   /// <param name="timeToMove"></param>
   public void Move(GamePiece piece, int x, int y, float timeToMove) {
      if (piece != null && !piece.IsMoving) {
         board.StartCoroutine(MoveRoutine(piece, new Vector3(x, y, 0), timeToMove));
      }
   }

   /// <summary>
   /// Move the game piece gradually.
   /// </summary>
   /// <param name="destination">The final desired destination (x,y) position.</param>
   /// <param name="timeToMove">The time in seconds we want the move to take.</param>
   /// <returns></returns>
   IEnumerator MoveRoutine(GamePiece piece, Vector3 destination, float timeToMove) {
      Vector3 start = piece.transform.position;  // store the original position (to use in the lerp)
      float elapsedTime = 0f;
      piece.IsMoving = true;

      // loop until we reach our final destination
      while (true && piece != null && piece.IsMoving) { // the piece may have been destroyed while running
         // if we are close enough, we are there
         if (Vector3.Distance(piece.transform.position, destination) < Mathf.Epsilon) { // epsilon is a very small number
            piece.transform.position = destination; // the final value it will get

            // because lerp works with floats, it make it look choppy when position is in between two grid coordinates when it stops
            // so clamp it to an int position
            piece.SetCoordinates(destination);

            board.GamePieceGridService.PlaceGamePiece(piece, destination);

            break;
         }

         // deltaTime = how long it took to run last frame
         elapsedTime += Time.deltaTime;

         // protects against invalid values (but note that Vector3.Lerp has a Clamp built in)
         float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);

         // gradually move towards the destination using the new time t 
         UseInterpolationMotion(piece, start, destination, t);

         // wait until next frame to continue execution (still in while loop when control returns)
         yield return null;
      }

      piece.IsMoving = false;
   }

   /// <summary>
   /// Linear motion is not that pleasing.  Better to use some sort of curve, such as the built in ones or
   /// an AnimationCurve.  The following will use the currently selected interpolation value.
   /// </summary>
   /// <param name="start">The start position</param>
   /// <param name="end">The end position</param>
   /// <param name="time">The time to take to move gradually</param>
   private void UseInterpolationMotion(GamePiece piece, Vector3 start, Vector3 end, float time) {

      switch (piece.interpolation) {
         case InterpolationType.Linear:
            piece.transform.position = Vector3.Lerp(start, end, time);
            break;
         case InterpolationType.EaseIn:
            piece.transform.position = Vector3.Lerp(start, end, 1 - Mathf.Cos(time * Mathf.PI * 0.5f));
            break;
         case InterpolationType.EaseOut:
            piece.transform.position = Vector3.Lerp(start, end, Mathf.Sin(time * Mathf.PI * 0.5f));
            break;
         case InterpolationType.SmootherStep:
            piece.transform.position = Vector3.Lerp(start, end, Mathf.Pow(time, 2) * (3 - 2 * time));
            break;
         case InterpolationType.SmoothStep:
            piece.transform.position = Vector3.Lerp(start, end, Mathf.Pow(time, 3) * (time * (time * 6 - 15) + 10));
            break;
         default:
            Debug.LogWarning("You chose an interpolation type that has not been configured.");
            break;
      }

      piece.transform.position = Vector3.Lerp(start, end, time);
   }
}
