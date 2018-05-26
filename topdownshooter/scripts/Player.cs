using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(GunController))]
public class Player : LivingEntity {

   [SerializeField] private bool debug;

   [SerializeField] private float moveSpeed = 5f;

   [SerializeField] private Transform crosshairPrefab;

   private PlayerController playerController;
   private GunController gunController;

   private Camera mainCamera;
   private Crosshairs crosshairs;

	protected override void Start () {
      base.Start();
      InitializeComponents();
	}

   private void InitializeComponents() {
      this.playerController = GetComponent<PlayerController>();
      this.gunController = GetComponent<GunController>();
      this.mainCamera = Camera.main;
      this.crosshairs = crosshairPrefab.GetComponent<Crosshairs>();
   }
	
	void Update () {
      HandlePlayerMovement();
      HandlePlayerRotation();
      HandleWeaponInput();
   }

   /// <summary>
   /// Handles the player movement during the update phase.
   /// </summary>
   private void HandlePlayerMovement() {
      // raw means to turn off default smoothing
      Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

      // direction * speed 
      Vector3 moveVelocity = moveInput.normalized * moveSpeed;

      playerController.Move(moveVelocity);
   }

   /// <summary>
   /// Handles the player rotation during the update phase.  This represents where we want the player to look.
   /// </summary>
   private void HandlePlayerRotation() {
      // show a ray from the camera down to the mouse position on the ground
      Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

      // we don't need a reference to the ground in the scene; we instead can generate one programmatically and that can serve as the ground
      Plane groundPlane = new Plane(
         Vector3.up,    // pass in the normal of the plane, which is a direction perpendicular to a plane that is lying flat
         Vector3.up * gunController.GunHeight   // the endpoint doesn't matter
      );

      // if the ray intersects (i.e. hit) the ground plane, then we'll know the length from camera to intersection (i.e. ray distance)
      float rayDistance;
      if (groundPlane.Raycast(ray, out rayDistance)) {
         // actual point of intersection 
         Vector3 point = ray.GetPoint(rayDistance);

         // draw a line in the scene view showing where the player will be looking
         if (debug) Debug.DrawLine(ray.origin, point, Color.red);

         // rotate the player to look at the point
         playerController.LookAt(point);

         crosshairs.transform.position = point;
         crosshairs.DetectTarget(ray);

         // debug for finding distance between crosshair dot point and player (to determine when crosshairs are too close)
         // squared magnitude is faster than magnitude
         float distance = (new Vector2(point.x, point.z) - new Vector2(transform.position.x, transform.position.z)).sqrMagnitude;
         if (distance > 1.21f) {
            // better aiming to crosshairs
            gunController.Aim(point);
         }
      }
   }

   private void HandleWeaponInput() {
      if (Input.GetMouseButton(0)) {
         gunController.OnTriggerHold();
      }

      if (Input.GetMouseButtonUp(0)) {
         gunController.OnTriggerRelease();
      }

      if (Input.GetKeyDown(KeyCode.R)) {
         gunController.Reload();
      }
   }
}
