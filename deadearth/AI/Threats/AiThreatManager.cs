using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents data and functions that interact with all the threats and targets of an AiEntity. 
/// The AiStateMachine will hold a reference of this and interact with it as needed.
/// </summary>
public class AiThreatManager {

   private SphereCollider targetTrigger;
   private AiStateMachine stateMachine;

   // the visual target for the entity, if any
   private AiTarget visualThreat = new AiTarget();

   // the audio target for the entity, if any
   private AiTarget audioThreat = new AiTarget();

   // the current target the entity is interested in
   private AiTarget target = new AiTarget();

   /// <summary>
   /// Initialize
   /// </summary>
   /// <param name="targetTrigger">The TargetTrigger that is sent out from an AI Entity GameObject to any 
   /// target it intends on pursuing.</param>
   /// <param name="stateMachine">A reference to the parent state machine</param>
   public AiThreatManager(
      SphereCollider targetTrigger, 
      AiStateMachine stateMachine
   ) {
      if (targetTrigger == null) {
         throw new System.ArgumentException(
            "Missing targetTrigger; did you forget to drag it onto the AI Entity GameObject in the inspector!"
         );
      }

      // this will only happen if it is false to begin with; nothing happens if changed after
      if (targetTrigger.isTrigger == false) {
         throw new System.ArgumentException(
            "Invalid targetTrigger; make sure to set 'Is Trigger' to true in the inspector!"
         );
      }

      this.targetTrigger = targetTrigger;
      this.stateMachine = stateMachine; // not necessary now, but just in case
   }

   /// <summary>
   /// Updates all the threats and targets.  This is meant to be called at fixed times via the physics FixedUpdate().
   /// </summary>
   /// <param name="entityPosition">The AiEntity's current position; if it is currently tracking a target, it
   /// will need to update the distance, etc.</param>
   public void PerformFixedUpdate(Vector3 entityPosition) {
      // visual and audio threats are going to be cleared each update (not sure why we do this because my zombie gets stuck in alerted sometimes)
      this.visualThreat.Clear();
      this.audioThreat.Clear();

      if (this.target.Type != AiTargetType.None) {
         this.target.UpdateDistance(entityPosition);
      }
   }

   /// <summary>
   /// Set the target using the given values, but use the state machine's stoppingDistance value.
   /// </summary>
   public void TrackTarget(AiTargetType type, Collider collider, Vector3 position, float distance) {
      this.TrackTarget(type, collider, position, distance, this.stateMachine.StoppingDistance);
   }

   /// <summary>
   /// Tracks the target using the given values.
   /// </summary>
   public void TrackTarget(AiTargetType type, Collider collider, Vector3 position, float distance, float stoppingDistance) {
      this.target.SetTarget(type, collider, position, distance);
      MoveTargetTriggerToTarget(stoppingDistance);
   }

   /// <summary>
   /// Tracks the target using the values of the given target.
   /// </summary>
   public void TrackTarget(AiTarget target) {
      this.TrackTarget(target.Type, target.Collider, target.Position, target.Distance, this.stateMachine.StoppingDistance);
   }

   // TODO: readd the method that takes the AiTarget.  It was removed due to an issue with passByValue that was resolved in a different location

   /// <summary>
   /// Tracks the given waypoint.  Cause targetTrigger sphere to be placed at the waypoint's position.
   /// </summary>
   public void TrackWaypoint(Waypoint waypoint) {
      this.target.SetWayPoint(
         waypoint.Transform.position,
         Vector3.Distance(stateMachine.AiEntityBodyTransform.position, waypoint.Transform.position) // stateMachine.transform.position
      );
      MoveTargetTriggerToTarget(this.stateMachine.StoppingDistance);
   }

   /// <summary>
   /// Updates the visual threat.  Because these are private, I learned it passes by value when using getter
   /// and I'll either have to make it public or have this method to set it.
   /// </summary>
   /// <param name="type"></param>
   /// <param name="collider"></param>
   /// <param name="distance"></param>
   public void TrackVisualThreat(AiTargetType type, Collider collider, float distance) {
      this.visualThreat.SetTarget(
         type,
         collider,
         collider.transform.position,
         distance
      );
   }

   /// <summary>
   /// Updates the audio threat.  Because these are private, I learned it passes by value when using getter
   /// and I'll either have to make it public or have this method to set it.
   /// </summary>
   /// <param name="type"></param>
   /// <param name="collider"></param>
   /// <param name="distance"></param>
   public void TrackAudioThreat(Collider collider, Vector3 soundPosition, float distance) {
      this.audioThreat.SetTarget(
         AiTargetType.Audio,
         collider,
         soundPosition,
         distance
      );
   }

