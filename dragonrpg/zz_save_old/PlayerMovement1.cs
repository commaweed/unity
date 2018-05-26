using System;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

[RequireComponent(typeof(ThirdPersonCharacter))]
public class PlayerMovement1 : MonoBehaviour {

   private static readonly KeyCode CROUCH_KEY = KeyCode.Z; // TODO: could use the input settings 

   [SerializeField]
   float walkStopRadius = 0.2f;

   [SerializeField]
   float attackStopRadius = 5.0f;

   ThirdPersonCharacter thirdPersonCharacter;

   private bool isJumping;
   private bool isInDirectMode = true; // start off in indirect mode

   private CameraRaycaster1 rayCaster;
   private Vector3 shortenedClickPoint;
   private Vector3 clickPoint;

   private void Start() {
      if (Camera.main == null) {
         Debug.LogWarning(
             "Warning: no main camera found. Third person character needs a Camera tagged \"MainCamera\"," +
             " for camera-relative controls.", gameObject);
      }

      this.thirdPersonCharacter = GetComponent<ThirdPersonCharacter>();
      //transform.position = Vector3.zero;
      //this.shortenedClickPoint = Vector3.zero;
      this.shortenedClickPoint = transform.position;

      //rayCaster = FindObjectOfType<CameraRaycaster>();
      this.rayCaster = Camera.main.GetComponent<CameraRaycaster1>();

      // register with the CameraRaycaster that we are listening to layer hit changes
      this.rayCaster.OnHitItemChangeObservers += OnHitItemChanged;
   }

   /// <summary>
   /// Event handler that fires whenever the CameraRaycaster layer hit changes.
   /// </summary>
   private void OnHitItemChanged(HitItemChangeEvent changeEvent) {
      if (!isInDirectMode) {
         HandleClickMovement(changeEvent.NewItem);
      }
   }

   private void Update() {
      HandlePlayerJump();
   }

   private void FixedUpdate() {
      if (Input.GetKeyDown(KeyCode.G)) {
         isInDirectMode = !isInDirectMode;
         shortenedClickPoint = transform.position;
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
      thirdPersonCharacter.Move(destination, Input.GetKey(CROUCH_KEY), isJumping);
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

      // walk speed multiplier
      if (Input.GetKey(KeyCode.LeftShift)) directionToMove *= 0.5f;

      MovePlayer(directionToMove);
   }

   /// <summary>
   /// Handle the mouse click.
   /// </summary>
   private void HandleClickMovement(HitItemMetadata itemHit) {
      if (Input.GetMouseButton(0) && itemHit != null) {
         // print("Cursor raycast hit " + cameraRaycaster.hit.collider.gameObject.name.ToString()); = 
         clickPoint = itemHit.ItemHit.point;
         switch (itemHit.LayerHit) {
            case Layer.Walkable:
               shortenedClickPoint = ShortenDestination(clickPoint, this.transform.position, this.walkStopRadius);
               break;
            case Layer.Enemy:
               shortenedClickPoint = ShortenDestination(clickPoint, this.transform.position, this.attackStopRadius);
               break;
            case Layer.RaycastEndStop:  // do nothing
               break;
            default:
               print("you haven't handled the following layer yet " + itemHit.LayerHit);
               return;
         }
      }

      // move the player to the mouse click location, but only if within the correct radius of the item clicked
      Vector3 deltaVector = shortenedClickPoint - transform.position;
      // magnitudes are always positive and there is a floating arithmetic issue that causes magnitude to never hit 0 causing player to spin
      if (Mathf.MoveTowards(deltaVector.magnitude, 0, .2f) > 0) {
         this.MovePlayer(deltaVector);
      } else {
         this.MovePlayer(Vector3.zero); // don't move
      }
   }

   /// <summary>
   /// Returns the normalized distince delta, shortened by the shortening value.
   /// </summary>
   /// <param name="destination">The target destination.</param>
   /// <param name="shortening">The amount to shorten.</param>
   /// <returns>The destination minus the shortened portion.</returns>
   private static Vector3 ShortenDestination(Vector3 newDestination, Vector3 currentDestination, float shortening) {
      Vector3 reductionVector = (newDestination - currentDestination).normalized * shortening;
      return newDestination - reductionVector;
   }

   /// <summary>
   /// Callback that fires whenever the Gizmos button is pressed.  It will provide some visuals to help us determine
   /// if our move to mouse click is working.
   /// </summary>
   private void OnDrawGizmos() {
      // draw movement gizmo
      Gizmos.color = Color.black;
      Gizmos.DrawLine(transform.position, shortenedClickPoint);
      Gizmos.DrawSphere(shortenedClickPoint, 0.1f);
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(clickPoint, 0.05f);

      Gizmos.color = new Color(0f, 255f, 0, .5f);
      Gizmos.DrawWireSphere(transform.position, this.attackStopRadius);
   }

}

