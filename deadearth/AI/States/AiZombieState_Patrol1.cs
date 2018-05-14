using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.AI;

/// <summary>
/// An Ai state that implements the patrol animation behavior for zombie-like ai entities.
/// </summary>
public class AiZombieState_Patrol1 : AiZombieState {

   [SerializeField] private float angleNeededForTurning;
   [SerializeField] private float slerpSpeed = 5.0f;
   [SerializeField] [Range(0.0f, 3.0f)] private float speed = 3.0f;

   /// <summary>
   /// Callback that is fired when this state first becomes active.
   /// </summary>
   public override void OnEnterState() {
      base.OnEnterState();

      zombieStateMachine.ModifyNavAgentUpdateAttributes(true, false);
      zombieStateMachine.Seeking = SeekingType.None;
      zombieStateMachine.Feeding = false;
      zombieStateMachine.AttackType = 0;

      // If the current target is not a waypoint then we need to select
      // a waypoint from te waypoint network and make this the new target
      // and plot a path to it
      if (!zombieStateMachine.ThreatManager.IsTargeting(AiTargetType.Waypoint)) {
         zombieStateMachine.ThreatManager.StopTrackingTarget(); // Clear any previous target
         zombieStateMachine.WaypointManager.TrackWayPoint();
      }

      // Make sure NavAgent is switched on
      this.zombieStateMachine.NavAgent.isStopped = false;
   }

   /// <summary>
   /// Returns the default state.
   /// </summary>
   /// <returns>Patrol</returns>
   public override AiStateType GetDefaultStateType() {
      return AiStateType.Patrol;
   }

   /// <summary>
   /// Called by the state machine each frame.
   /// </summary>
   /// <returns>Either the current state or a new state.</returns>
   public override AiStateType OnUpdate() {
      // setting it here so changes in inspector take immediate affect
      this.zombieStateMachine.Speed = this.speed;

      AiTarget? potentialThreat = this.zombieStateMachine.ThreatManager.DeterminePotentialThreat();
      AiStateType state = this.zombieStateMachine.ThreatManager.DetermineNextPotentialThreatState(potentialThreat);

      if (state == AiStateType.None) {
         state = GetDefaultStateType();
      } else {
         bool isFoodThreat = this.zombieStateMachine.ThreatManager.DoesFoodThreatExist();
         if (isFoodThreat && !this.zombieStateMachine.CanHungerBeSatisfied((AiTarget) potentialThreat)) {
            state = GetDefaultStateType();
         } else {
            this.zombieStateMachine.ThreatManager.TrackTarget((AiTarget) potentialThreat);
         }
      }

      if (state == GetDefaultStateType()) {
         state = HandleDefaultState();
      }

      return state;
   }

   /// <summary>
   /// Handles the patrol by continuing to pursue waypoints, etc.
   /// </summary>
   /// <returns>Either the patrol state or any new state if certain conditions were met.</returns>
   private AiStateType HandleDefaultState() {
      AiStateType state = GetDefaultStateType();
      if (zombieStateMachine.NavAgent.pathPending) {
         // let the navmeshagent path complete before checking for state change, etc.
         zombieStateMachine.Speed = 0;
         return state;
      }

      zombieStateMachine.Speed = this.speed;

      // Calculate angle we need to turn to be facing our target
      angleNeededForTurning = this.zombieStateMachine.ThreatManager.DetermineAngleNeededToTurnTowardsTarget();

      // If its too big then drop out of Patrol and into Alerted
      if (Mathf.Abs(angleNeededForTurning) > this.turnOnSpotThreshold) {
         state = AiStateType.Alerted;
      } else {
         // If root rotation is not being used then we are responsible for keeping zombie rotated
         // and facing in the right direction. 
         if (!this.zombieStateMachine.RootMotionProperties.ShouldUseRootRotation) {
            // Generate a new Quaternion representing the rotation we should have
            Quaternion newRotation = Quaternion.LookRotation(this.zombieStateMachine.NavAgent.desiredVelocity);

            // Smoothly rotate to that new rotation over time
            this.zombieStateMachine.transform.rotation = Quaternion.Slerp(
               this.zombieStateMachine.AiEntityBodyTransform.rotation,
               newRotation,
               Time.deltaTime * this.slerpSpeed
            );
         }

         // If for any reason the nav agent has lost its path then send it to next waypoint 
         if (zombieStateMachine.HasLostNavMeshPath()) {
            zombieStateMachine.WaypointManager.SetNextWayPoint();
            zombieStateMachine.WaypointManager.TrackWayPoint();
         }
      }

      return state;
   }

   /// <summary>
   /// Callback that is fired by the parent state machine when the zombie has reached its target.
   /// That is, its collider has entered the target's collider with "Is Trigger" enabled.
   /// </summary>
   /// <param name="isReached">whether it has reached it or not</param>
   public override void OnDestinationReached(bool isReached) {
      base.OnDestinationReached(isReached);

      // Only interesting in processing arrivals not departures
      if (isReached) {
         // Select the next waypoint in the waypoint network
         if (this.zombieStateMachine.ThreatManager.IsTargeting(AiTargetType.Waypoint)) {
            zombieStateMachine.WaypointManager.SetNextWayPoint();
            zombieStateMachine.WaypointManager.TrackWayPoint();
         }
      }
   }
}