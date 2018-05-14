using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this to the main camera gameobject to give the ability to rotate the camera when the game is running.
/// Used in the early stages when testing.  Later removed for Gary's versions. 
/// </summary>
public class CameraRotate : MonoBehaviour {

   [SerializeField]
   private float sensitivity = 5F;

   [SerializeField]
   private float minThreshold = -25f;

   [SerializeField]
   private float maxThreshold = 90f;

   [SerializeField]
   private float height = 18f;

   /// <summary>
   /// Inspector angles are in human readable circular degrees.  
   /// </summary>
   /// <param name="angle">The non-inspector angle value.</param>
   /// <returns>The value of an angle as it would be seen in the inspector</returns>
   private static float TranslateToInspectorAngle(float angle) {
      float result = angle % 360;
      return result > 180 ? result - 360 : result;
   }

   /// <summary>
   /// Inspector angles are in human readable circular degrees.  Converts the given Euler angle to the
   /// inspector angle.
   /// </summary>
   /// <param name="angle">The non-inspector angle value.</param>
   /// <returns>The value of an angle as it would be seen in the inspector</returns>
   private static float TranslateFromInspectorAngle(float angle) {
      if (angle >= 0) return angle;
      return 360 - (-angle % 360);
   }

   /// <summary>
   /// Monobehavior lifecycle method that is called after all Update functions have been called. 
   /// This is useful to order script execution. For example a follow camera should always be implemented in LateUpdate 
   /// because it tracks objects that might have moved inside Update.
   /// </summary>
   void LateUpdate() {
      this.transform.position = new Vector3(28, height, -10);

      // on right mouse, rotate around y axis
      if (Input.GetMouseButton(1)) {

         float rotationX = Input.GetAxis("Mouse X") * sensitivity;
         float rotationY = -1 * Input.GetAxis("Mouse Y") * sensitivity;

         Vector3 newValues = new Vector3(
               transform.localEulerAngles.x + rotationY,
               transform.localEulerAngles.y + rotationX,
               transform.localEulerAngles.z
         );

         // stay within the angle bounds, as seen in the inspector
         float translatedX = TranslateToInspectorAngle(newValues.x);
         if (translatedX < minThreshold) newValues.x = TranslateFromInspectorAngle(minThreshold);
         if (translatedX > maxThreshold) newValues.x = TranslateFromInspectorAngle(maxThreshold);

         this.transform.localEulerAngles = newValues;
      }
   }
}