   /// <summary>
   /// Moves the target trigger to the underlying target position and enables it.   
   /// Basically causes the target trigger gameobject to be sent out from the entity to the current 
   /// target location.  
   /// </summary>
   /// <param name="stoppingDistance">The desired stopping distance to use for the radius.</param>
   private void MoveTargetTriggerToTarget(float stoppingDistance) {
      this.targetTrigger.radius = stoppingDistance;
      this.targetTrigger.transform.position = this.target.Position;  // place SphereCollider at target's position
      this.targetTrigger.enabled = true;
   }

   /// <summary>
   /// Stops tracking the main target by clearing the target and disables the target trigger.
   /// </summary>
   public void StopTrackingTarget() {
      this.target.Clear();
      this.targetTrigger.enabled = false;
   }

   /// <summary>
   /// Determines the next potential state according to the provided potentil threat.  Certain conditions
   /// may still need to be applied after the fact before the entity will assume that new state.  
   /// I got lucky creating this early on.  It turned out that for the future videos, they nearly all returned
   /// the same states shown here.  Using it seems to simplify the clarity/readability of the code for me, but
   /// it did come at a cost (often I had issues, such as a typo, and I couldn't rely on Gary's code to help me resolve them).
   /// </summary>
   /// <param name="potentialThreat">A potential threat or null.</param>
   /// <returns>The next potential state according to the provided threat or NONE</returns>
   public AiStateType DetermineNextPotentialThreatState(AiTarget? potentialThreat) {
      AiStateType state = AiStateType.None;

      if (potentialThreat != null) {
         switch (((AiTarget) potentialThreat).Type) {
            case AiTargetType.Visual_Player:
               state = AiStateType.Pursuit;
               break;
            case AiTargetType.Visual_Light:
               state = AiStateType.Alerted;
               break;
            case AiTargetType.Audio:
               state = AiStateType.Alerted;
               break;
            case AiTargetType.Visual_Food:
               state = AiStateType.Pursuit;
               break;
            default:
               state = AiStateType.None;
               break;
         }
      }

      return state;
   }

   /// <summary>
   /// Determines the potential threat according to the current threats that are being tracked by
   /// the threat manager.
   /// </summary>
   /// <returns>The current visual or audio threat or null if one could not be determined.</returns>
   public AiTarget? DeterminePotentialThreat() {
      AiTarget? activeThreat = null;

      // they have to be done in priority order
      if (DoesPlayerThreatExist() || DoesLightThreatExist()) {
         activeThreat = this.visualThreat;
      } else if (DoesAudioThreatExist()) {
         activeThreat = this.audioThreat;
      } else if (DoesFoodThreatExist()) {
         activeThreat =  this.visualThreat;
      }

      return activeThreat;
   }

   public AiTarget CurrentVisualThreat { get { return this.visualThreat; } }
   public AiTarget CurrentAudioThreat { get { return this.audioThreat; } }
   public AiTarget CurrentTarget { get { return this.target; } }

   /// <summary>
   /// Calculate the angle we need to turn the AI Entity body towards the target
   /// </summary>
   /// <returns>A positive angle in degrees that is needed to face the target.</returns>
   public float DetermineAngleNeededToTurnTowardsTarget() {
      float angle = CalculationUtil.FindSignedAngle(
         this.stateMachine.AiEntityBodyTransform.forward,
         this.stateMachine.NavAgent.steeringTarget - this.stateMachine.AiEntityBodyTransform.position
      );
      return angle;
   }

   // here for convenience and readability
   public bool DoesPlayerThreatExist() { return AiTargetType.Visual_Player == this.visualThreat.Type; }
   public bool DoesLightThreatExist() { return AiTargetType.Visual_Light == this.visualThreat.Type; }
   public bool DoesFoodThreatExist() { return AiTargetType.Visual_Food == this.visualThreat.Type; }
   public bool DoesVisualThreatExist() { return AiTargetType.None != this.visualThreat.Type; }
   public bool DoesAudioThreatExist() { return AiTargetType.None != this.audioThreat.Type; }
   public bool DoesTargetExist() { return AiTargetType.None != this.target.Type; }
   public bool IsTargeting(AiTargetType type) { return this.target.Type == type; }
}
