using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

/// <summary>
/// Cause agents to navigate the navmesh using a network of waypoints, but without animation.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MyAgentWaypointMovement : MonoBehaviour {

   [SerializeField]
   private AiWaypointNetwork network;

   [SerializeField]
   private bool hasPath;
   [SerializeField]
   private bool pathPending;
   [SerializeField]
   private bool isPathStale;
   [SerializeField]
   private float remainingDistance;
   [SerializeField]
   private string currentWaypointDisplay;

   private NavMeshAgent navAgent;
   private Waypoint currentWaypoint;
   private WaypointEngine engine;

   public AnimationCurve JumpCurve = new AnimationCurve();

   // Use this for initialization
   void Start () {
      Assert.IsNotNull(this.network, "NavAgentExample: 'Way Point Network' is missing; did you forget to set it in inspector!");
      navAgent = GetComponent<NavMeshAgent>();

      engine = new WaypointEngine(this.network);

      if (this.currentWaypoint == null) {
         this.currentWaypoint = engine.GetWaypoint(network.PathStartIndex);
      }

      SetWaypointDestination(false);
   }

   // Update is called once per frame
   void Update() {
      HandleOffMeshLink();
      HandleFindNextWayPoint();
   }

   /// <summary>
   /// Sets the waypoint destination for the nav agent
   /// </summary>
   /// <param name="shouldIncrement"></param>
   private void SetWaypointDestination(bool shouldIncrement) {
      if (this.network == null) { return; }

      currentWaypoint = (shouldIncrement ? currentWaypoint.NextWaypoint : currentWaypoint);
      
      if (currentWaypoint != null && currentWaypoint.Transform != null) {
         this.currentWaypointDisplay = currentWaypoint.ToString();
         this.navAgent.destination = currentWaypoint.Transform.position;
      }
   }

   /// <summary>
   /// Handles the finding of the next waypoint.
   /// </summary>
   private void HandleFindNextWayPoint() {
      this.hasPath = this.navAgent.hasPath;
      this.pathPending = this.navAgent.pathPending;
      this.isPathStale = this.navAgent.isPathStale;
      this.remainingDistance = this.navAgent.remainingDistance;

      //!this.hasPath
      if ((this.remainingDistance <= this.navAgent.stoppingDistance && !this.pathPending) || 
         this.navAgent.pathStatus == NavMeshPathStatus.PathInvalid
      ) {
         SetWaypointDestination(true);
      } else if (this.isPathStale) {
         SetWaypointDestination(true);
      }
   }

   /// <summary>
   /// If agent is on an offmesh link then perform a jump (it is waiting for us to do something)
   /// </summary>
   private void HandleOffMeshLink() {
      if (this.navAgent.isOnOffMeshLink) {
         StartCoroutine(Jump(1.0f));
         return;
      }
   }

   /// <summary>
   /// Jump to destination on off mesh link.
   /// </summary>
   /// <param name="duration">The total time in seconds to complete the jump.</param>
   /// <returns></returns>
   IEnumerator Jump(float duration) {
      // Get the current OffMeshLink data
      OffMeshLinkData data = this.navAgent.currentOffMeshLinkData;

      // Start Position is agent current position
      Vector3 startPos = this.navAgent.transform.position;

      // End position is fetched from OffMeshLink data and adjusted for baseoffset of agent
      Vector3 endPos = data.endPos + (this.navAgent.baseOffset * Vector3.up);

      // Used to keep track of time
      float time = 0.0f;

      // Keep iterating for the passed duration
      while (time <= duration) {
         // Calculate normalized time
         float t = time / duration;

         // Lerp between start position and end position and adjust height based on evaluation of t on Jump Curve
         this.navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * Vector3.up);

         // Accumulate time and yield each frame
         time += Time.deltaTime;
         yield return null;
      }

      // All done so inform the agent it can resume control
      this.navAgent.CompleteOffMeshLink();
   }
}
