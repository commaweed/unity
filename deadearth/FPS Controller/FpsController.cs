using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityStandardAssets.Characters.FirstPerson;

public enum PlayerMoveState {  NotMoving, Crouching, Walking, Running, NotGrounded, Landing }

/// <summary>
/// Custom First Person Character Controller.  
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FpsController : MonoBehaviour {

   // temp for testing
   public List<AudioSource> audioSources = new List<AudioSource>();
   private int audioToUse = 0; // index to fetch
   // temp for resting

   // this are members of the MouseLook script and will be delegated to that script
   [SerializeField] private float XSensitivity = 5f;
   [SerializeField] private float YSensitivity = 5f;

   // speeds are a per second value (so ensure you multiply Time.deltaTime where necessary)
   [SerializeField] private float walkSpeed = 1.0f;
   [SerializeField] private float runSpeed = 4.5f;
   [SerializeField] private float jumpSpeed = 7.5f;
   [SerializeField] private float crouchSpeed = 1.0f;
   [SerializeField] private float runStepMultiplier = 0.65f; // slow bobbing down

   // when not in the air, we want a force to act on our player that makes it feel real (it floats down otherwise)
   [SerializeField] private float stickToGroundForce = 7.5f;

   // allows us to read back the gravity value and apply that when player is not grounded
   // thus we add a gravity multiplier to give a more intense gravity force that the physics system isn't giving us
   [SerializeField] private float gravityMultiplier = 2.5f; // the push down force (2.5 x gravity)

   // use the standard assets mouse look class for mouse input and camera look control (i.e. use mouse for rotation)
   [SerializeField] private MouseLook mouseLook;

   // The script that performs the head bobbing (we need to register events with it)
   // look in the inspector to control the values for the AnimationCurve
   [SerializeField] private CurveControlledHeadBob headBobController;

   [SerializeField] private GameObject flashlight;

   // need access to the main camera (Camera.Main should work)
   private Camera camera;

   private bool jumpButtonPressed = false;

   // floating point arithmetic can skew over time, so continually store the current value of the camera local position
   // and add the bob localposition offset to that value each time
   private Vector3 originalCameraLocalPosition = Vector3.zero;

   // since we are crouching, we'll need to set it back to the original height
   private float originalCharacterHeight;

   // every update we query horizontal and vertical axis and store it here
   private Vector2 inputVector = Vector2.zero;

   // the input horizontal and vertical values (inputVector) needs to be mapped into a world object
   private Vector3 moveDirection = Vector3.zero;

   // are we currently ...
   private bool isWalking = true;
   private bool isJumping = false;
   private bool isCrouching = false;

   // we need to track through updates whether our character was grounded or not
   private bool previouslyGrounded = false;

   // keeps track of how long our character has been in the air (i.e. jumping or falling)
   // one reason to track this is for when to activate the landing animation, for example
   // when in air, we increment it, and when land reset it
   private float fallingTimer;

   // it is how we move our character ;)
   // the position of it is at the capsule's center point
   private CharacterController characterController;

   private PlayerMoveState moveState = PlayerMoveState.NotMoving;

   public PlayerMoveState MoveState { get { return this.moveState; } }
   public float WalkSpeed { get { return this.walkSpeed; } }
   public float RunSpeed { get { return this.runSpeed; } }

   //  // Monobehavior life-cycle method that is called once (after Awake())
   private void Start() {
      Assert.IsNotNull(Camera.main, "Missing main camera; did you add a FPS camera mount to this rig and tag it as main camera?");
      Assert.IsNotNull(flashlight, "Missing flashlight; did you forget to drag the spotlight flashlight to the FPS controller?");
      this.camera = Camera.main;
      this.characterController = GetComponent<CharacterController>();
   
      InitializeState();
   }

   // Monobehavior life-cycle method that is called once per frame - variable time
   private void Update() {
      UpdateState();
      UpdateMouseLook();
      DeterminePlayerMoveState();
   }

   // Monobehavior life-cycle method that is known as the physics update - called over uniform/consistent time
   // typically camera movement should occur here
   private void FixedUpdate() {
      UpdatePhysicsState();
      MovePlayer();
      HandleHeadBob();
   }

   /// <summary>
   /// Set the initial state for all properties of the player.
   /// </summary>
   private void InitializeState() {
      this.moveState = PlayerMoveState.NotMoving;
      this.originalCharacterHeight = characterController.height;
      this.flashlight.SetActive(false);
      ResetFallingTimer();
      InitializeHeadBob();
      InitializeMouseLook();
   }

   /// <summary>
   /// Initialize the simulated head bob animation.
   /// </summary>
   private void InitializeHeadBob() {
      this.originalCameraLocalPosition = this.camera.transform.localPosition;
      this.headBobController.Initialize();
      // play a foot step sound as we move around; use the animation curve of the head bob to give appearance of reality
      this.headBobController.RegisterEventCallback(1.5f, PlayFootStepSound, ClientAnimationCurveCallBackType.Vertical);
   }

   /// <summary>
   /// Initialize mouse rotation by using the standard assets script; we are currently overriding some of its properties.
   /// </summary>
   private void InitializeMouseLook() {
      this.mouseLook.Init(transform, this.camera.transform);
      this.mouseLook.XSensitivity = this.XSensitivity;
      this.mouseLook.YSensitivity = this.YSensitivity;
   }

   /// <summary>
   /// Update some of the mouse look script properties, presumably once per frame so as to pick up any changes in the inspector.
   /// </summary>
   private void UpdateMouseLook() {
      this.mouseLook.XSensitivity = this.XSensitivity;
      this.mouseLook.YSensitivity = this.YSensitivity;

      // Allow Mouse Look a chance to process mouse and rotate camera (if game is paused for example)
      if (Time.timeScale > Mathf.Epsilon) {
         mouseLook.LookRotation(transform, camera.transform);
      }
   }

   /// <summary>
   /// Determine the player move state based upon the current player overall state.
   /// </summary>
   private void DeterminePlayerMoveState() {
      // Calculate Character Status
      if (!previouslyGrounded && characterController.isGrounded) {
         if (fallingTimer > 0.5f) { // TODO: create method will good name
            // TODO: Play Landing Sound
         }

         moveDirection.y = 0f;
         isJumping = false;
         moveState = PlayerMoveState.Landing;
      } else if (!characterController.isGrounded) {
         moveState = PlayerMoveState.NotGrounded;
      } else if (characterController.velocity.sqrMagnitude < 0.01f) {
         moveState = PlayerMoveState.NotMoving;
      } else if (isCrouching) {
         moveState = PlayerMoveState.Crouching;
      } else if (isWalking) {
         moveState = PlayerMoveState.Walking;
      } else {
         moveState = PlayerMoveState.Running;
      }

      previouslyGrounded = characterController.isGrounded;
   }

   /// <summary>
   /// Updates the player overall state once per frame.
   /// </summary>
   private void UpdateState() {
      UpdateFallingTimer();

      if (!jumpButtonPressed && !isCrouching) {
         jumpButtonPressed = Input.GetButtonDown("Jump");
      }

      if (Input.GetButtonDown("Crouch")) {
         this.isCrouching = !isCrouching;
         this.characterController.height = isCrouching ? originalCharacterHeight / 2.0f : originalCharacterHeight;
      }

      if (Input.GetButtonDown("Flashlight")) {
         ToggleFlashlight();
      }
   }

   /// <summary>
   /// Updates the player state within the physics update.
   /// </summary>
   private void UpdatePhysicsState() {
      this.inputVector = ComputeInputVector();
      this.isWalking = !Input.GetKey(KeyCode.LeftShift);
   }

   /// <summary>
   /// Turns the flashlight on or off.
   /// </summary>
   private void ToggleFlashlight() {
      this.flashlight.SetActive(!this.flashlight.activeSelf);
   }

   /// <summary>
   /// Moves the player in the direction indicated by the input system.
   /// </summary>
   private void MovePlayer() {
      Vector3 desiredMoveDirection = GetNormalizedDesiredMovementDirection();
      float currentSpeed = isCrouching ? this.crouchSpeed : this.isWalking ? this.walkSpeed : this.runSpeed;

      // Scale movement by our current speed (walking value or running value)
      moveDirection.x = desiredMoveDirection.x * currentSpeed;
      moveDirection.z = desiredMoveDirection.z * currentSpeed;

      // If grounded
      if (characterController.isGrounded) {
         // Apply severe down force to keep control sticking to floor
         moveDirection.y = -stickToGroundForce;

         // If the jump button was pressed then apply speed in up direction
         // and set isJumping to true. Also, reset jump button status
         if (jumpButtonPressed) {
            moveDirection.y = jumpSpeed;
            jumpButtonPressed = false;
            isJumping = true;
            // TODO: Play Jumping Sound
         }
      } else {
         // Otherwise we are not on the ground so apply standard system gravity multiplied
         // by our gravity modifier
         moveDirection += Physics.gravity * gravityMultiplier * Time.fixedDeltaTime;
      }

      // Move the Character Controller
      characterController.Move(moveDirection * Time.fixedDeltaTime);
   }

   /// <summary>
   /// Returns the normalized desired movement direction (i.e. unit vector).
   /// </summary>
   /// <returns>A unit vector in the direction we desire the player to move.  This does not mean the
   /// player will actually end up moving that direction.</returns>
   private Vector3 GetNormalizedDesiredMovementDirection() {
      // this is direction we desire to go, but not necessarily the direction we will go
      Vector3 desiredMoveDirection = transform.forward * inputVector.y + transform.right * inputVector.x;

      // shoot a ray in the world down direction (find point directly underneath our feet)
      RaycastHit hitInfo;
      if (
         Physics.SphereCast(
            transform.position,
            characterController.radius,
            Vector3.down,  
            out hitInfo,
            characterController.height / 2f,
            1 // which geometric layers should be sensitive = 1 all layers
         )
      ) {
         // found it, so project our desired movement vector onto that plane, and return the unit vector (length of 1)
         desiredMoveDirection = Vector3.ProjectOnPlane(desiredMoveDirection, hitInfo.normal).normalized;
      }

      return desiredMoveDirection;
   }

   /// <summary>
   /// Returns a vector represents the current input system values (i.e. horizontal and vertical).  These
   /// are both float values between 0 and 1 (I believe).
   /// </summary>
   /// <returns>A vector2 containing the current horizontal and vertical values.  Note, if the sqrMagnitude > 1,
   /// we normalize the vector (i.e. unit vector).</returns>
   private static Vector2 ComputeInputVector() {
      Vector2 inputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

      // if it is > 1, we'd end up moving it at a faster speed then walk or run (so use unit length)
      if (inputVector.sqrMagnitude > 1) inputVector.Normalize(); 

      return inputVector;
   }

   // If we are falling increment timer
   private void UpdateFallingTimer() {
      if (characterController.isGrounded) {
         ResetFallingTimer();
      } else {
         this.fallingTimer += Time.deltaTime;
      }
   }

   private void ResetFallingTimer() {
      this.fallingTimer = 0.0f;
   }

   /// <summary>
   /// Handles the bobbing of the main camera that we see as the character moves.
   /// </summary>
   private void HandleHeadBob() {
      Vector3 speedZeroY = new Vector3(characterController.velocity.x, 0f, characterController.velocity.z);
      if (speedZeroY.magnitude > 0.01f) {
         camera.transform.localPosition = originalCameraLocalPosition +
            headBobController.GetLocalSpaceOffset(
               // slow bobbing down when running
               speedZeroY.magnitude * (isCrouching || isWalking ? 1f : this.runStepMultiplier)
            );
      } else {
         camera.transform.localPosition = this.originalCameraLocalPosition;
      }
   }

   /// <summary>
   /// Play the footstep sound as we move, but not if we are currently crouching.
   /// </summary>
   void PlayFootStepSound() {
      if (isCrouching) return;
      this.audioSources[this.audioToUse].Play();
      this.audioToUse = (audioToUse == 0) ? 1 : 0; // alternate between the two foot clips
   }
}
