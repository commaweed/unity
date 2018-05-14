using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sensor GameObjects and their associated Collider only care about other collider objects that
/// have been marked with layer "AI Trigger".  This script contains callbacks for when those other
/// objects interact with the sensor collider.  Each callback will notify the parent state machine
/// that a trigger event occurred so that the state machine can take action.
/// Attach this script to the Sensor GameObject that is a child of an "AI Entity" GameObject.  AI Entity
/// has to have an associated AiStateMachine attached to it.
/// </summary>
public class AiSensor : MonoBehaviour {

   // maintain a reference to the parent state machine that is associated with the sensor object
   private AiStateMachine parentStateMachine;
   public AiStateMachine ParentStateMachine { set { this.parentStateMachine = value; } }  

   /// <summary>
   /// Callback that handles the case when the collider "other" enters the trigger.  It will delegate the handling
   /// of the callback to the parent state machine.
   /// </summary>
   /// <param name="other">The collider that triggered the collision</param>
   private void OnTriggerEnter(Collider other) {
      if (this.parentStateMachine != null) {
         this.parentStateMachine.OnTriggerEvent(AiTriggerEventType.Enter, other);
      }
   }

   /// <summary>
   /// OnTriggerStay is called once per physics update for every Collider "other" is touching the trigger.  It will 
   /// delegate the handling of the callback to the parent state machine.
   /// </summary>
   /// <param name="other">The collider that triggered the collision</param>
   private void OnTriggerStay(Collider other) {
      if (this.parentStateMachine != null) {
         this.parentStateMachine.OnTriggerEvent(AiTriggerEventType.Stay, other);
      }
   }

   /// <summary>
   /// OnTriggerExit is called when the Collider "other" has stopped touching the trigger.  It will 
   /// delegate the handling of the callback to the parent state machine.
   /// </summary>
   /// <param name="other">The collider that triggered the collision</param>
   private void OnTriggerExit(Collider other) {
      if (this.parentStateMachine != null) {
         this.parentStateMachine.OnTriggerEvent(AiTriggerEventType.Exit, other);
      }
   }
}
