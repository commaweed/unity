using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WaypointDisplayMode { Lines, Paths }
public class AiWaypointNetwork : MonoBehaviour {

   [SerializeField]
   private List<Transform> waypoints = new List<Transform>();
   public List<Transform> Waypoints {
      get { return this.waypoints; }
   }

   [HideInInspector]
   [SerializeField]
   private WaypointDisplayMode displayMode;
   public WaypointDisplayMode DisplayMode {
      get { return this.displayMode; }
      set { this.displayMode = value; }
   }

   [HideInInspector]
   [SerializeField]
   private int pathStartIndex;
   public int PathStartIndex {
      get { return this.pathStartIndex; }
      set { this.pathStartIndex = value; }
   }

   [HideInInspector]
   [SerializeField]
   private int pathEndIndex;
   public int PathEndIndex {
      get { return this.pathEndIndex; }
      set { this.pathEndIndex = value; }
   }

   public Transform GetStartWaypoint() {
      return GetWaypoint(pathStartIndex);
   }

   public Transform GetEndWaypoint() {
      return GetWaypoint(pathEndIndex);
   }

   public Transform GetWaypoint(int index) {
      Transform result = null;

      // wrap around
      index = index % waypoints.Count;
      if (index < 0) {
         index = waypoints.Count + index;
      }

      result = waypoints[index];

      return result;
   }

   public Transform GetFirstNonNullWaypoint(int index) {
      Transform result = null;
      
      for (int i=0; i < waypoints.Count; i++) {
         result = GetWaypoint(index + i);
         if (result != null) {
            break;
         }
      }

      return result;
   }


}
