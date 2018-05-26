using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rpg.Environment {
   /// <summary>
   /// Rotates the pinwheel on the windmill.
   /// </summary>
   public class SpinMe : MonoBehaviour {

      [SerializeField] float xRotationsPerMinute = 1f;
      [SerializeField] float yRotationsPerMinute = 1f;
      [SerializeField] float zRotationsPerMinute = 1f;

      void Update() {
         float FORMULA = Time.deltaTime / 60 * 360;

         float xDegreesPerFrame = FORMULA * xRotationsPerMinute; // TODO COMPLETE ME
         transform.RotateAround(transform.position, transform.right, xDegreesPerFrame);

         float yDegreesPerFrame = FORMULA * yRotationsPerMinute; // TODO COMPLETE ME
         transform.RotateAround(transform.position, transform.up, yDegreesPerFrame);

         float zDegreesPerFrame = FORMULA * zRotationsPerMinute; // TODO COMPLETE ME
         transform.RotateAround(transform.position, transform.forward, zDegreesPerFrame);
      }
   }
}
