using System;
using UnityEngine;
using UnityEngine.AI;

namespace UnityStandardAssets.Characters.ThirdPerson {

   public class EnemyAICharacterControl : AICharacterControl {

      //[SerializeField]
      //private AiWaypointNetwork wayPointNetwork;

      //[SerializeField]
      //private int currentWaypointIndex;

      //bool foundPlayer = false;
      //public bool FoundPlayer {
      //   get { return this.foundPlayer; }
      //   set {
      //      this.foundPlayer = value;

      //      if (this.foundPlayer) {
      //         this.agent.speed = 3.5f;
      //      } else {
      //         this.agent.speed = 1f;
      //      }
      //   }
      //}

      //private new void Start() {
      //   base.Start();
      //   SetTarget(null);
      //   SetWaypointDestination(false);
      //}

      //private new void Update() {
      //   HandleFindNextWayPoint();
      //   HandleAgentStoppingDistance();
      //}

      //private void HandleFindNextWayPoint() {
      //   if (!this.agent.hasPath && !this.agent.pathPending) {
      //      SetWaypointDestination(true);
      //   } else if (this.agent.isPathStale) {
      //      SetWaypointDestination(false);
      //   }
      //}

      //private void SetWaypointDestination(bool shouldIncrement) {
      //   if (this.wayPointNetwork == null || this.foundPlayer) { return; }

      //   currentWaypointIndex = (shouldIncrement ? 1 : 0) + currentWaypointIndex;
      //   Transform destination = this.wayPointNetwork.GetFirstNonNullWaypoint(currentWaypointIndex);
      //   if (destination != null) {
      //      this.target = destination;
      //      this.SetAgentDestination();
      //   }
      //}

      //public override void SetTarget(Transform target) {
      //   //if (this.target != target) { // if the target changes
      //      this.target = target;
      //   //}
      //}

   }
}
