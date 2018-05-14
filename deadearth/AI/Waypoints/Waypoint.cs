using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaypointDisplayMode { None, Lines, Paths }

/// <summary>
/// Represents a waypoint gameobject that can be added to a waypoint network.
/// </summary>
public class Waypoint {

   private Transform transform;
   private int index;

   /// <summary>
   /// Initialize.
   /// </summary>
   /// <param name="index">The waypoing index in a waypoint network.</param>
   /// <param name="transform">The transform location in the scene view.</param>
   public Waypoint(int index, Transform transform) {
      this.index = index;
      this.transform = transform;
   }

   public Transform Transform { get { return this.transform; } }
   public int Index { get { return this.index; } }
   public Waypoint NextWaypoint { get; set; }

   /// <summary>
   /// Returns a string representation of this waypoint.  It can be used in the Inspector to show the current waypoint
   /// the AiEntity is targeting.
   /// </summary>
   /// <returns></returns>
   public override string ToString() {
      return string.Format(
         "{0}: {1} -> {2} : {3}",
         Index,
         Transform.gameObject == null ? "null" : Transform.gameObject.name,
         NextWaypoint == null ? "null" : NextWaypoint.Transform.gameObject.name,
         Transform.position
      );
   }
}
