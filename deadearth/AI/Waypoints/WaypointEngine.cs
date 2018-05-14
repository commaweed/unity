using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This engine is used to build waypoint connections and retrieve waypoints in a given waypoint network.
/// Working with the index offsets that Gary provided seemed a little problematic at first.  That and
/// it seemed a better approach to compute all the next-destinations one time versus doing it each time
/// it was the next one was requested.
/// </summary>
public class WaypointEngine {

   private AiWaypointNetwork waypointNetwork;
   public AiWaypointNetwork WaypointNetwork { get { return this.waypointNetwork; } }

   // all the non-null waypoints (connected together)
   private List<Waypoint> waypoints = new List<Waypoint>();

   /// <summary>
   /// Initialize.
   /// </summary>
   /// <param name="waypointNetwork">A waypoint network with 0 or more connected waypoints.</param>
   public WaypointEngine(AiWaypointNetwork waypointNetwork) {
      // did you drag your waypoint network to the AI Entity?
      Assert.IsNotNull(waypointNetwork, "Invalid waypointNetwork; it cannot be null!  Check your inspector!");

      // Does your network any waypoints assigned to it?
      Assert.IsNotNull(waypointNetwork.Waypoints, "Invalid network.Waypoints; it cannot be null!!");
      if (waypointNetwork.Waypoints.Count == 0) {
         Debug.LogWarning("The given waypoint network exists, but no waypoints have been added to it!");
      }
      this.waypointNetwork = waypointNetwork;

      // build the contiguos network and pre-compute all the next waypoint destinations 
      BuildWaypoints();
   }

   /// <summary>
   /// Builds the waypoint network connections according to the waypoints that have been added to it (i.e. non null ones).
   /// Each non-null waypoints will be assigned a reference to the next waypoint now.  
   /// </summary>
   private void BuildWaypoints() {
      // build the chain of non-null waypoints and their associated NextWaypoint waypoint "up-front"
      // thus, every waypoint will have a reference to a valid "next" waypoint in the chain from the beginning (no need to compute it each time)
      Waypoint previous = null;
      for (int i = 0; i < WaypointNetwork.Waypoints.Count; i++) {
         Waypoint waypoint = new Waypoint(i, WaypointNetwork.Waypoints[i]);
         if (WaypointNetwork.Waypoints[i] != null) {
            waypoints.Add(waypoint);

            if (previous != null) {
               previous.NextWaypoint = waypoint;
            }

            previous = waypoint;
         }
      }

      // set the NextWaypoint for the last record
      if (waypoints.Count > 1) {
         waypoints[waypoints.Count - 1].NextWaypoint = waypoints[0];
      }
   }

   /// <summary>
   /// Returns an index that is within the bounds of the waypoints collection.  Since we are still working with indices, 
   /// we need to handle boundaries of the collection. 
   /// TODO:  since we mostly use the indices to determine path in the editor, see if we can do away with it altogether.
   /// </summary>
   /// <param name="index">The index to normalize.</param>
   /// <returns>An index value that is within the bounds of the collection indices.</returns>
   private int NormalizeIndex(int index) {
      index = index % WaypointNetwork.Waypoints.Count;
      if (index < 0) {
         index = WaypointNetwork.Waypoints.Count + index;
      }
      return index;
   }

   /// <summary>
   /// Returns the waypoint at the given index, but only after normalizing the index (the index go 
   /// be out of bounds and we need to ensure we map it to one that fits in the boundaries.  If there
   /// are no waypoints in the network, it will return null.  If there are gaps in the collection,
   /// it will return the next one it can find by iterating over the collection until it finds the
   /// next highest index. 
   /// Note, this isn't meant to be used to get the "Next" waypoint.  Once you have a valid waypoint, 
   /// just call waypoint.NextWaypoint() to get the next one in the chain (remember it is pre-computed).
   /// </summary>
   /// <param name="index">The index to use in the lookup.</param>
   /// <returns>The waypoint at the given index (or the next waypoing in the connection)</returns>
   public Waypoint GetWaypoint(int index) {
      return waypoints
         .Where(waypoint => waypoint.Index >= NormalizeIndex(index)) // filter until we find an index that matches
         .FirstOrDefault();   // return that waypoint at that index, but if not found, return null
      ;
   }

}
