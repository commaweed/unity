using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// An Ai state that implements the pursuit animation behavior for zombie-like ai entities.
/// Basically whenever the zombie is alerted to a threat, it can pursue it in some cases.  
/// When it does, it will enter this state.  Because the target it is pursing might be moving,
/// we'll need to repath and this can be expensive.  So we'll have to create track of timers
/// and use different values as the zombie gets closer to the target.  That is, the farther away
/// the target is, the less we want to repath and the closer it gets, the more we'll want to 
/// repath, but we don't want to repath more than say 20 times per second.  Definitely do not
/// repath every frame! 
/// </summary>
public class AiZombieState_Pursuit1 : AiZombieState {

   // how fast we want the zombie to pursue the target (will tell animator which animation to use)
   [SerializeField] [Range(0, 10)] private float speed = 2.0f;

   // we will be rotating the zombie on the spot, similar to how we do in the patrol state
   // the speed at which we rotate the more current orientation onto our ideal orientation where we are facing our steering target
   [SerializeField] private float slerpSpeed = 5.0f;

   // at most, we never want to repath more than 20 times / second  (should be enough accuracy)
   // we'll have a timer and everytime we recalculate path, we'll reset the timer
   [SerializeField] private float repathVisualMinDuration = 0.05f;

   // we'll never want to calculate repath less than once every 5 seconds (may not get to this)
   // this is based upon being far away from the target
   [SerializeField] private float repathVisualMaxDuration = 5.0f;

   // another path will not be calculated until the timer is incremented past distance to target * this
   // but it will never be calculated more than a frequency of repathVisualMinDuration
   // used to scale the distance between the target and the zombie
   // essentially sets the number of seconds until the next repath should be done
   [SerializeField] private float repathDistanceMultiplier = 0.035f;

   // audio targets most often do not move, hence they have a different min/max repath 
   [SerializeField] private float repathAudioMinDuration = 0.25f;
   [SerializeField] private float repathAudioMaxDuration = 5.0f;

   // we only want to be in a pursue state for so long
   [SerializeField] private float maxDuration = 40.0f;

   // this timer helps us track how long the zombie has been in the pursue state, upto maxDuration
   private float timer = 0.0f;

   // this timer helps us track when to do repath calculations 
   private float repathTimer = 0.0f;

   // how strongly we want to zombie head to look at the player
   [SerializeField] [Range(0.0f, 1.0f)] private float lookAtWeight = 0.7f;

   // how soon we want to make the zombie head look at the player
   [SerializeField] [Range(0.0f, 90.0f)] private float lookAtAngleThreshold = 15.0f;

   // keep track of the zombie look weight so we can gradually ramp up/down to the lookAtWeight
   private float currentLookAtWeight = 0.0f;

   /// <summary>
   /// Callback that is fired when this state first becomes active.
   /// </summary>
   public override void OnEnterState() {
      base.OnEnterState();

      zombieStateMachine.ModifyNavAgentUpdateAttributes(true, false);
      zombieStateMachine.Seeking = 0;
      zombieStateMachine.Feeding = false;
      zombieStateMachine.AttackType = 0;

      ResetPursuitTimer();
      ResetRepathTimer();

      // Set path and make sure the navAgent is on (for a short period of time there will be no path because its async and queues it)
      zombieStateMachine.NavAgent.SetDestination(zombieStateMachine.ThreatManager.CurrentTarget.Position);
      zombieStateMachine.NavAgent.isStopped = false;
   }

   /// <summary>
   /// Resets both the pursuit timer .
   /// </summary>
   private void ResetPursuitTimer() {
      this.timer = 0.0f;
   }

   /// <summary>
   /// Resets both the repath timer.
   /// </summary>
   private void ResetRepathTimer() {
      this.repathTimer = 0.0f;
   }

   /// <summary>
   /// Returns the default state.
   /// </summary>
   /// <returns>Pursuit</returns>
   public override AiStateType GetDefaultStateType() {
      return AiStateType.Pursuit;
   }

