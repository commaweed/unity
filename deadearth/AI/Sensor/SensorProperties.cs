using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Represents properties related to the Sensor GameObject that is attached to an AI Entity GameObject.  These
/// were extracted from the original AiStateMachine to deconvolute it.  Thus any property or method related to
/// the Sensor GameObject can be found here.
/// </summary>
public class SensorProperties {

   private SphereCollider sensorTrigger;
   private AiStateMachine stateMachine;
   private Vector3 worldPosition;
   private float worldRadius;

   /// <summary>
   /// Initialize all properties with the given sensor trigger.  Note that the collider should have the
   /// "Is Trigger" enabled.
   /// </summary>
   /// <param name="sensorTrigger"></param>
   public SensorProperties(SphereCollider sensorTrigger, AiStateMachine stateMachine) {
      /*
        Assertions should be sufficient to notify us if something has not been configured correctly because many of the 
        required components are actually manually added by us rather than instantiated by code and we'll get a meaningful
        message right when we play the scene.
        Thus, this is used in favor of all the "null" checks that were found in the lesson videos and that gives a little
        more clarity to the code.
      */

      Assert.IsNotNull(sensorTrigger, "Missing sensorTrigger; did you forget to drag it onto the AI Entity GameObject in the inspector!");
      // this will only happen if it is false to begin with; nothing happens if changed after
      Assert.IsTrue(sensorTrigger.isTrigger, "Invalid sensorTrigger; make sure to set 'Is Trigger' to true in the inspector!");

      this.sensorTrigger = sensorTrigger;
      this.stateMachine = stateMachine;

      // pass the parent state machine to the AiSensor script so that it can delegate trigger callbacks to it
      AiSensor script = this.sensorTrigger.GetComponent<AiSensor>();
      Assert.IsNotNull(script, "Missing AiSensor script on the Sensor GameObject!");
      script.ParentStateMachine = stateMachine;

      // Since sensor is a child with a local position and rotation, we need to ensure its position and radius
      // scales correctly when ancestor values change (i.e. compute global or world position and radius)
      // alternate way is to use the new method that computes both values
      this.worldPosition = CalculationUtil.ComputeWorldPosition(this.sensorTrigger);
      this.worldRadius = CalculationUtil.ComputeWorldRadius(this.sensorTrigger);
   }

   // expose getters
   public SphereCollider Trigger { get { return this.sensorTrigger; } } 
   public Vector3 WorldPosition { get { return this.worldPosition; } }
   public float WorldRadius { get { return this.worldRadius; } }

}
