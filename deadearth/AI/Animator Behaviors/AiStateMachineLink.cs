using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parent class of all Animator statemachine behavior scripts.  Once you open an AnimatorController (e.g. "Omni Zombie 1"), 
/// in the resulting Animator Window, when clicking on a pre-configured animation, you will see a button in the inspector
/// called "Add Behavior".  When you click on that button, it will display a popup menu with children of this class as choices.
/// An example is RootMotionConfigurator.  We use that behavior to tell our animations whether they need to use their root
/// position or rotation or not.
/// (They need to be attached to the animation or blend in the Animator as behaviors)
/// </summary>
public abstract class AiStateMachineLink : StateMachineBehaviour {

   // all we do in the base class is store a reference to the parent state machine and we do this so we can pass the values 
   // in the animator state machine behavior script that change.  All property changes are eventually delegated to the
   // AiStateMachine's reference to RootMotionProperties.
   private AiStateMachine stateMachine;
   public AiStateMachine StateMachine {
      get {
         if (this.stateMachine == null) {
            throw new System.ArgumentNullException(
               "Invalid attempt to get reference to AiStateMachine from AiStateMachineLink; did you configure it correctly?"
            );
         }
         return this.stateMachine;
      }
      set { this.stateMachine = value;  }
   }

}
