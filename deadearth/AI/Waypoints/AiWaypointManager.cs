using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Manages all of the waypoint logic for an AiStateMachine that is associated with a particular AI Entity.
/// </summary>
public class AiWaypointManager : MonoBehaviour {

   /// <summary>
   /// Represents the patrol network for the AI Entity.  You can preconfigure any number of networks by first creating a bunch of
   /// waypoints and then creating the network and dragging one or more of those waypoints into the network.  Then drag that 
   /// network onto the AI Entity to assign it that patrol route.
   /// </summary>
   [SerializeField] private AiWaypointNetwork waypointNetwork;
   //public AiWaypointNetwork WaypointNetwork { get { return this.waypointNetwork; } }

   // allows the zombie to look as if it isn't following a pattern of sorts
   [SerializeField] protected bool shouldPatrolRandomly;
   //public bool ShouldPatrolRandomly { get { return this.shouldPatrolRandomly; } }

   // this will display the current waypoint in the network the zombie is pursuing when it is in the patrol state
   [SerializeField] private string currentWaypointDisplay;
   //public string CurrentWaypointDisplay { set { this.currentWaypointDisplay = value; } }

   // a reference to the parent AiStateMachine
   private AiStateMachine stateMachine;

   // the waypoint the AI Entity is currently pursuing
   private Waypoint currentWaypoint;

   /*
   Rather than use numeric indices which seemed somewhat problematic and which involved computing the next waypoint, this 
   engine is designed to compute all the non-null waypoints up-front with their corresponding next-waypoints.  It still
   supports the numeric indices for the Editor path display that Gary demonstrated.
   */
   private WaypointEngine waypointEngine;

   /// <summary>
   /// Monobehavior life cyle method that is called before start.  Initializes all of the required components and sets up the initial
   /// state.
   /// </summary>
   protected virtual void Awake() {
      this.stateMachine = GetComponent<AiStateMachine>();
      Assert.IsNotNull(
         stateMachine, 
         "Missing stateMachine sybling component; did you add this to an AI Entity gameobject that has an AiStateMachine script component?"
      );
      Assert.IsNotNull(
         this.waypointNetwork,
         "Missing waypointNetwork; did you forget to add it in the inspector?"
      );
      InitializeWaypointNetwork();
   }

   /// <summary>
   /// Initialize the waypoint network, to include creating an error if it does not exist.  Also
   /// sets the current waypoint to the next waypoint, taking into account shouldPatrolRandomly. 
   /// </summary>
   private void InitializeWaypointNetwork() {
      this.waypointEngine = new WaypointEngine(this.waypointNetwork);
      SetNextWayPoint();
   }

   /// <summary>
   /// Sets the current waypoint to the next waypoint, taking into account the shouldPatrolRandomly value.
   /// </summary>
   public void SetNextWayPoint() {
      if (this.shouldPatrolRandomly) {
         this.currentWaypoint = this.waypointEngine.GetWaypoint(Random.Range(0, this.waypointNetwork.Waypoints.Count));
      } else {
         this.currentWaypoint = currentWaypoint == null ? this.waypointEngine.GetWaypoint(0) : this.currentWaypoint.NextWaypoint;
      }

      // this just updates the display in the inspector so that we can see which waypoint the entity is heading towards
      this.currentWaypointDisplay = this.currentWaypoint.ToString();
   }

   /// <summary>
   /// Causes the AI Entity to walk towards the current waypoint.  It does this by causing it's active target
   /// to be a waypoint and not a threat such as the player, a light, a sound, etc.
   /// </summary>
   public void TrackWayPoint() {
      if (this.currentWaypoint != null) {
         // track the current way point
         stateMachine.ThreatManager.TrackWaypoint(this.currentWaypoint);

         // Tell NavAgent to make a path to this waypoint
         stateMachine.NavAgent.SetDestination(this.currentWaypoint.Transform.position);

         // ensure navagent resumes (if it is stopped) - the condition check might not be necessary
         if (stateMachine.NavAgent.isStopped) {
            stateMachine.NavAgent.isStopped = false;
         }
      }
   }
}