   /// <summary>
   /// Called by the state machine each frame.
   /// </summary>
   /// <returns>Either the current state or a new state.</returns>
   public override AiStateType OnUpdate() {
      UpdateTimers();

      if (HasReachedMaxTime()) {
         return AiStateType.Patrol;
      }

      // TODO: change method so it doesn't have so many returns (returning at top is ok) - maybe use if/else

      // IF we are chasing the player and have entered the melee trigger then attack
      if (zombieStateMachine.ThreatManager.IsTargeting(AiTargetType.Visual_Player) && zombieStateMachine.IsInMeleeRange) {
         return AiStateType.Attack;
      }

      // Otherwise this is navigation to areas of interest so use the standard target threshold
      if (zombieStateMachine.IsTargetReached) {
         switch (zombieStateMachine.ThreatManager.CurrentTarget.Type) {
            // If we have reached the source
            // example, flashlight was shown behind, and player ran away, zombie arrived there, so it goes into alerted
            case AiTargetType.Audio:
            case AiTargetType.Visual_Light:
               zombieStateMachine.ThreatManager.StopTrackingTarget();  
               return AiStateType.Alerted;      // Become alert and scan for targets
            case AiTargetType.Visual_Food:
               return AiStateType.Feeding;
         }
      }

      // If for any reason the nav agent has lost its path then call then drop into alerted state
      // so it will try to re-aquire the target or eventually giveup and resume patrolling
      if (zombieStateMachine.HasLostNavMeshPath()) {
         return AiStateType.Alerted;
      }

      if (zombieStateMachine.NavAgent.pathPending) {
         zombieStateMachine.Speed = 0;
      } else {
         zombieStateMachine.Speed = this.speed;

         // zombie is very close, so make sure it is still facing target; or it reached it and we need to change state
         if (HandleZombieIsCloseOrAtTarget()) {
            return AiStateType.Alerted;
         }
      }

      // player is probably still current target and so we need to repath continually to ensure we are still pursing it
      if (HandlePlayerIsVisualThreat()) {
         return AiStateType.Pursuit;
      }

      // If our target is the last sighting of a player then remain in pursuit as nothing else can override
      if (zombieStateMachine.ThreatManager.IsTargeting(AiTargetType.Visual_Player)) {
         return AiStateType.Pursuit;
      }

      // If here, current player is not the threat, but some other visual threat exists (e.g. light, sound)
      AiStateType type = HandleAlertedStateThreats();
      if (type != AiStateType.None) {
         return type;
      }

      return GetDefaultStateType();
   }

   /// <summary>
   /// Handle the situation where the zombie gets very close to the target or reaches it.  If reached 
   /// and not found, return true so that the zombie can go back to an alerted state.  If the target
   /// wasn't exactly reached but the zombie is very close to it, handle rotation of the zombie so that
   /// it can stay facing the target.
   /// </summary>
   /// <returns>true if the target was reached and not found</returns>
   private bool HandleZombieIsCloseOrAtTarget() {
      bool wasTargetReached = false;

      // if we are very close to the player and we want to keep aligned to the player, but we are not in melee range
      // then keep facing the player
      if (
         zombieStateMachine.ThreatManager.IsTargeting(AiTargetType.Visual_Player) &&
         zombieStateMachine.ThreatManager.DoesPlayerThreatExist() &&
         zombieStateMachine.IsTargetReached
      ) {
         FaceTarget();
      } else if (
         !zombieStateMachine.RootMotionProperties.ShouldUseRootRotation &&
         !zombieStateMachine.IsTargetReached
      ) {
         //  we are not pursuing the player and are really close either
         FaceTargetUsingNavAgent(this.slerpSpeed);
      } else if (zombieStateMachine.IsTargetReached) {
         // if get here, we reached our target, but it wasn't the player; so can back to an alerted mode
         wasTargetReached = true;
      }

      return wasTargetReached;
   }

   /// <summary>
   /// Handles the case where the player is not the target but is still the visual threat and so
   /// we need to continually repath to its location because it could be moving.
   /// </summary>
   /// <returns>true if the player is still the target and should continue being pursued</returns>
   private bool HandlePlayerIsVisualThreat() {
      bool shouldContinuePursing = false;

      if (zombieStateMachine.ThreatManager.DoesPlayerThreatExist()) {
         RepathToThreatIfNecessary(zombieStateMachine.ThreatManager.CurrentVisualThreat);

         // continue to track the player that is the visual threat
         zombieStateMachine.ThreatManager.TrackTarget(zombieStateMachine.ThreatManager.CurrentVisualThreat);
         shouldContinuePursing = true;
      }

      return shouldContinuePursing;
   }

   /// <summary>
   /// Indicates whether or not to perform the expensive repath to the target.  We want to repath less
   /// frequently as we get closer to the target.  Thus, need to calculate distance to the target and
   /// clamp it to our min/max times.
   /// </summary>
   /// <returns>true if we should repath to the target's location</returns>
   private bool ShouldRepath(AiTarget threat) {
      return Mathf.Clamp(
         threat.Distance * this.repathDistanceMultiplier,
         this.repathVisualMinDuration, this.repathVisualMaxDuration
      ) < this.repathTimer;
   }

   /// <summary>
   /// Performs the repath to the given threat if necessary.
   /// </summary>
   /// <param name="threat">The threat to repath to.</param>
   private void RepathToThreatIfNecessary(AiTarget threat) {
      // The position must be different different - maybe same threat but it has moved so repath periodically
      // and our repath timer must be ready to repath so we can save on cpu cycles
      if (zombieStateMachine.ThreatManager.CurrentTarget.Position != threat.Position && ShouldRepath(threat)) {
         zombieStateMachine.NavAgent.SetDestination(threat.Position); 
         ResetRepathTimer();
      }
   }

