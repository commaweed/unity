using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Ai state that implements the Alerted animation behavior for zombie-like ai entities.
/// </summary>
public class AiZombieState_Alerted1 : AiZombieState {

   [SerializeField] [Range(1, 60)] float maxDuration = 10.0f;
   [SerializeField] float threatAngleThreshold = 10.0f;
   [SerializeField] float directionChangeTime = 1.5f;

   float directionChangeTimer = 0.0f;

   private float timer = 0f;

   /// <summary>
   /// Callback that is fired when this state first becomes active.
   /// </summary>
   public override void OnEnterState() {
      base.OnEnterState();

      // Configure State Machine
      zombieStateMachine.ModifyNavAgentUpdateAttributes(true, false); // we want animation to control rotation
      zombieStateMachine.Speed = 0;
      zombieStateMachine.Seeking = 0;
      zombieStateMachine.Feeding = false;
      zombieStateMachine.AttackType = 0;

      ResetMaxDurationTimer();
      ResetDirectionChangeTimer();
   }

   /// <summary>
   /// Returns the default state.
   /// </summary>
   /// <returns>Alerted</returns>
   public override AiStateType GetDefaultStateType() {
      return AiStateType.Alerted;
   }

   /// <summary>
   /// Called by the state machine each frame.
   /// </summary>
   /// <returns>Either the current state or a new state.</returns>
   public override AiStateType OnUpdate() {
      UpdateTimers();

      if (HasReachedMaxTime()) {
         // reset the waypoint but stay in the alerted state with a fresh timer
         zombieStateMachine.WaypointManager.TrackWayPoint();
         ResetMaxDurationTimer();
      }

      AiTarget? potentialThreat = zombieStateMachine.ThreatManager.DeterminePotentialThreat();
      AiStateType state = zombieStateMachine.ThreatManager.DetermineNextPotentialThreatState(potentialThreat);

      if (state == AiStateType.None) {
         state = GetDefaultStateType();
      } else {
         AiTarget newThreat = (AiTarget) potentialThreat;
         if (newThreat.Type == AiTargetType.Visual_Player) {
            zombieStateMachine.ThreatManager.TrackTarget(newThreat);
         } else {
            if (newThreat.Type == AiTargetType.Audio || newThreat.Type == AiTargetType.Visual_Light) {
               zombieStateMachine.ThreatManager.TrackTarget(newThreat);
               ResetMaxDurationTimer();
            } 

            if (newThreat.Type == AiTargetType.Visual_Food) {
               if (zombieStateMachine.ThreatManager.DoesTargetExist()) {
                  state = GetDefaultStateType(); // food is less of a priority, so reset back to alerted state (audio or light)
               } else {
                  zombieStateMachine.ThreatManager.TrackTarget(newThreat);
               }
            } 
         }
      }

      if (state == GetDefaultStateType()) {
         state = HandleDefaultState();
      }

      return state;
   }

   /// <summary>
   /// Handles the alerted state by continuing to pursue waypoints, etc.
   /// </summary>
   /// <returns>Either the alerted state or any new state if certain conditions were met.</returns>
   private AiStateType HandleDefaultState() {
      AiStateType state = GetDefaultStateType();

      if (
         !zombieStateMachine.IsTargetReached &&
         (zombieStateMachine.ThreatManager.DoesAudioThreatExist() || zombieStateMachine.ThreatManager.DoesLightThreatExist())
      ) {
         // we got close to the target due to light or sound, so now we need to know if we should pursue it or seek to find it
         float angle = CalculationUtil.FindSignedAngle(
            zombieStateMachine.AiEntityBodyTransform.forward,
            zombieStateMachine.ThreatManager.CurrentTarget.Position - zombieStateMachine.AiEntityBodyTransform.position
         );
         
         if (zombieStateMachine.ThreatManager.DoesAudioThreatExist() && Mathf.Abs(angle) < this.threatAngleThreshold) { 
            // it's a sound and we are capable of heading to it, so pursue it
            state = AiStateType.Pursuit;
         } else if (HasReachedMaxDirectionChangeTime()) {
            // it's not a sound and we are not capable of turning towards it, so determine which way we should turn
            if (Random.value < zombieStateMachine.Intelligence) {
               SeekTowards(angle);  // smartly turn
            } else {
               SeekRandomly();      // randomly turn because we are a stupid zombie :)
            }
            ResetDirectionChangeTimer();  // TODO: this does sometimes happen often and it makes the zombie appear as if it is stuck in the alerted state
         }
      } else if (
         zombieStateMachine.ThreatManager.IsTargeting(AiTargetType.Waypoint) &&
         !zombieStateMachine.NavAgent.pathPending
      ) {
         // we were targeting a waypoint and we arrived at it, so determine if we can head to next one or turn towards it
         float angle = zombieStateMachine.ThreatManager.DetermineAngleNeededToTurnTowardsTarget();

         if (Mathf.Abs(angle) < this.turnOnSpotThreshold) {
            state = AiStateType.Patrol;
         } else if (HasReachedMaxDirectionChangeTime()) {
            SeekTowards(angle);
            ResetDirectionChangeTimer();
         }
      } else if (HasReachedMaxDirectionChangeTime()) {
         // we didn't find what we were looking for and our clock ran out, so turn randomly and repeat the entire alert process
         // TODO: this does sometimes happen often and it makes the zombie appear as if it is stuck in the alerted state
         SeekRandomly();
         ResetDirectionChangeTimer();
      }

      return state;
   }

   /// <summary>
   /// Perform a seek in the direction of the given angle.  Seek left on negative and right on positive.
   /// </summary>
   /// <param name="angle">The angle to seek towards</param>
   private void SeekTowards(float angle) {
      zombieStateMachine.SetNumericSeeking((int) Mathf.Sign(angle));
   }

   /// <summary>
   /// Performs a seek in a random direction, left or right.  Seeking causes the zombie to turn in that direction and
   /// seek its current target.
   /// </summary>
   private void SeekRandomly() {
      zombieStateMachine.SetNumericSeeking((int) Mathf.Sign(Random.Range(-1.0f, 1.0f)));
   }

   /// <summary>
   /// Updates the timer and indicates whether or not the max time has been reached.
   /// </summary>
   /// <returns></returns>
   private bool HasReachedMaxTime() {
      return this.timer < 0.0f;
   }
   
   /// <summary>
   /// Indicates whether or not the max direction change time has been reached.
   /// </summary>
   /// <returns>true if the max direction change time has been reached</returns>
   private bool HasReachedMaxDirectionChangeTime() {
      return directionChangeTimer > directionChangeTime;
   }

   /// <summary>
   /// Updates all of the timers.
   /// </summary>
   private void UpdateTimers() {
      timer -= Time.deltaTime;
      directionChangeTimer += Time.deltaTime;
   }

   /// <summary>
   /// Resets the max duration timer.
   /// </summary>
   private void ResetMaxDurationTimer() {
      timer = maxDuration;
   }

   /// <summary>
   /// Resets the direction change timer.
   /// </summary>
   private void ResetDirectionChangeTimer() {
      directionChangeTimer = 0.0f;
   }
}
