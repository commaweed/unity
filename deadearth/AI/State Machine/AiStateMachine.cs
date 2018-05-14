using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

// ============================================================================================
// The following info represents definitions of things that need to be configured in the scene 
// ============================================================================================

// Create the following Layers:
// "AI Entity" Layer
//    only one object type should be assigned: AI Entity
//    represents the zombie's physical body
// "AI Entity Trigger" Layer is sensitive to AI Entity Objects entering it
//    example object that uses layer:  Target Trigger, Flashlight
//    represents any colliders that should be sensitive the zombie's body entering it
// "AI Trigger" Layer is sensitive to any object that is considered hostile to AI Entity (i.e. aggrevators)
//    only object that uses this layer:  Sensor 
// "Player" Layer
//    the player also acts as an aggravator to the "AI Entity" GameObject
// "Visual Aggrevator" Layer
//    is an aggravator to the "AI Entity" GameObject
// "Audio Aggrevator" Layer
//    is an aggravator to the "AI Entity" GameObject
// "AI Body Part"
//    represents a zombie body part, for example, that are RayCaster might hit
// "Default" 
//    buildings will be assigned the default layer

// NOTE: for Layers, be sure to edit the sensitivies of layers in the physics window
//    choose "Edit" -> "Project Settings" -> "Physics"
//    navigate to the Layer Matrix near the bottom and uncheck all the boxes for the new layers
//    then add the type of sensitivies listed below:
//       "AI Entity" is sensitive to "AI Entity Trigger"
//       "Audio Aggravator" is sensitive to "AI Trigger"
//       "Visual Aggravator" is sensitive to "AI Trigger"
//       "Player" is sensitive to "AI Trigger" <-- more to come

// Add custom Tags (right now these mostly represent the aggravators for the zombie) - player tag is top priority
//    tag 0 = AI Sound Emitter (this can mean many types of sounds - may want to break this out to many tags)
//    tag 1 = AI Visual (objects that are visual threats to the zombie)
//    tag 2 = AI Food (give to dead bodies on the ground)
//    tag 3 = Flashlight
//    tag 4 = Melee Zone (associated with the Melee Zone Trigger that has a capsule collider; this gameobject is a child of AI Entity)
//    priority:  player > visual > audio

// Game Objects that comprise something like a zombie:
// "AI Entity" GameObject is the thing that moves and is managed by root motion in the navagent (e.g. a zombie)
//    it must have the following components:  Animator, NavMeshAgent, RigidBody, Collider, and an AiStateMachine
//    "Is Kinematic" must be checked on RigidBody

// "Sensor" GameObject which listens for sounds (20 meter radius) 
//    it is a child of "AI Entity"
//    it must have a collider on it and have "Is Trigger" checked
// "Target Trigger" GameObject will not move with the AI Entity (not a child of "AI Entity") 
//    it is a child of "Omni Zombie Jill"
//    it must have a collider on it and have "Is Trigger" checked
//    it will be sent out to a new target location each time one is set and reset when target is cleared
// "Omni Zombie Jill" GameObject is a container that holds both the "AI Entity" and the "Target Trigger" for the "AI Entity"
//    these are generic/normal types of zombies (not specialized)

// Terms:
// aggravator - any object the zombie considers to be of interest

// animation window terms:
//    cinematic layer needs to have full weight of 1 (want no blending) - click cog


// all the various states that are state machine supports
public enum AiStateType { None, Idle, Patrol, Alerted, Attack, Feeding, Pursuit, Dead }

// the sensor collider will fire these types of events when "Ai Trigger Layer" colliders trigger it
public enum AiTriggerEventType { Enter, Stay, Exit }

/// <summary>
/// This is the base class for all the character AI state machines, such as zombies, NPCs, etc.  At the moment, AI 
/// simple means any Entity that uses the Navigation Mesh (NavMesh) for its primary motion.  But we also use a hybrid
/// motion system with the Animator Controller and the state machine helps us change from one motion type to the other.  
/// Furthermore, the state machine can control various properties about the state of a zombie, for example, like how 
/// hungry it is, how fast it moves, whether the zombie is attacking or patroling, etc.
/// For the most part, this state machine is configured with callbacks that can receive various events and it will 
/// simply delegate the handling of those events to each of the various AiState scripts that were dragged onto the
/// AI Entity (i.e. it somewhat acts as a facade to those child states).
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]  
[RequireComponent(typeof(Animator))]             // This machine patterns it behavior after the controller we built:  "Omni Zombie 1"
[RequireComponent(typeof(Rigidbody))]            // it needs to have kinematic checked
[RequireComponent(typeof(Collider))]             // the capsule collider (i.e. entityCollider)
[RequireComponent(typeof(AiWaypointManager))]    // the script that handles all things related to waypoints for this AI Entity
public abstract class AiStateMachine : MonoBehaviour {

