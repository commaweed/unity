using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Represents an AI waypoint network.  It will contain 0 or more waypoints that can be connected together.
/// AI entities can use it for things like patrolling.
/// </summary>
public class AiWaypointNetwork : MonoBehaviour {

   /// <summary>
   /// Represents potential Waypoints in a Waypoint network.  The size is set in the Inspector and waypoints are
   /// dragged to the various index positions.  It does allow for no waypoint in any of the positions.  The
   /// associated Waypoint engine was built to handle missing waypoints.
   /// </summary>
   [SerializeField]
   private List<Transform> waypoints = new List<Transform>();

   /// <summary>
   /// Used for testing only.  It can show in the inspector information about the current waypoint an AI Entity is
   /// headed towards.
   /// </summary>
   [HideInInspector]
   [SerializeField]
   private WaypointDisplayMode displayMode;

   /// <summary>
   /// The index of the start path.  It is used in the editor when the mode is set to Path and it will show the navmesh
   /// path using this as its start waypoint, provided that a waypoint with that index actually exists.
   /// </summary>
   [HideInInspector]
   [SerializeField]
   private int pathStartIndex;

   /// <summary>
   /// The index of the end path.  It is used in the editor when the mode is set to Path and it will show the navmesh
   /// path using this as its end waypoint, provided that a waypoint with that index actually exists.
   /// </summary>
   [HideInInspector]
   [SerializeField]
   private int pathEndIndex;


   public List<Transform> Waypoints { get { return this.waypoints; } }
   public WaypointDisplayMode DisplayMode {
      get { return this.displayMode; }
      set { this.displayMode = value; }
   }

   /// <summary>
   /// Sets/Gets the starting index for a navmesh Path.  This is mostly used in the Editor to display
   /// the calculated navmesh path between two points; it is also used in the other test animator controller
   /// scripts that Gary demonstrated.
   /// </summary>
   public int PathStartIndex {
      get { return this.pathStartIndex; }
      set { this.pathStartIndex = value; }
   }

   /// <summary>
   /// Sets/Gets the ending index for a navmesh path.  This is mostly used in the Editor to display
   /// the calculated navmesh path between two points; it is also used in the other test animator controller
   /// scripts that Gary demonstrated.
   /// </summary>
   public int PathEndIndex {
      get { return this.pathEndIndex; }
      set { this.pathEndIndex = value; }
   }
}
