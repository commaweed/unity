using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour {

   [SerializeField]
   private AiWaypointNetwork wayPointNetwork;

   [SerializeField]
   private int currentIndex;

   private NavMeshAgent navAgent;

	// Use this for initialization
	void Start () {
      navAgent = GetComponent<NavMeshAgent>();
      SetWaypointDestination(false);
   }

   private void SetWaypointDestination(bool shouldIncrement) {
      if (this.wayPointNetwork == null) { return; }

      int newIndex = (shouldIncrement ? 1 : 0) + currentIndex;
      Transform destination = this.wayPointNetwork.GetFirstNonNullWaypoint(newIndex);
      if (destination != null) {
         this.navAgent.destination = destination.position;
      }
   }

   // Update is called once per frame
   void Update () {
      HandleFindNextWayPoint();
   }

   private void HandleFindNextWayPoint() {
      if (!this.navAgent.hasPath && !this.navAgent.pathPending) {
         SetWaypointDestination(true);
      } else if (this.navAgent.isPathStale) {
         SetWaypointDestination(false);
      }
   }
}