   /// <summary>
   /// Handles threats that tend to produce the Alerted state, such as an Audio or Visual_light threat.
   /// </summary>
   /// <returns>A new state or NONE if nothing needed to be handled</returns>
   private AiStateType HandleAlertedStateThreats() {
      AiStateType state = AiStateType.None;

      if (zombieStateMachine.ThreatManager.DoesLightThreatExist()) {
         state = HandleVisualLightThreats();
      } else if (zombieStateMachine.ThreatManager.DoesAudioThreatExist()) {
         state = HandleAudioThreat();
      }

      return state;
   }

   /// <summary>
   /// Handles the case where the current visual threat is a Visual_light.
   /// </summary>
   /// <returns>A new state or NONE if nothing needed to be handled</returns>
   private AiStateType HandleVisualLightThreats() {
      AiStateType state = AiStateType.None;

      AiThreatManager manager = zombieStateMachine.ThreatManager;
      AiTarget visualThreat = zombieStateMachine.ThreatManager.CurrentVisualThreat;

      // and we currently have a lower priority target then drop into alerted
      // mode and try to find source of light
      if (manager.IsTargeting(AiTargetType.Audio) || manager.IsTargeting(AiTargetType.Visual_Food)) {
         state = AiStateType.Alerted;
      } else if (manager.IsTargeting(AiTargetType.Visual_Light)) {
         // Get unique ID of the collider of our target
         int currentID = zombieStateMachine.ThreatManager.CurrentTarget.GetColliderID();

         // If this is the same light
         if (currentID == visualThreat.GetColliderID()) {
            RepathToThreatIfNecessary(visualThreat);
            state = AiStateType.Pursuit;
         } else {
            state = AiStateType.Alerted;
         }
      }

      manager.TrackTarget(visualThreat);

      return state;
   }

   /// <summary>
   /// Handles the case where the current Audio AiTarget contains a current Audio Threat.
   /// </summary>
   /// <returns>A new state or NONE if nothing needed to be handled</returns>
   private AiStateType HandleAudioThreat() {
      AiStateType state = AiStateType.None;

      AiThreatManager manager = zombieStateMachine.ThreatManager;
      AiTarget audioThreat = zombieStateMachine.ThreatManager.CurrentAudioThreat;

      if (manager.IsTargeting(AiTargetType.Visual_Food)) {
         state = AiStateType.Alerted;
      } else if (manager.IsTargeting(AiTargetType.Audio)) {
         // Get unique ID of the collider of our target
         int currentID = zombieStateMachine.ThreatManager.CurrentTarget.GetColliderID();

         // If this is the same light
         if (currentID == zombieStateMachine.ThreatManager.CurrentAudioThreat.GetColliderID()) {
            RepathToThreatIfNecessary(audioThreat);
            state = AiStateType.Pursuit;
         } else {
            state = AiStateType.Alerted;
         }
      }

      manager.TrackTarget(audioThreat);

      return state;
   }


   /// <summary>
   /// Increments both the pursuit timer and the repath timer.
   /// </summary>
   private void UpdateTimers() {
      repathTimer += Time.deltaTime;
      timer += Time.deltaTime;
   }

   /// <summary>
   /// Updates the timer and indicates whether or not the max time has been reached.
   /// </summary>
   /// <returns></returns>
   private bool HasReachedMaxTime() {
      return timer > maxDuration;
   }

   /// <summary>
   /// Callback that is fired by the parent state machine whenever its "OnAnimatorIK()" is invoked.
   /// Note:  For this to work, need to make sure in the Animator, click on Base Layer Cog, check "IK pass".
   /// Note:  this didn't work so well in the Patrol state because the head would continually look at the target through walls, etc.
   /// </summary>
   public override void OnAnimatorIkSystemUpdated() {
      float anglePlayerToZombie = Vector3.Angle(
         zombieStateMachine.AiEntityBodyTransform.forward,
         zombieStateMachine.ThreatManager.CurrentTarget.Position - zombieStateMachine.AiEntityBodyTransform.position
      );

      if (anglePlayerToZombie < lookAtAngleThreshold) {
         // want head to look at the target position
         zombieStateMachine.Animator.SetLookAtPosition(
            zombieStateMachine.ThreatManager.CurrentTarget.Position + Vector3.up
         );

         this.currentLookAtWeight = Mathf.Lerp(this.currentLookAtWeight, this.lookAtWeight, Time.deltaTime);
      } else {
         this.currentLookAtWeight = Mathf.Lerp(this.currentLookAtWeight, 0.0f, Time.deltaTime);
      }

      // the weight that we wish to blend the look with
      zombieStateMachine.Animator.SetLookAtWeight(this.currentLookAtWeight);
   }

}
