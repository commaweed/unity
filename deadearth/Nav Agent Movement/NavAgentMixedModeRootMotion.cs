using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;

/// <summary>
/// Root motion is used to calculate Agent Velocity only.
/// 
/// NavMeshAgent only updates Position (not rotation).
/// 
/// Current speed and turn rate of NavMeshAgent is computed and passed to the animator as parameters.  These 
/// are used to find the correct animation blend that most closely matches.
/// 
/// Agent is forced to always face along its desired velocity vector.
/// 
/// Root rotation is totally discarded.
/// 
/// Suffers from rotational sliding but has much better navigation accuracy.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class NavAgentMixedModeRootMotion : MonoBehaviour {

   // Animator Controller parameter constants
   private const string ANGLE_PARAM = "Angle";
   private const string SPEED_PARAM = "Speed";
   private static readonly int ANGLE_HASH = Animator.StringToHash(ANGLE_PARAM);
   private static readonly int SPEED_HASH = Animator.StringToHash(SPEED_PARAM);

   // don't allow to rotate more than degrees / second
   private const float MAX_MOVEMENT_ROTATION_PER_SECOND = 80.0f; // in degrees

   [SerializeField]
   private AiWaypointNetwork network;

   [SerializeField]
   private bool useRunAnim = false;

   private RuntimeAnimatorController withWalkController;
   private RuntimeAnimatorController withRunController;

   [SerializeField]
   private bool hasPath;
   [SerializeField]
   private bool pathPending;
   [SerializeField]
   private bool isPathStale;
   [SerializeField]
   private float remainingDistance;
   [SerializeField]
   private string currentWaypointDisplay;

   private NavMeshAgent navAgent;
   private Animator animatorController;

   private Waypoint currentWaypoint;
   private WaypointEngine engine;

   public AnimationCurve JumpCurve = new AnimationCurve();

   private float smoothAngle = 0f;

   [SerializeField]
   private bool mixedMode = true;

   // Use this for initialization
   void Start() {
      InitializeNavAgent();
      InitializeAnimatorController();
      InitializeWaypointDestination();
   }

   // Update is called once per frame
   void Update() {
      SetRuntimeAnimatorController();
      //HandleOffMeshLink();
      HandleAnimations();
      HandleFindNextWayPoint();
   }

   private RuntimeAnimatorController LoadRuntimeAnimatorController(string name) {
      RuntimeAnimatorController controller = Resources.Load(name) as RuntimeAnimatorController;
      Assert.IsNotNull(controller, "Unable to load AnimatorController with name [" + name + "]");
      return controller;
   }

   private void SetRuntimeAnimatorController() {
      this.animatorController.runtimeAnimatorController = useRunAnim ? this.withRunController : this.withWalkController;
   }

   /// <summary>
   /// Overrides the Animator."Apply Root Motion" so that it can be done by script (here).
   /// </summary>
   private void OnAnimatorMove() {
      // have our animator controller handle the rotation (i.e. like setting Animator.applyRootMotion = true)
      if (this.mixedMode && !IsInLocomotionAnimationState()) {
         transform.rotation = this.animatorController.rootRotation;
      }

      // tell navagent what direction and speed we want it to move
      if (Time.deltaTime != 0) { // e.g. if pause game = 0
         this.navAgent.velocity = this.animatorController.deltaPosition / Time.deltaTime;
      }
   }

   /// <summary>
   /// Initializes the NavAgent.
   /// </summary>
   private void InitializeNavAgent() {
      navAgent = GetComponent<NavMeshAgent>();
      navAgent.stoppingDistance = 1.0f;

      TurnOffNavAgentAutoUpdate();
   }

   /// <summary>
   /// Turns off NavAgent's ability to update our agent.
   /// </summary>
   private void TurnOffNavAgentAutoUpdate() {
      //this.navAgent.updatePosition = false;
      this.navAgent.updateRotation = false;
   }

   /// <summary>
   /// Initializes the underlying Animator Controller.
   /// </summary>
   private void InitializeAnimatorController() {
      animatorController = GetComponent<Animator>();

      // load the two controllers
      this.withWalkController = LoadRuntimeAnimatorController("MixedModeRootMotionAuthority");
      this.withRunController = LoadRuntimeAnimatorController("MixedModeRootMotionAuthorityWithRun");

      SetRuntimeAnimatorController();
      animatorController.applyRootMotion = true;
   }

   /// <summary>
   /// Initializes the Waypoint stuff, to include setting the initialze direction.
   /// </summary>
   private void InitializeWaypointDestination() {
      Assert.IsNotNull(this.network, "NavAgentExample: 'Way Point Network' is missing; did you forget to set it in inspector!");

      engine = new WaypointEngine(this.network);

      if (this.currentWaypoint == null) {
         this.currentWaypoint = engine.GetWaypoint(network.PathStartIndex);
      }

      SetWaypointDestination(false);
   }

   /// <summary>
   /// Handles all movement animations.
   /// </summary>
   private void HandleAnimations() {
      Vector3 localDesiredVelocity = ComputeLocalDesiredVelocity();
      float localDesiredAngle = ComputeDesiredMovementAngle(localDesiredVelocity);

      HandleAgentLookRotation(localDesiredAngle);
      HandleAngleAnimation(localDesiredVelocity, localDesiredAngle);

      // using calmer turn on the spot
      // z is how fast we are walking forward from our own point of view (small amount = smoother turn on the spot)
      // localDesiredVelocity.magnitude (large amount = not very good turn on the spot)
      HandleSpeedAnimation(localDesiredVelocity.z);  
   }

   private void HandleAgentLookRotation(float localDesiredAngle) {
      // we'll have issues if the agent is stopped (i.e. desiredVelocity = 0), protect against that
      if (this.navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon) { // epsilon is a very small value
         if (
            !this.mixedMode || 
            (
               this.mixedMode && 
               Mathf.Abs(localDesiredAngle) < MAX_MOVEMENT_ROTATION_PER_SECOND &&
               IsInLocomotionAnimationState()
            )
         ) {
            Quaternion lookRotation = Quaternion.LookRotation(
               this.navAgent.desiredVelocity, // forward vector that describes which direction the agent is facing
               Vector3.up  // the axis to rotate around
            );

            // smooth out the spikes (rotate from current rotation to look rotation with speed of 5 per second)
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
         } 
      }
   }

   /// <summary>
   /// Indicates wether or not the Animator is currently in the Locomotion blend state.
   /// </summary>
   /// <returns>True if it is in that state.</returns>
   private bool IsInLocomotionAnimationState() {
      return this.animatorController.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Locomotion");
   }

   /// <summary>
   /// Computes the local desired velocity of the agent.
   /// </summary>
   /// <returns>The inverse vector of the agent's desired velocity.</returns>
   private Vector3 ComputeLocalDesiredVelocity() {
      return transform.InverseTransformVector(this.navAgent.desiredVelocity);
   }

   /// <summary>
   /// Handles the Animator's angle parameter animation changes.  It will smooth the angle.
   /// </summary>
   /// <param name="localDesiredVelocity">The agent's local desired velocity.</param>
   /// <param name="desiredAngle">The agent's local desired angle.</param>
   private void HandleAngleAnimation(Vector3 localDesiredVelocity, float desiredAngle) {
      // compute the desired angle and smooth it using rotation speed / second
      this.smoothAngle = Mathf.MoveTowardsAngle(
         this.smoothAngle,
         ComputeDesiredMovementAngle(localDesiredVelocity),
         MAX_MOVEMENT_ROTATION_PER_SECOND * Time.deltaTime
      );

      this.animatorController.SetFloat(ANGLE_HASH, this.smoothAngle);
   }

   /// <summary>
   /// Handles the Animator's speed parameter animation changes.  The speed will be dampened.
   /// </summary>
   private void HandleSpeedAnimation(float speed) {
      this.animatorController.SetFloat(SPEED_HASH, speed, 0.1f, Time.deltaTime);
   }

   /// <summary>
   /// Compute the Agent's desired movement angle in degrees using the given Agent's local desired velocity.
   /// </summary>
   /// <param name="localDesiredVelocity">The agent's local desired velocity.</param>
   /// <returns></returns>
   private float ComputeDesiredMovementAngle(Vector3 localDesiredVelocity) {
      // find angle between forward vector and the local desired velocity vector in radians
      float angle = Mathf.Atan2(localDesiredVelocity.x, localDesiredVelocity.z);

      // convert to degrees (i.e. multipy by 360 / 2pi)
      angle = angle * Mathf.Rad2Deg;

      return angle;
   }

   /// <summary>
   /// Sets the waypoint destination for the nav agent
   /// </summary>
   /// <param name="shouldIncrement"></param>
   private void SetWaypointDestination(bool shouldIncrement) {
      if (this.network == null) { return; }

      currentWaypoint = (shouldIncrement ? currentWaypoint.NextWaypoint : currentWaypoint);
      
      if (currentWaypoint != null && currentWaypoint.Transform != null) {
         this.currentWaypointDisplay = currentWaypoint.ToString();
         this.navAgent.destination = currentWaypoint.Transform.position;
      }
   }

   /// <summary>
   /// Handles the finding of the next waypoint.
   /// </summary>
   private void HandleFindNextWayPoint() {
      this.hasPath = this.navAgent.hasPath;
      this.pathPending = this.navAgent.pathPending;
      this.isPathStale = this.navAgent.isPathStale;
      this.remainingDistance = this.navAgent.remainingDistance;

      //!this.hasPath
      if ((this.remainingDistance <= this.navAgent.stoppingDistance && !this.pathPending) || 
         this.navAgent.pathStatus == NavMeshPathStatus.PathInvalid
         /* || this.navAgent.pathStatus == NavMeshPathStatus.PathPartial */
      ) {
         SetWaypointDestination(true); // next waypoint
      } else if (this.isPathStale) {
         SetWaypointDestination(false); // current waypoint
      }
   }

   /// <summary>
   /// If agent is on an offmesh link then perform a jump (it is waiting for us to do something)
   /// </summary>
   private void HandleOffMeshLink() {
      if (this.navAgent.isOnOffMeshLink) {
         StartCoroutine(Jump(1.0f));
         return;
      }
   }

   /// <summary>
   /// Jump to destination on off mesh link.
   /// </summary>
   /// <param name="duration">The total time in seconds to complete the jump.</param>
   /// <returns></returns>
   IEnumerator Jump(float duration) {
      // Get the current OffMeshLink data
      OffMeshLinkData data = this.navAgent.currentOffMeshLinkData;

      // Start Position is agent current position
      Vector3 startPos = this.navAgent.transform.position;

      // End position is fetched from OffMeshLink data and adjusted for baseoffset of agent
      Vector3 endPos = data.endPos + (this.navAgent.baseOffset * Vector3.up);

      // Used to keep track of time
      float time = 0.0f;

      // Keep iterating for the passed duration
      while (time <= duration) {
         // Calculate normalized time
         float t = time / duration;

         // Lerp between start position and end position and adjust height based on evaluation of t on Jump Curve
         this.navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * Vector3.up);

         // Accumulate time and yield each frame
         time += Time.deltaTime;
         yield return null;
      }

      // All done so inform the agent it can resume control
      this.navAgent.CompleteOffMeshLink();
   }

}
