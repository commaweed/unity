using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Ai state that implements the Idle animation behavior for zombie-like ai entities.
/// </summary>
public class AiZombieState_Idle1 : AiZombieState {

   [SerializeField] Vector2 idleTimeRange = new Vector2(10f, 60f);

   // when we transition into the idle state, we only want to remain there for the range specified by idleTimeRane
   // so we need to keep track of how long we have been in the state and take action when outside range
   float maxDuration = 0f;
   float timer = 0f;

   /// <summary>
   /// Returns the default state.
   /// </summary>
   /// <returns>Idle</returns>
   public override AiStateType GetDefaultStateType() {
      return AiStateType.Idle;
   }

   /// <summary>
   /// Callback that is fired when this state first becomes active.
   /// </summary>
   public override void OnEnterState() {
      base.OnEnterState();

      ResetTimer();

      this.zombieStateMachine.ModifyNavAgentUpdateAttributes(true, false);
      this.zombieStateMachine.Speed = 0f;
      this.zombieStateMachine.Seeking = SeekingType.None;
      this.zombieStateMachine.Feeding = false;
      this.zombieStateMachine.AttackType = 0;
      this.zombieStateMachine.ThreatManager.StopTrackingTarget(); 
   }

   /// <summary>
   /// Called by the state machine each frame.
   /// </summary>
   /// <returns>Either Idle or a new state based upon the threats that were processed</returns>
   public override AiStateType OnUpdate() {
      UpdateTimer();

      AiTarget? potentialThreat = this.zombieStateMachine.ThreatManager.DeterminePotentialThreat();
      AiStateType state = this.zombieStateMachine.ThreatManager.DetermineNextPotentialThreatState(potentialThreat);

      if (state == AiStateType.None) {
         if (HasReachedMaxTime()) {
            zombieStateMachine.WaypointManager.TrackWayPoint();
            state = AiStateType.Alerted;
         } else {
            state = GetDefaultStateType();
         }
      } else {
         this.zombieStateMachine.ThreatManager.TrackTarget((AiTarget) potentialThreat);
      }

      return state;
   }

   /// <summary>
   /// Resets the timer, to include the max duration value.
   /// </summary>
   private void ResetTimer() {
      this.maxDuration = Random.Range(idleTimeRange.x, idleTimeRange.y);
      this.timer = 0f;
   }

   /// <summary>
   /// Updates the timer.
   /// </summary>
   private void UpdateTimer() {
      timer += Time.deltaTime;
   }

   /// <summary>
   /// Indicates whether or not the max time has been reached.
   /// </summary>
   /// <returns></returns>
   private bool HasReachedMaxTime() {
      return timer > maxDuration;
   }
}
