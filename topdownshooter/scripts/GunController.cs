using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour {

   [SerializeField] private Transform playerHandsTransform;
   [SerializeField] private Gun startingGun;

   private Gun equipedGun;

   private void Start() {
      if (startingGun != null) {
         EquipGun(startingGun);
      }
   }

   public void EquipGun(Gun gun) {
      // if a gun is already equiped, discard it
      if (this.equipedGun != null) {
         Destroy(this.equipedGun.gameObject);
      }

      // equip a new gun
      equipedGun = Instantiate(gun, playerHandsTransform.position, playerHandsTransform.rotation) as Gun;

      // child the gun to the player hands
      equipedGun.transform.parent = playerHandsTransform;
   }

   public void OnTriggerHold() {
      if (equipedGun != null) {
         equipedGun.OnTriggerHold();
      }
   }

   public void OnTriggerRelease() {
      if (equipedGun != null) {
         equipedGun.OnTriggerRelease();
      }
   }

   public void Aim(Vector3 aimPoint) {
      if (equipedGun != null) {
         equipedGun.Aim(aimPoint);
      }
   }

   public void Reload() {
      if (equipedGun != null) {
         equipedGun.Reload();
      }
   }

   public float GunHeight { get { return playerHandsTransform.position.y; } }

}
