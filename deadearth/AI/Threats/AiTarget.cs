using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// certain things can be aggrevators to the AI Entity and they can become a target
public enum AiTargetType { None, Waypoint, Visual_Player, Visual_Light, Visual_Food, Audio }

/// <summary>
/// Represents a target that can be tracked by an AIEntity.
/// </summary>
public struct AiTarget {

   private AiTargetType type;
   private Collider collider;
   private Vector3 position;
   private float distance;       // keep track of distance to target's position
   private float timeSeen;       // keep time updated so we know when entity last saw it

   public AiTargetType Type { get { return this.type; } }
   public Collider Collider { get { return this.collider; } }
   public Vector3 Position { get { return this.position; } }
   public float Distance { get { return this.distance; } set { this.distance = value; } }
   public float TimeSeen { get { return this.timeSeen; } }

   /// <summary>
   /// Sets the target the given target type..
   /// </summary>
   /// <param name="type">Any type but None</param>
   /// <param name="collider">The collider for the target.</param>
   /// <param name="position">The position of the target.</param>
   /// <param name="distance"></param>
   public void SetTarget(AiTargetType type, Collider collider, Vector3 position, float distance) {
      if (type == AiTargetType.None) {
         throw new System.ArgumentException("Invalid type; use Clear() to set target type back to None!");
      }
      if (collider == null) {
         throw new System.ArgumentException("Invalid collider; it must exist!");
      }
      this.type = type;
      this.collider = collider;
      this.position = position;
      this.distance = distance;
      this.timeSeen = Time.time;
   }

   /// <summary>
   /// Sets the target as the given waypoint.
   /// </summary>
   /// <param name="position">The transform position of the waypoint destination.</param>
   /// <param name="distance">The current distance the AI Entity is from the waypoint.</param>
   public void SetWayPoint(Vector3 position, float distance) {
      this.type = AiTargetType.Waypoint;
      this.collider = null;
      this.position = position;
      this.distance = distance;
      this.timeSeen = Time.time;
   }

   /// <summary>
   /// Clears all the target values and reset them to a non-tracking state.
   /// </summary>
   public void Clear() {
      this.type = AiTargetType.None;
      this.collider = null;
      this.position = Vector3.zero;
      this.distance = 0.0f;
      this.timeSeen = Mathf.Infinity;
   }

   /// <summary>
   /// Updates the distance by calculating the distance between itself and the given entity transform.
   /// </summary>
   /// <param name="entityTransform">The entity transform to use in the distance computation.</param>
   public void UpdateDistance(Vector3 entityPosition) {
      this.distance = Vector3.Distance(entityPosition, this.position);
   }

   /// <summary>
   /// Returns a string representation of this object that can be used in the inspector (we only have so much room to show it).
   /// </summary>
   /// <returns>The type and distance from it.</returns>
   public override string ToString() {
      return string.Format("[{0}], [{1}]", this.Type, this.distance);
   }

   /// <summary>
   /// Returns the collider ID of the active/current target or -1 is there isn't one.
   /// </summary>
   /// <returns>The collider ID or -1.</returns>
   public int GetColliderID() {
      return this.collider != null ? this.collider.GetInstanceID() : -1;
   }

}
