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
public class NavAgentPartialRootMotion : MonoBehaviour {

   // Animator Controller parameter constants
   private const string ANGLE_PARAM = "Angle";
   private const string SPEED_PARAM = "Speed";
   private static readonly int ANGLE_HASH = Animator.StringToHash(ANGLE_PARAM);
   private static readonly int SPEED_HASH = Animator.StringToHash(SPEED_PARAM);

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

   private float smoothAngle = 0f;

   // Use this for initialization
   void Start() {
      InitializeNavAgent();
      InitializeAnimatorController();
      InitializeWaypointDestination();
   }

   // Update is called once per frame
   void Update() {
      SetRuntimeAnimatorController();
      HandleAgentLookRotation();
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
      this.withWalkController = LoadRuntimeAnimatorController("PartialRootMotionAuthority");
      this.withRunController = LoadRuntimeAnimatorController("PartialRootMotionAuthorityWithRun");

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

   private void HandleAgentLookRotation() {
      // we'll have issues if the agent is stopped (i.e. desiredVelocity = 0), protect against that
      if (this.navAgent.desiredVelocity.sqrMagnitude > Mathf.Epsilon) { // epsilon is a very small value
         Quaternion lookRotation = Quaternion.LookRotation(
            this.navAgent.desiredVelocity, // forward vector that describes which direction the agent is facing
            Vector3.up  // the axis to rotate around
         );

         // smooth out the spikes (rotate from current rotation to look rotation with speed of 5 per second)
         transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5.0f * Time.deltaTime);
      }
   }

   /// <summary>
   /// Handles all movement animations.
   /// </summary>
   private void HandleAnimations() {
      Vector3 localDesiredVelocity = ComputeLocalDesiredVelocity();
      HandleAngleAnimation(localDesiredVelocity);

      // using calmer turn on the spot
      // z is how fast we are walking forward from our own point of view (small amount = smoother turn on the spot)
      // localDesiredVelocity.magnitude (large amount = not very good turn on the spot)
      HandleSpeedAnimation(localDesiredVelocity.z);  
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
   private void HandleAngleAnimation(Vector3 localDesiredVelocity) {
      float maxMovementRotationPerSecond = 80.0f; // never can rotate more than degrees / second

      // compute the desired angle and smooth it using rotation speed / second
      this.smoothAngle = Mathf.MoveTowardsAngle(
         this.smoothAngle,
         ComputeDesiredMovementAngle(localDesiredVelocity),
         maxMovementRotationPerSecond * Time.deltaTime
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

}