   // TODO: AiStateType and AiState are coupled, but AiState can be null; refactor to ensure they can remain together

   // the current state type
   [SerializeField] protected AiStateType currentStateType = AiStateType.Idle;

   // just here for display to show what is currently being targeted (it will show the target type and distance from it)
   [SerializeField] public string trackingTarget;
   [SerializeField] public string trackingVisualThreat;
   [SerializeField] public string trackingAudioThreat;
   private void HandleDebugDisplay() { // updated with each frame
      this.trackingTarget = this.threatManager.CurrentTarget.ToString();
      this.trackingVisualThreat = this.threatManager.CurrentVisualThreat.ToString();
      this.trackingAudioThreat = this.threatManager.CurrentAudioThreat.ToString();
   }

   // the current state (when not null, it knows the current state type)
   protected AiState currentState;

   /*
   This collider moves out from the AI Enity to the current target when the entity begins to track that target.  
   It is triggered when the entity's collider reaches it (collides with it).  This target trigger is never something
   that is processed by other external objects.
   */
   [SerializeField] private SphereCollider targetTrigger;

   /*
   This collider is a component of the Sensor GameObject.  The Sensor is a child of an AI Entity GameObject.
   It represents a sensor that surronds the entity and it can be triggered when things the entity is interested 
   in collide with it.  Thus, it needs to be registered with the GameSceneManager so that fast look ups can occur.
   */
   [SerializeField] private SphereCollider sensorTrigger;

   // ==== navigation-related ====
   // we are going to ignore what the NavMeshAgent tells us to do velocity-wise, so we need to manage stopping distance
   [SerializeField] [Range(0, 15)] private float stoppingDistance = 1.0f;
   public float StoppingDistance { get { return this.stoppingDistance;  } }

   // represents all of the states that have been added to the entity 
   // (AiState script children that are manually dragged to the AI Entity GameObject)
   protected Dictionary<AiStateType, AiState> stateCache = new Dictionary<AiStateType, AiState>();

   // a reference to the sybling Nav Mesh Agent component on the parent AI Entity GameObject
   private NavMeshAgent navAgent;
   public NavMeshAgent NavAgent { get { return this.navAgent; } }

   // a reference to the sybling Animator component on the parent AI Entity GameObject
   private Animator animator;
   public Animator Animator { get { return this.animator; } } 

   // the collider that surrounds the Ai Entity itself; it needs to be registered with the scene manager
   // NOTE:  this is NOT the Sensor's collider
   protected Collider entityCollider;

   // all properties and methods related to the root motion behaviors of the animator were moved to a separate class
   // it maintains the current state of the Animator root more properties
   private RootMotionProperties rootMotionProperties = new RootMotionProperties();
   public RootMotionProperties RootMotionProperties {
      get { return this.rootMotionProperties; }
   }

   // all properties and methods related to the Sensor GameObject moved to a separate class
   private SensorProperties sensor;
   public SensorProperties Sensor { get { return this.sensor; } }

   // all properties and methods related to the AI Entities current threats and target were moved to a separate class
   private AiThreatManager threatManager;
   public AiThreatManager ThreatManager { get { return this.threatManager; } }

   // all properties and methods related to the waypoint network associated to this entity are in a separate class
   private AiWaypointManager waypointManager;
   public AiWaypointManager WaypointManager { get { return this.waypointManager; } }

   /// <summary>
   /// Indicates whether or not the AI Entity is in melee range to the player.  Tracking this helps us track when to 
   /// fire the attack animations.
   /// </summary>
   public bool IsInMeleeRange { get; set; }

   /// <summary>
   /// Indicates whether or not the AI Entity has reached the destination of it's targetTrigger (or it's active target).
   /// Remember the AI Entity will send it's targetTrigger collider out from itself to the location of any new target
   /// it begins to track.
   /// </summary>
   public bool IsTargetReached { get; set; }

