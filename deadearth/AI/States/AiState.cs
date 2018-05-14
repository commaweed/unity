using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The parent class for all entity state scripts.  Each child can override callbacks so they can update themselves.
/// Serves as the base class for all the individual states whether for AI (i.e. zombies), npc's, or whatever.
/// NOTE:  The chain of trigger callbacks events are as follows: 
///    1.  Each AI Entity GameObject contains a Sensor GameObject that has a Sphere Collider with "Is Trigger" checked
///    2.  The Sensor Object will also have an AiSensor script Component 
///    3.  The AiSensor Script will contain the callbacks related to triggering collisions (enter, stay, exit)
///    4.  The AiSensor callbacks will delegate handling to the AiStateMachine
///    5.  The AiStateMachine will in turn delegate handling to all the associated AiState(s) scripts that are Components of an AiEntity 
///        (E.g. AiZombieState_Patrol1, etc.)
///    6.  However, each of those AiState script Components are involved in a ancestral hierarchy with this class being the top-most parent
///    7.  For zombies, each state's parent is AiZombieState and that class currently contains the actual handling for all the trigger
///        events that can occur for any child state.  THUS LOOK IN AiZombieState to debug collisions of the Sensor collider with 
///        other colliders.  
/// </summary>
/// TODO: see if we can use generics with AiState<K extends AiStateMachine>
public abstract class AiState : MonoBehaviour {

   
   private AiStateMachine stateMachine;

   /// <summary>
   /// Returns a reference to the parent state machine.
   /// </summary>
   public AiStateMachine StateMachine {
      get {
         return this.stateMachine;
      }
   }

   /// <summary>
   /// Set a reference to the parent state machine.  Gary expects all children to override this so they can cast to eliminate the 
   /// need to continually cast when we need child-state-machine specific behavior.  Generics would eliminate the need to override
   /// it and use a double reference.  However, I couldn't figure out how to get it to work because apparently c# didn't feel a
   /// wild-card generic type was needed or safe (e.g. ?) and that prevented me from configuring the state cache with the unknown 
   /// types.
   /// </summary>
   /// <param name="stateMachine">The parent.</param>
   public virtual void SetStateMachine(AiStateMachine stateMachine) {
      if (stateMachine == null) {
         throw new System.ArgumentNullException("Invalid StateMachine; it appears this state did not get the reference added to it!");
      }
      this.stateMachine = stateMachine;
   }

   /// <summary>
   /// Default callback that can handle state initialization.  The parent state machine will call it on the NEW state 
   /// whenever a state change occurs.  It will give the child state a chance to initialize the state just before it
   /// goes into the update state that happens every frame.
   /// </summary>
   public virtual void OnEnterState() { }

   /// <summary>
   /// Default callback that can handle state cleanup.  The parent state machine will call it on the OLD state 
   /// whenever a state change occurs.  It will give the child state a chance to cleanup the state.
   /// </summary>
   public virtual void OnExitState() { }

   /// <summary>
   /// Default callback that can handle the "OnAnimatorMove()" monobehavior callback that was setup in the parent state
   /// machine; we use it mostly to override root motion of the Nav Mesh Agent in favor of the Animation root motion
   /// or our own custom motion (position or rotation).
   /// </summary>
   public virtual void OnAnimatorUpdated() {
      // ask the state machine if we need to override the nav agent's velocity and handle it ourselves
      if (this.stateMachine.RootMotionProperties.ShouldUseRootPosition) {
         // override velocity of navAgent; mimic the velocity that is contained in the animation
         // velocity is specified in m/sec and we need it in this fraction of a second so divide by time
         this.stateMachine.NavAgent.velocity =
            this.stateMachine.Animator.deltaPosition // tells us how much it should have moved 
            / Time.deltaTime; // divide here so we get the velocity for this fraction of a second (this update)
      }

      // ask the state machine if we need to also calculate the root rotation; if so, get the value from the animator
      if (this.stateMachine.RootMotionProperties.ShouldUseRootRotation) {
         this.stateMachine.transform.rotation = this.stateMachine.Animator.rootRotation;
      }
   }

   /// <summary>
   /// Default callback that can handle the "OnAnimatorMove()" monobehavior callback that was setup in the parent state
   /// machine using unity's IK system (IK Pass has to be turned on in the Animator on the appropriate layer (e.g. Base Layer).
   /// It can be used to issue commands to the various body parts of the avatar, such as to make the zombie head look at
   /// its target while it is pursuing it.
   /// </summary>
   public virtual void OnAnimatorIkSystemUpdated() { }

   /// <summary>
   /// Default callback that can handle AI Trigger events.
   /// </summary>
   /// <param name="eventType">The type of event that was triggered</param>
   /// <param name="other">The collider that triggered the event.</param>
   public virtual void OnTriggerEvent(AiTriggerEventType eventType, Collider other) { }

   /// <summary>
   /// Default callback for when the destination was reached.
   /// </summary>
   /// <param name="wasReached">Not sure why it needs to pass in the boolean because the method should
   /// only fire when the destination is actually reached.</param>
   public virtual void OnDestinationReached(bool wasReached) { } 

   /// <summary>
   /// Returns the default state type.
   /// </summary>
   /// <returns>The default state type.</returns>
   public abstract AiStateType GetDefaultStateType();

   /// <summary>
   /// Default callback that can handle updating the state.  It gives the possibility of potentially 
   /// changing states within the child.  Return a different state than the underlying state to change
   /// to the new state.
   /// </summary>
   public abstract AiStateType OnUpdate();

}
