using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contains static utilities related to making position/rotation calculations within our game.
/// </summary>
public class CalculationUtil {

   // prevent instantiation
   private CalculationUtil() { }

   /// <summary>
   /// Computes both the world position and world rotation (i.e. world space) of the given collider.  It
   /// uses lossyScale which takes into account to position and rotation of all the ancestor gameobjects
   /// the collider is a child of (basicaly traverses up the hierarchy).  
   /// Alternative to computing both at the same time, the individual methods below can be used as well.
   /// </summary>
   /// <param name="col"></param>
   /// <param name="worldPosition"></param>
   /// <param name="worldRadius"></param>
   public static void ConvertSphereColliderToWorldSpace(
      SphereCollider collider, 
      out Vector3 worldPosition, 
      out float worldRadius
   ) {
      // Default Values
      worldPosition = Vector3.zero;
      worldRadius = 0.0f;

      if (collider != null) {
         // Calculate world space position of sphere center
         worldPosition = collider.transform.position;
         worldPosition.x += collider.center.x * collider.transform.lossyScale.x;
         worldPosition.y += collider.center.y * collider.transform.lossyScale.y;
         worldPosition.z += collider.center.z * collider.transform.lossyScale.z;

         // Calculate world space radius of sphere
         worldRadius = Mathf.Max(
            collider.radius * collider.transform.lossyScale.x,
            collider.radius * collider.transform.lossyScale.y
         );

         worldRadius = Mathf.Max(worldRadius, collider.radius * collider.transform.lossyScale.z);
      }
   }

   /// <summary>
   /// Computes the world position of the given sensor by using the lossy scale which takes into account the ancestor
   /// positions.
   /// Since sensor is a child with a local position, we need to ensure its position scales correctly when parent
   /// values change.  This can be accomplished by multiplying the lossyScale value to the center value.  so, it is
   /// the world space position of the center of our Sensor GameObject that is a child of the "AI Entity" GameObject.
   /// </summary>
   /// <param name="sphere">A SphereCollider with it's center on the head of an AI Entity game object.  It will have
   /// a local position and radius which need to take into account any changes to ancestors it might be under.</param>
   /// <returns>The world space position of the sensor.</returns>
   public static Vector3 ComputeWorldPosition(SphereCollider sphere) {
      Vector3 point = sphere.transform.position;

      point.x += sphere.center.x * sphere.transform.lossyScale.y;

      point.y +=
         // e.g. this is the sensor.collider center.y offset value (e.g. 1.6), which is roughly the height of the entity head
         sphere.center.y *

         // lossyscale is something that is calculated as the hierarchy works its way up
         sphere.transform.lossyScale.y;

      point.z += sphere.center.z * sphere.transform.lossyScale.z;

      return point;
   }

   /// <summary>
   /// Computes the world radius of the given sensor by using the lossy scale which takes into account the ancestor
   /// positions.
   /// </summary>
   /// <param name="sphere">A SphereCollider with it's center on the head of an AI Entity game object.</param>
   /// <returns>The world space radius of the sensor.</returns>
   public static float ComputeWorldRadius(SphereCollider sphere) {
      float radius = Mathf.Max(
         sphere.radius * sphere.transform.lossyScale.x,
         sphere.radius * sphere.transform.lossyScale.y
      );

      return Mathf.Max(
         radius,
         sphere.radius * sphere.transform.lossyScale.z
      );
   }

   /// <summary>
   /// Returns the signed angle between two positions (in degrees).
   /// </summary>
   /// <param name="fromVector"></param>
   /// <param name="toVector"></param>
   /// <returns></returns>
   public static float FindSignedAngle(Vector3 fromVector, Vector3 toVector) {
      float angle = 0f;

      if (fromVector != toVector) {
         angle = Vector3.Angle(fromVector, toVector);
         Vector3 cross = Vector3.Cross(fromVector, toVector);
         angle *= Mathf.Sign(cross.y);
      }

      return angle;
   }

}