   /// <summary>
   /// Monobehavior life cyle method that is called before start.  Initializes all of the required components and sets up the initial
   /// state.
   /// </summary>
   protected virtual void Awake() {
      InitializeNavAgent();
      InitializeAnimatorController();
      InitializeEntityCollider();
      InitializeSensor();
      InitializeThreatManager();
      InitializeWaypointManager();
   }

   /// <summary>
   /// Monobehavior life cycle method that is called after Awake.  It performs more initialization of state machine related
   /// properties and caches.  At this time I do not know the benefit of using one over the other for certain things.
   /// </summary>
   protected virtual void Start() {
      BuildStateCache();
      InitializeStateMachineLinks();
      RegisterCollidersWithGameSceneManager();
   }

   /// <summary>
   /// called each time frame changes and it will use non-uniform times.  Gives the current state a chance to 
   /// update itself and perform transitions into other states.
   /// </summary>
   protected virtual void Update() {
      HandleDebugDisplay();
      HandlePotentialStateChange();
   }

   // 
   /// <summary>
   /// Called with each tick of the physics system, thus it is called with consistent/uniform time.
   /// </summary>
   protected virtual void FixedUpdate() {
      // delegate to the threat manager (basically it will update the threats and target with each tick)
      threatManager.PerformFixedUpdate(this.transform.position);
   }

   /// <summary>
   /// Initializes the Nav Mesh Agent sybling component.  At the moment, all it does is store a reference 
   /// to the component.
   /// </summary>
   private void InitializeNavAgent() {
      navAgent = GetComponent<NavMeshAgent>();
   }

   /// <summary>
   /// Initializes the underlying Animator Controller.  At the moment, all it does is store a reference 
   /// to the component.
   /// </summary>
   private void InitializeAnimatorController() {
      animator = GetComponent<Animator>();
   }

   /// <summary>
   /// Initializes the underlying collider..
   /// </summary>
   private void InitializeEntityCollider() {
      entityCollider = GetComponent<Collider>(); // the capsule collider that we added to AI Entity

      //Assert.IsNotNull(entityCollider, "Missing required collider on GameObject AiStateMachine is on!");
   }

   /// <summary>
   /// Initializes the sensor.
   /// </summary>
   private void InitializeSensor() {
      this.sensor = new SensorProperties(this.sensorTrigger, this);
   }

   /// <summary>
   /// Initializes all of the threats/targets.
   /// </summary>
   private void InitializeThreatManager() {
      this.threatManager = new AiThreatManager(this.targetTrigger, this);
   }

   /// <summary>
   /// Initialize the waypoint manager.  It will manage everyting related to our waypoint networks, patrolling, etc.
   /// </summary>
   private void InitializeWaypointManager() {
      this.waypointManager = GetComponent<AiWaypointManager>();
   }

   /// <summary>
   /// Initializes all of the state machine links; this should be doen in the Start().
   /// </summary>
   private void InitializeStateMachineLinks() {
      AiStateMachineLink[] scripts = this.animator.GetBehaviours<AiStateMachineLink>();
      foreach (AiStateMachineLink script in scripts) {
         script.StateMachine = this;
      }
   }

   /// <summary>
   /// Register all the important colliders with the GameSceneManager.
   /// </summary>
   private void RegisterCollidersWithGameSceneManager() {
      // register the important colliders with the scene manager
      GameSceneManager.Instance.RegisterStateMachine(this.entityCollider, this);
      GameSceneManager.Instance.RegisterStateMachine(this.sensorTrigger, this);
   }

   /// <summary>
   /// Build the state cache based upon the AiStates that have been added to this GameObject this script is also a part of.
   /// There may not be any state scripts that have been added.
   /// </summary>
   private void BuildStateCache() {
      AiState[] states = GetComponents<AiState>();
      if (states == null || states.Length == 0) {
         Debug.LogWarning("No Child AiState Script Components were added to the parent AI Entity GameObject!");
         return;
      }

      foreach (AiState state in states) {
         AiStateType key = state.GetDefaultStateType();
         if (state != null && !stateCache.ContainsKey(key)) {
            state.SetStateMachine(this);
            stateCache[key] = state;
         }
      }

      ChangeState(this.currentStateType);
   }

