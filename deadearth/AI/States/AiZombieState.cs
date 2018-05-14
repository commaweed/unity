using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// The parent AiState class for all zombie Ai Entities.  It contains all the logic for handling the trigger callbacks
/// for when the zombie's sensor collider collides with the other colliders of interest.  GameObjects with these Colliders will have
/// their Layer set and Tag set to values that will be tested in the trigger callbacks below.
/// 
/// Because the sensor collider trigger events are primarily handled here, this is basically where the active visual or audio threats
/// are tracked.  Remember these two types of threats are always cleared in the FixedUpdate() call that happens just prior to (Update())
/// so they will be cleared the next go around.
/// 
/// Note:  It is the AiSensor script that will first receive the trigger collision event and it will delegate it to the AiStateMachine.
///        The AiStateMachine then delegates it down to this class through the parent AiState via the onTriggerEvent().
/// </summary>
public abstract class AiZombieState : AiState {

   // the highest angle between zombie facing direction and target needed before turning is required
   // (otherwise seeking might need to occur)
   [SerializeField] protected float turnOnSpotThreshold = 80.0f;

   // this is used for debugging to show a line for when the raycast is made to determine if collider is visible
   private bool debugRayCastLineInSceneView = true; 

   protected AiZombieStateMachine zombieStateMachine;

   // store a reference to our casted type so we don't have to continually cast
   // this could "SOOOO..." use java-like generics (with wild-card type '?') - I tried this and ran into when trying to store types in the cache 
   public override void SetStateMachine(AiStateMachine stateMachine) {
      base.SetStateMachine(stateMachine);
      if (stateMachine.GetType() == typeof(AiZombieStateMachine)) {
         this.zombieStateMachine = (AiZombieStateMachine) stateMachine;
      } else {
         Debug.LogWarning("Received invalid AiZombieStateMachine type: " + stateMachine.GetType());
      }
   }

   // mask to use when establishing if there is line of sight with the player
   // default layer is first in the list
   protected int playerLayerMask;

   // the index number of the ai body part layer
   protected int bodyPartLayerIndex;

   protected int visualLayerMask;

   /// <summary>
   /// Monobehavior life-cycle method that is the first method called when you startup a scene with in it or instantiate it.
   /// Used here to initialize.
   /// </summary>
   protected virtual void Awake() {
      // when we shoot our ray, we need to include default layer to determine if it is closest and if it is, target is not visible
      playerLayerMask = LayerMask.GetMask("Default", "Player", "AI Body Part");
      visualLayerMask = LayerMask.GetMask("Default", "Player", "AI Body Part", "Visual Aggravator"); 

      bodyPartLayerIndex = LayerMask.NameToLayer("AI Body Part");
   }

   // children can override this and call this (in case we ever put anything in it)
   protected virtual void Start() {}

   /// <summary>
   /// Provides the handling of trigger events that occur whenever the AiEntity Sensor Collider collides
   /// with another collider it is sensitive to.
   /// </summary>
   /// <param name="eventType">The event type that occured</param>
   /// <param name="other">The collider that triggered the event</param>
   public override void OnTriggerEvent(AiTriggerEventType eventType, Collider other) {
      base.OnTriggerEvent(eventType, other); // at the moment, the parent doesn't do anything

      /* 
       The very first time this is called, at the start of a physics update (i.e. FixedUpdate), 
       visual and audio threats are cleared.  So any threat we set here will override it anyways. 
      */

      // process most important threats first
      if (eventType != AiTriggerEventType.Exit) {
         if (other.CompareTag("Player")) {
            HandlePlayerThreat(other);
         } else if (!zombieStateMachine.ThreatManager.DoesPlayerThreatExist()) {
            if (other.CompareTag("Flashlight")) {
               HandleFlashlightThreat((BoxCollider) other);
            } else if (other.CompareTag("AI Sound Emitter")) {
               HandleAudioThreat(other);
            } else if (other.CompareTag("AI Food") &&
               !zombieStateMachine.ThreatManager.DoesLightThreatExist() &&
               !zombieStateMachine.ThreatManager.DoesAudioThreatExist() &&
               this.zombieStateMachine.IsHungery()
            ) {
               HandleHungerThreat(other);
            }
         }
      }
   }

