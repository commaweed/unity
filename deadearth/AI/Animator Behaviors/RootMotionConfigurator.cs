using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attach this script to animator animations to indicate to them whether or not we want 
/// the position and/or rotation to be controlled by the animation itself (1) or whether
/// we'll do the work in our code (0).  You do this by clicking the "Add Behavior" button
/// once you click on an animation.  Choose this script from the drop-down list.
/// </summary>
public class RootMotionConfigurator : AiStateMachineLink {

   // position (e.g. 1) means yes, use the animation's root position or rotation
   // negative (e.g. -1) means no, don't use the animation's root position or rotation
   // these can be set as default values from within the Animator window in its inspector tab
   [SerializeField] private int rootPosition;
   [SerializeField] private int rootRotation;

   /// <summary>
   /// Called on the first Update frame when a statemachine enters this state.  Apply the
   /// script values to the state machine.
   /// </summary>
   /// <param name="animator">not used</param>
   /// <param name="stateInfo">not used</param>
   /// <param name="layerIndex">not used</param>
   public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      /* 
      When the event is fired to this callback, all we do is register the values we set in the RootMotionConfigurator
      script that was attached to the respective animation and then we INCREMENT the global values that are tracked
      via RootMotionProperties (a reference of the AiStateMachine).  AiState children can query it
      to determine if rootPosition and/or rootRotation should be used or not
      */
      this.StateMachine.AddRootMotionRequest(rootPosition, rootRotation);
   }

   /// <summary>
   /// Called on the last update frame when a statemachine exits this state.  Remove the
   /// script values from the state machine.
   /// </summary>
   /// <param name="animator">not used</param>
   /// <param name="stateInfo">not used</param>
   /// <param name="layerIndex">not used</param>
   public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      /* 
      When the event is fired to this callback, all we do is register the values we set in the RootMotionConfigurator
      script that was attached to the respective animation and then we DECREMENT the global values that are tracked
      via RootMotionProperties (a reference of the AiStateMachine).  AiState children can query it
      to determine if rootPosition and/or rootRotation should be used or not
      */
      this.StateMachine.AddRootMotionRequest(-rootPosition, -rootRotation);
   }

}