   /// <summary>
   /// Retrieves the AiState from the cache according to the provided type.
   /// </summary>
   /// <param name="type">The type of state to fetch.</param>
   /// <returns>The AiState if it was found or null.</returns>
   private AiState GetStateFromCache(AiStateType type) {
      AiState state;
      if (this.stateCache.TryGetValue(type, out state)) {  // wow, not a huge fan of trygetvalue with out parameter)
         return state;
      }
      Debug.LogWarningFormat(
         "State of type {0} not found in cache!  Did you forget to drag it's AiState script to the AI Entity in the inspector?",
         type
      );
      return null;
   }

   /// <summary>
   /// Handles state change if it occurs.
   /// </summary>
   private void HandlePotentialStateChange() {
      if (this.currentState == null) {
         return;
      }
      
      // allow the current state to execute for a single frame 
      // (i.e. notify it that monobehavior life-cycle method update() was called (one frame))
      AiStateType potentialNewStateType = this.currentState.OnUpdate();

      if (potentialNewStateType != this.currentStateType) {
         if (!ChangeState(potentialNewStateType)) {
            // if here, it most likely means that state's script wasn't added to the AI Entity
            if (!ChangeState(AiStateType.Idle)) {
               // if here, it most likely means the Idle state script wasn't added to the AI Entity, so just set the type
               this.currentStateType = AiStateType.Idle; // TODO: maybe make an AiState that has a NONE type so this isn't standalone
            }   
         }
      }
   }

   /// <summary>
   /// Changes the current state to the new state, but only if it exists in the cache.
   /// </summary>
   /// <param name="newState">The new state to use.</param>
   /// <returns>true if the new state exists in the cache and the state was changed.</returns>
   private bool ChangeState(AiStateType newStateType) {
      bool didStateChange = false;

      AiState newState = GetStateFromCache(newStateType);
      if (newState != null) {
         if (currentState != null) {
            currentState.OnExitState();   // give the old state a chance to cleanup
         }
         newState.OnEnterState();         // give the new state a chance to initialize
         currentState = newState;
         this.currentStateType = newStateType;     
         didStateChange = true;
      }
    
      return didStateChange;
   }

   /// <summary>
   /// Callback that fires when another collider enters our entityCollider's space (i.e. the capsule collider
   /// that is a component of the AI Entity GameObject).  Note this entityCollider does not have "Is Trigger"
   /// set.  The other collider should be the targetTrigger and it needs to have it set.
   /// In essence, whenever a zombie tracks a target or a waypoint, it will send out its targetTrigger to
   /// that known location and collide with it when it eventually arrives at it.  Thus, this callback allows the 
   /// child states to know when the zombie has reached the destination.
   /// </summary>
   /// <param name="other">The other collider, which is supposed to be the targetTrigger collider.</param>
   protected virtual void OnTriggerEnter(Collider other) {
      FireDestinationReachedEvent(true, other);
   }

   /// <summary>
   /// Callback that fires when another collider (targetTrigger) exits our entityCollider's space.
   /// This callback allows us to informs the child state that the AI entity is no longer at its destination 
   /// (typically true when a new target has been set by the child)
   /// </summary>
   /// <param name="other">The other collider, which is supposed to be the targetTrigger collider.</param>
   protected virtual void OnTriggerExit(Collider other) {
      FireDestinationReachedEvent(false, other);
   }

   /// <summary>
   /// Callback that fires when another collider (targetTrigger) is still in our entityCollider's space.
   /// This callback was needed to handle some buggy behavior that was accomplished simply by providing
   /// enter and exit.
   /// </summary>
   /// <param name="other">The other collider, which is supposed to be the targetTrigger collider.</param>
   protected virtual void OnTriggerStay(Collider other) {
      if (other != this.targetTrigger) {
         return;
      }
      IsTargetReached = true;
   }

   /// <summary>
   /// Notify the children with a destination reached event.
   /// </summary>
   /// <param name="wasReached">Whether the destination was actually reached or not.</param>
   /// <param name="other">The other collider that acts as the destination.</param>
   private void FireDestinationReachedEvent(bool wasReached, Collider other) {
      if (other != this.targetTrigger) {
         return;
      }

      // this was in an individual state but had to be brought up to the state machine to serve mulitple states
      // quesion, 
      IsTargetReached = wasReached;

      if (this.currentState != null) {
         this.currentState.OnDestinationReached(wasReached); // notify the child
      }
   }