   /// <summary>
   /// Indicates whether or not the distance to the given threat is smaller then anything we may have 
   /// previously stored 
   /// </summary>
   /// <param name="threat">The threat to test</param>
   /// <param name="distanceToThreat">The pre-computed distance to the given threat</param>
   /// <returns>true if a previous threat existed and zombie is now closer to it.</returns>
   private bool IsCloserThanLastThreat(AiTarget threat, float distanceToThreat) {
      AiThreatManager manager = zombieStateMachine.ThreatManager;
      return threat.Type == AiTargetType.None || distanceToThreat < threat.Distance;
   }

   /// <summary>
   /// Actions to take whenever a player threat is detected.
   /// </summary>
   /// <param name="playerCollider">The player collider that triggered the collision</param>
   private void HandlePlayerThreat(Collider playerCollider) {
      AiThreatManager manager = zombieStateMachine.ThreatManager;
      float distanceToThreat = Vector3.Distance(zombieStateMachine.Sensor.WorldPosition, playerCollider.transform.position);

      // if the currently stored threat is not the player or it is the player and it's closer than what was last stored
      if (!manager.DoesPlayerThreatExist() ||
         (manager.DoesPlayerThreatExist() && IsCloserThanLastThreat(manager.CurrentVisualThreat, distanceToThreat))) {
         RaycastHit hitInfo;
         if (IsColliderVisible(playerCollider, out hitInfo, playerLayerMask)) {
            // it's close and in our FOV so store as the current most dangerous threat
            this.zombieStateMachine.ThreatManager.TrackVisualThreat(
               AiTargetType.Visual_Player,
               playerCollider,
               distanceToThreat
            );
         }
      }
   }

   /// <summary>
   /// Actions to take whenever a Visual_light threat is detected.
   /// </summary>
   /// <param name="flashlightCollider">The flashlight collider that triggered the collision</param>
   private void HandleFlashlightThreat(BoxCollider flashlightCollider) {
      float distanceToThreat = Vector3.Distance(zombieStateMachine.Sensor.WorldPosition, flashlightCollider.transform.position);

      // the flashlight is offset on the z-axis so that it protrudes out from the player
      float zSize = flashlightCollider.size.z * flashlightCollider.transform.lossyScale.z; // lossyScale factors in ancector positions
      float aggragationFactor = distanceToThreat / zSize;
      if (aggragationFactor < this.zombieStateMachine.Sight && aggragationFactor <= this.zombieStateMachine.Intelligence) {
         if (IsCloserThanLastThreat(zombieStateMachine.ThreatManager.CurrentVisualThreat, distanceToThreat)) {
            this.zombieStateMachine.ThreatManager.TrackVisualThreat(
               AiTargetType.Visual_Light,
               flashlightCollider,
               distanceToThreat
            );
         }
      }
   }

   /// <summary>
   /// Actions to take whenever an Audio threat is detected.
   /// </summary>
   /// <param name="audioCollider">The Audio collider that triggered the collision</param>
   private void HandleAudioThreat(Collider audioCollider) {
      SphereCollider soundTrigger = (SphereCollider) audioCollider;
      if (soundTrigger == null)
         return;

      // Get the position of the Agent Sensor 
      Vector3 agentSensorPosition = zombieStateMachine.Sensor.WorldPosition;

      Vector3 soundPos;
      float soundRadius;
      CalculationUtil.ConvertSphereColliderToWorldSpace(soundTrigger, out soundPos, out soundRadius);

      // How far inside the sound's radius are we
      float distanceToThreat = (soundPos - agentSensorPosition).magnitude;

      // Calculate a distance factor such that it is 1.0 when at sound radius 0 when at center
      float distanceFactor = (distanceToThreat / soundRadius);

      // Bias the factor based on hearing ability of Agent.
      distanceFactor += distanceFactor * (1.0f - this.zombieStateMachine.Hearing);

      // Too far away
      if (distanceFactor > 1.0f)
         return;

      // if We can hear it and is it closer then what we previously have stored
      if (IsCloserThanLastThreat(zombieStateMachine.ThreatManager.CurrentAudioThreat, distanceToThreat)) {
         // Most dangerous Audio Threat so far
         zombieStateMachine.ThreatManager.TrackAudioThreat(
            audioCollider, 
            soundPos, 
            distanceToThreat
         );
      }
   }

