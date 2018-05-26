using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rpg.CameraUi {

   public class CameraFollow : MonoBehaviour {

      [SerializeField]
      private GameObject player;

      [SerializeField]
      private float sensitivity = 5F;


      // Use this for initialization
      void Start() {
         // instead of using GUI, could use the following to find the player gameobject
         //GameObject notUsedPlayerRef = GameObject.FindGameObjectWithTag("Player");
         //print(notUsedPlayerRef.ToString());


      }

      // Update is called once per frame
      void Update() {

      }

      private static float TranslateToInspectorAngle(float angle) {
         float result = angle % 360;
         return result > 180 ? result - 360 : result;
      }

      private static float TranslateFromInspectorAngle(float angle) {
         if (angle >= 0) return angle;
         return 360 - (-angle % 360);
      }

      /// <summary>
      /// LateUpdate is called after all Update functions have been called. 
      /// This is useful to order script execution. For example a follow camera should always be implemented in LateUpdate 
      /// because it tracks objects that might have moved inside Update.
      /// </summary>
      void LateUpdate() {
         this.transform.position = player.transform.position;

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
            if (translatedX < -25) newValues.x = TranslateFromInspectorAngle(-25);
            if (translatedX > 45) newValues.x = TranslateFromInspectorAngle(45);

            this.transform.localEulerAngles = newValues;
         }
      }
   }
}