   /// <summary>
   /// Callback that occurs when a trigger event occurs in the underlying Sensor via it's collider.
   /// This is a custom callback that Gary built that can handle any of the collider trigger events 
   /// (enter, stay, exit) that were originally configured in the AiSensor script.  Whenever that 
   /// script receives a trigger event in one of the configured monobeharior trigger callbacks, it will push
   /// those events to the associated AiStateMachine and this method will delegate the handling to the child 
   /// AiStates.
   /// </summary>
   /// <param name="type">The event type that matches the three monobehavior callback types.</param>
   /// <param name="other">The collider that triggered the event.</param>
   public virtual void OnTriggerEvent(AiTriggerEventType type, Collider other) {
      if (currentState != null) {
         currentState.OnTriggerEvent(type, other);
      }
   }

   /// <summary>
   /// Callback for processing animation movements for modifying root motion.  Delegates handling to 
   /// the current state.
   /// Called by unity after root motion has been evaluated but not applied to the object.  This allows us to
   /// determine via code what to do with the root motion information.
   /// </summary>
   protected virtual void OnAnimatorMove() {
      if (this.currentState != null) {
         this.currentState.OnAnimatorUpdated();
      }
   }

   /// <summary>
   /// Callback for setting up animation IK (inverse kinematics).  Delegates handling to the current
   /// state.  
   /// Called by unity just prior to the IK system being updated giving us a chance to setup IK targets and weights.
   /// </summary>
   protected virtual void OnAnimatorIK(int layerIndex) {
      if (this.currentState != null) {
         this.currentState.OnAnimatorIkSystemUpdated();
      }
   }

   /// <summary>
   /// Modifies the underlying Nav Mesh Agent ability to update.
   /// Configure the navmeshagent to enable/disable auto updates of position/rotation to our transform.
   /// That is, do we want the NavMeshAgent to control our position and/or rotation?
   /// In nearly every case, we'll almost always have the navAgent control the position, but not the rotation.
   /// Rotation will either be controlled via the rootmotion in the animation or if we're in a running 
   /// state and need more accuracy when facing.  Thus, we'll normally set shouldUpdatePosition to true
   /// and shouldUpdateRotation to false.
   /// </summary>
   /// <param name="shouldUpdatePosition">Set to true to have navagent control position.</param>
   /// <param name="shouldUpdateRotation">Set to true to have navagent control rotation.</param>
   public void ModifyNavAgentUpdateAttributes(bool shouldUpdatePosition, bool shouldUpdateRotation) {
      this.navAgent.updatePosition = shouldUpdatePosition;
      this.navAgent.updateRotation = shouldUpdateRotation;
   }

   /// <summary>
   /// Add the given root motion properties to the underlying root motion struct.  Basically we assigned a script called
   /// RootMotionConfigurator to most of the animations in our "Omni Zombie 1" Animator controller.  Any property change 
   /// that occurs in a particular animation that uses the script will call this method so that we can update our property
   /// object.  Our child states have a reference to this object and can check to see if they should handle root motion
   /// or not.  Three things can control motion:  The Nav Mesh Agent, the built-in root motion that is in the animations,
   /// or our own custom code.
   /// </summary>
   /// <param name="rootPosition">A value of 1 (on) or -1 (off) that increment/decrement the root motion properties.</param>
   /// <param name="rootRotation">A value of 1 (on) or -1 (off) that increment/decrement the root motion properties.</param>
   public void AddRootMotionRequest(int rootPosition, int rootRotation) {
      this.rootMotionProperties.IncrementRootPositionRefCount(rootPosition);
      this.rootMotionProperties.IncrementRootRotationRefCount(rootRotation);
   }

   /// <summary>
   /// Indicates whether or not the NavMesh path was lost and a new action might need to occur to get the entity back on path.
   /// </summary>
   /// <returns>true if the navagent has lost its path</returns>
   public bool HasLostNavMeshPath() {
      return this.NavAgent.isPathStale ||
         (!this.NavAgent.hasPath && !this.NavAgent.pathPending) ||
         this.NavAgent.pathStatus != NavMeshPathStatus.PathComplete;
   }

   /// <summary>
   /// Just provides clarity for the underlying AI Entity transform.  Either can be used by children or clients.
   /// </summary>
   public Transform AiEntityBodyTransform { get { return this.transform; } }
}