   /// <summary>
   /// Actions to take whenever a Visual_food threat is detected.
   /// </summary>
   /// <param name="hungerCollider">The visual_food collider that triggered the collision</param>
   private void HandleHungerThreat(Collider hungerCollider) {
      // How far away is the threat from us
      float distanceToThreat = Vector3.Distance(zombieStateMachine.Sensor.WorldPosition, hungerCollider.transform.position);

      if (IsCloserThanLastThreat(zombieStateMachine.ThreatManager.CurrentVisualThreat, distanceToThreat)) {
         // If so then check that it is in our FOV and it is within the range of this AIs sight
         RaycastHit hitInfo;
         if (IsColliderVisible(hungerCollider, out hitInfo, visualLayerMask)) {
            // Yep this is our most appealing target so far
             zombieStateMachine.ThreatManager.TrackVisualThreat(
               AiTargetType.Visual_Food,
               hungerCollider,
               distanceToThreat
            );
         }
      }
   }

   /// <summary>
   /// Indicates whether or not the given target is visible.  This is done by shooting a raycast from the zombie's head
   /// to the given target.
   /// </summary>
   /// <param name="target">The target to test</param>
   /// <param name="hitInfoResult">A potential hit.  We set this as an out parameter but it doesn't appear to be used.</param>
   /// <param name="layerMask">We only want to include these layers in the test</param>
   /// <returns></returns>
   protected virtual bool IsColliderVisible(Collider target, out RaycastHit hitInfoResult, int layerMask = -1) {
      bool result = false;
      hitInfoResult = new RaycastHit();

      if (IsThreatInFieldOfView(target)) {
         RaycastHit hitInfo;
         result = ShootRayAtThreat(target, out hitInfo, layerMask);
         hitInfoResult = hitInfo;
      }

      return result;
   }

   /// <summary>
   /// Determines if the given threat is in the zombie's field of view.
   /// </summary>
   /// <param name="threat">The threat to test</param>
   /// <returns>true if the angle between the targt and the zombie is <= 1/2 FOV</returns>
   private bool IsThreatInFieldOfView(Collider threat) {
      float angle = Vector3.Angle(
         threat.transform.position - zombieStateMachine.AiEntityBodyTransform.position,
         zombieStateMachine.AiEntityBodyTransform.forward  
      );

      // if angle between zombie and target is half of fov, it is outside of fov
      bool isInFov = angle <= ((AiZombieStateMachine) StateMachine).FieldOfView * 0.5f;
     
      return isInFov;
   }

   /// <summary>
   /// Shoot a raycast at the target to see if we hit it.
   /// </summary>
   /// <param name="threat">The threat to shoot the raycast at.</param>
   /// <param name="hitInfo">Information about what we hit (or missed)</param>
   /// <param name="layerMask">The layer mask to use.</param>
   /// <returns>true if the target was hit and is in our line of sight.</returns>
   private bool ShootRayAtThreat(Collider threat, out RaycastHit hitInfo, int layerMask = -1) {
      hitInfo = new RaycastHit();

      RaycastHit[] hits = Physics.RaycastAll(
        // shoot from zombie position
        zombieStateMachine.Sensor.Trigger.transform.position,
        // unit vector in direction of threat
        (threat.transform.position - zombieStateMachine.Sensor.Trigger.transform.position).normalized,
        // how far we want the ray to go, limited at most by senor radius and further reduced by poor zombie sight
        zombieStateMachine.Sensor.WorldRadius * zombieStateMachine.Sight,
        // limited to the GameObject layers we want to include in the hit
        layerMask
      );

      float closestColliderDistance = float.MaxValue;
      Collider closestCollider = null;
      for (int i=0; i < hits.Length; i++) {
         RaycastHit hit = hits[i];

         /*
         (debug only)
         the following will draw a RED line in the Scene view from the zombie position to the threat position; 
         this allows you to see if the ray is working or not 
         TODO: apparently there is a IsDebug or something like that (versus the boolean I created)
         */
         if (debugRayCastLineInSceneView) {
            Debug.DrawLine(zombieStateMachine.Sensor.Trigger.transform.position, hit.point, Color.red);
         }

         if (hit.distance < closestColliderDistance) {
            // we don't want to hit ourselves 
            int hitLayer = hit.transform.gameObject.layer;
            if (hitLayer != bodyPartLayerIndex || (hitLayer == bodyPartLayerIndex && !DidHitSelf(hit))) {
               closestColliderDistance = hit.distance;
               closestCollider = hit.collider;
               hitInfo = hit;    
            } 
         }
      }
      bool isLos = IsTargetInLineOfSight(closestCollider, threat);

      return isLos;
   }

