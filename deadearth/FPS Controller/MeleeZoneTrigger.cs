using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// The trigger script for the Melee Zone GameObject that is attached to a GameObject that is a child of the player character.
/// The Melee Zome Game Object must have a collider associated with it for this to work and it must have its "Is Trigger"
/// set.
/// </summary>
[RequireComponent(typeof(CapsuleCollider))]
public class MeleeZoneTrigger : MonoBehaviour {

   /// <summary>
   /// Initialize
   /// </summary>
   private void Start() {
      Collider meleeZoneCollider = GetComponent<Collider>();

      // this will only happen if it is false to begin with; nothing happens if changed after
      Assert.IsTrue(meleeZoneCollider.isTrigger, "Invalid sensorTrigger; make sure to set 'Is Trigger' to true in the inspector!");
   }

   /// <summary>
   /// Callback that handles the case when the collider "other" enters the trigger.  
   /// </summary>
   /// <param name="other">The collider that triggered the collision</param>
   private void OnTriggerEnter(Collider other) {
      AiStateMachine stateMachine = GameSceneManager.Instance.GetStateMachine(other);
      if (stateMachine != null) {
         stateMachine.IsInMeleeRange = true;
      }
   }

   /// <summary>
   /// OnTriggerExit is called when the Collider "other" has stopped touching the trigger.  
   /// </summary>
   /// <param name="other">The collider that triggered the collision</param>
   private void OnTriggerExit(Collider other) {
      AiStateMachine stateMachine = GameSceneManager.Instance.GetStateMachine(other);
      if (stateMachine != null) {
         stateMachine.IsInMeleeRange = false;
      }
   }

}
