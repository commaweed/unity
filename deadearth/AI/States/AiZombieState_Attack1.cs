using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An Ai state that implements the attack animation behavior for zombie-like ai entities.
/// </summary>
public class AiZombieState_Attack1 : AiZombieState {

   // the speed we want the zombie to be moving at when in attack state
   [SerializeField] [Range(0, 10)] private float speed = 0.0f;

   [SerializeField] private float slerpSpeed = 5.0f;

   [SerializeField] private float stoppingDistance = 1.0f;

   // how strongly we want to zombie head to look at the player
   [SerializeField] [Range(0.0f, 1.0f)] private float lookAtWeight = 0.7f;

   // how soon we want to make the zombie head look at the player
   [SerializeField] [Range(0.0f, 90.0f)] private float lookAtAngleThreshold = 15.0f;
   
   // keep track of the zombie look weight so we can gradually ramp up/down to the lookAtWeight
   private float currentLookAtWeight = 0.0f;

   /// <summary>
   /// Returns the default state.
   /// </summary>
   /// <returns>Idle</returns>
   public override AiStateType GetDefaultStateType() {
      return AiStateType.Attack;
   }

   /// <summary>
   /// Callback that is fired when this state first becomes active.
   /// </summary>
   public override void OnEnterState() {
      base.OnEnterState();

      // Configure State Machine
      zombieStateMachine.ModifyNavAgentUpdateAttributes(true, false);
      zombieStateMachine.Seeking = 0;
      zombieStateMachine.Feeding = false;
      RandomlySetNextAttackAnimation();
      zombieStateMachine.Speed = this.speed;
      currentLookAtWeight = 0.0f;
   }

   /// <summary>
   /// Callback that is fired when this state goes inactive.
   /// </summary>
   public override void OnExitState() {
      UnsetAttackAnimation();
   }

   /// <summary>
   /// Called by the state machine each frame.
   /// </summary>
   /// <returns>Either the current state or a new state.</returns>
   public override AiStateType OnUpdate() {
      AiStateType state = GetDefaultStateType();

      AdjustSpeed();

      if (zombieStateMachine.ThreatManager.DoesPlayerThreatExist()) {
         zombieStateMachine.ThreatManager.TrackTarget(zombieStateMachine.ThreatManager.CurrentVisualThreat);

         if (zombieStateMachine.IsInMeleeRange) {
            FaceTargetGradually(this.slerpSpeed);
            RandomlySetNextAttackAnimation();
         } else {
            state = AiStateType.Pursuit;
         }
      } else {
         // PLayer has stepped outside out FOV or hidden so face in his/her direction and then
         // drop back to Alerted mode to give the AI a chance to re-aquire target
         FaceTarget();
         state = AiStateType.Alerted;
      }

      return state;
   }

   /// <summary>
   /// Sets the Animator parameter for the attackType to a random value so a random attack animation matching the value will play.
   /// </summary>
   private void RandomlySetNextAttackAnimation() {
      zombieStateMachine.AttackType = Random.Range(1, 100);
   }

   /// <summary>
   /// Turns off the attack animation.  That is, it sets it to a state where it will transition to the default 
   /// state in the "Attack Layer".
   /// </summary>
   private void UnsetAttackAnimation() {
      zombieStateMachine.AttackType = 0;
   }

   /// <summary>
   /// Reset the speed based upon the stopping distance comparison.  We want the zombie to stop moving when it's distance
   /// to target is less than the underlying stopping distance.
   /// </summary>
   private void AdjustSpeed() {
      float distanceToTarget = Vector3.Distance(
         zombieStateMachine.transform.position, 
         zombieStateMachine.ThreatManager.CurrentTarget.Position
      );
      this.speed = distanceToTarget < this.stoppingDistance ? 0 : this.speed;
   }

   /// <summary>
   /// Callback that is fired by the parent state machine whenever its "OnAnimatorIK()" is invoked.
   /// Note:  For this to work, need to make sure in the Animator, click on Attack Layer Cog, check "IK pass".
   /// </summary>
   // TODO: modularize this because it is also used by the pursuit state
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

         // gradually apply the look weight so that the zombie moves its head towards target
         this.currentLookAtWeight = Mathf.Lerp(this.currentLookAtWeight, this.lookAtWeight, Time.deltaTime); 
      } else {
         // gradually apply the zero-weight so that the zombie moves its head away and back to normal 
         this.currentLookAtWeight = Mathf.Lerp(this.currentLookAtWeight, 0.0f, Time.deltaTime);
      }

      // the weight that we wish to blend the look with
      zombieStateMachine.Animator.SetLookAtWeight(this.currentLookAtWeight);
   }

}
