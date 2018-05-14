using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This animator controller was created to demonstrate the attack animation while moving.
/// </summary>
[RequireComponent(typeof(Animator))]
public class SpeedController : MonoBehaviour {

   private const string SPEED_PARAM = "Speed";
   private const string ATTACK_PARAM = "Attack";

   public float speed = 0f;
   public bool attack = false;

   private Animator animatorController = null;

   private int speedHash = Animator.StringToHash(SPEED_PARAM);
   private int attachHash = Animator.StringToHash(ATTACK_PARAM);

   // Use this for initialization
   void Start () {
      InitializeAnimatorController();
	}

   /// <summary>
   /// Initializes the underlying Animator Controller.
   /// </summary>
   private void InitializeAnimatorController() {
      animatorController = GetComponent<Animator>();
      animatorController.runtimeAnimatorController = Resources.Load("SpeedAuthority") as RuntimeAnimatorController;
      animatorController.applyRootMotion = true;
   }

   // Update is called once per frame
   void Update () {
      this.animatorController.SetFloat(speedHash, speed);
      this.animatorController.SetBool(attachHash, attack);
   }
}