   /// <summary>
   /// Indicates whether or not the given hit is against the zombie itself, such as a body part.
   /// </summary>
   /// <param name="hit">The actual item that was hit by the raycast</param>
   /// <returns></returns>
   private bool DidHitSelf(RaycastHit hit) {
      return StateMachine == GameSceneManager.Instance.GetStateMachine(hit.collider); //hit.rigidbody.GetInstanceID ()
   }

   /// <summary>
   /// Indicatges whether or not the given target is within site.
   /// </summary>
   /// <param name="closestHit">The item that was hit.</param>
   /// <param name="target">The desired target</param>
   /// <returns>true if the desired target matches the item that was hit by the Raycast</returns>
   private bool IsTargetInLineOfSight(Collider closestHit, Collider target) {
      return closestHit && closestHit.gameObject == target.gameObject;
   }

   /// <summary>
   /// Face the target if not using root rotation. 
   /// </summary>
   protected void FaceTarget() {
      FaceTargetGradually(-1f); 
   }

   /// <summary>
   /// Face the target if not using root rotation.  If slerpSpeed is not -1, it will face the target gradually.
   /// </summary>
   protected void FaceTargetGradually(float slerpSpeed) {
      if (!zombieStateMachine.RootMotionProperties.ShouldUseRootRotation) {
         // get position of the player (but only interested in calculating it in 2d, so set y to zombie position)
         Vector3 targetPosition = zombieStateMachine.ThreatManager.CurrentTarget.Position;
         targetPosition.y = zombieStateMachine.AiEntityBodyTransform.position.y;

         // rotate zombie to keep it facing the player
         // in other words, create a vector from zombie's position to the target's position (i.e. vector subtraction)
         Quaternion newRotation = Quaternion.LookRotation(targetPosition - zombieStateMachine.AiEntityBodyTransform.position);

         if (slerpSpeed == -1f) {
            zombieStateMachine.AiEntityBodyTransform.rotation = newRotation;
         } else {
            // gradually rotate over time
            zombieStateMachine.AiEntityBodyTransform.rotation = Quaternion.Slerp(
               zombieStateMachine.AiEntityBodyTransform.rotation,
               newRotation,
               Time.deltaTime * slerpSpeed
            );
         }
      }
   }

   /// <summary>
   /// Slowly update the zombie rotation to match the navagents desired rotation.
   /// </summary>
   /// <param name="slerpSpeed"></param>
   protected void FaceTargetUsingNavAgent(float slerpSpeed) {
      // slowly update the zombie rotation to match the navagents desired rotation, but only if we are not
      // pursuing the player and are really close to him

      // Generate a new Quaternion representing the rotation we should have
      // this is like the desired velocity of the navagent; it represents the new direction the zombie wants to face
      Quaternion newRotation = Quaternion.LookRotation(zombieStateMachine.NavAgent.desiredVelocity);

      // use slerp to get smooth rotatation to that new rotation over time
      zombieStateMachine.transform.rotation = Quaternion.Slerp(
         zombieStateMachine.AiEntityBodyTransform.rotation,
         newRotation,
         Time.deltaTime * slerpSpeed
      );
   }

}
