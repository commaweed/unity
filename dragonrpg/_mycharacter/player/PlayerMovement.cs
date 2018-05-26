using System;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.ThirdPerson;

using Rpg.CameraUi;

namespace Rpg.Character {

   [RequireComponent(typeof(AICharacterControl))]
   [RequireComponent(typeof(NavMeshAgent))]
   [RequireComponent(typeof(ThirdPersonCharacter))]
   public class PlayerMovement : MonoBehaviour {

      private GameObject walkTarget;

      // sybling components
      private ThirdPersonCharacter thirdPersonCharacter;
      private AICharacterControl aiController;
      private NavMeshAgent navMeshAgent;

      private bool isJumping;
      private bool isInDirectMode;

      private CameraRaycaster rayCaster;

      private void Start() {
         if (Camera.main == null) {
            Debug.LogWarning(
                "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\"," +
                " for camera-relative controls.", gameObject);
         }

         this.thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
         this.aiController = GetComponent<AICharacterControl>();
         this.navMeshAgent = GetComponent<NavMeshAgent>();
         this.rayCaster = Camera.main.GetComponent<CameraRaycaster>();


         // register with the CameraRaycaster that we are listening to layer hit changes
         this.rayCaster.notifyMouseClickObservers += OnHitItemChanged;

         // start off in indirect mode
         this.isInDirectMode = true;
      }

      /// <summary>
      /// Event handler that fires whenever the CameraRaycaster layer hit changes.
      /// </summary>
      private void OnHitItemChanged(RaycastHit itemHit, int layerHit) {
         if (!isInDirectMode) {
            HandleClickMovement(itemHit, layerHit);
         }
      }

      private void Update() {
         HandlePlayerJump();
      }

      private void FixedUpdate() {
         if (Input.GetKeyDown(KeyCode.G)) {
            isInDirectMode = !isInDirectMode;
            this.aiController.enabled = !this.aiController.enabled;
            this.navMeshAgent.enabled = !this.navMeshAgent.enabled;

            if (isInDirectMode) {
               if (this.walkTarget != null) Destroy(this.walkTarget);
            } else {
               this.walkTarget = new GameObject("walkTarget");
            }
         }

         if (isInDirectMode) {
            HandleDirectMovement();
         }
      }

      /// <summary>
      /// Handles the user input related to the jump key.
      /// </summary>
      private void HandlePlayerJump() {
         if (!isJumping) {
            this.isJumping = Input.GetButtonDown("Jump");
         }
      }

      /// <summary>
      /// Move the player based upon the user input.  Delegates to the thirdPersonCharacter script.
      /// </summary>
      /// <param name="destination">The direction to move.</param>
      private void MovePlayer(Vector3 destination) {
         thirdPersonCharacter.Move(destination, false, isJumping); // Input.GetKey(CROUCH_KEY) middle value
         isJumping = false;
      }

      /// <summary>
      /// Handles the direct movement mode.  That is, calculates the move direction based upon the user input and moves the player 
      /// accordingly.
      /// </summary>
      private void HandleDirectMovement() {
         Vector3 directionToMove;

         float h = Input.GetAxis("Horizontal");
         float v = Input.GetAxis("Vertical");
         if (Camera.main != null) {
            // calculate camera relative direction to move (direction to move is based upon the main camera)
            Vector3 cameraForwardDirection = Vector3.Scale(Camera.main.transform.forward, new Vector3(1, 0, 1)).normalized;
            directionToMove = v * cameraForwardDirection + h * Camera.main.transform.right;
         } else {
            // we use world-relative directions in the case of no main camera
            directionToMove = v * Vector3.forward + h * Vector3.right;
         }

         // stop walking if shift is held
         if (Input.GetKey(KeyCode.LeftShift)) directionToMove = Vector3.zero;

         MovePlayer(directionToMove);
      }

      /// <summary>
      /// Handle the mouse click.
      /// </summary>
      private void HandleClickMovement(RaycastHit itemHit, int layerHit) {
         print("layer changed " + layerHit);
         //float distance = Vector3.Distance(itemHit.transform.position, this.transform.position);
         switch (layerHit) {
            case (int)Layer.Walkable:
               this.walkTarget.transform.position = itemHit.point;
               this.aiController.SetTarget(this.walkTarget.transform);
               break;
            case (int)Layer.Enemy:
               GameObject enemy = itemHit.collider.gameObject;
               this.aiController.SetTarget(enemy.transform);
               break;
            default:
               print("you haven't handled the following layer yet " + layerHit);
               return;
         }
      }
   }
}

