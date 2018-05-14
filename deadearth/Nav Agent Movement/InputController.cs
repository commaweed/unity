using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple input controller that was used to test the early stages of an Animator and AnimatorController.
/// </summary>
[RequireComponent(typeof(Animator))]
public class InputController : MonoBehaviour {

   public bool attack = false;

   private const string HORIZONTAL_PARAM = "Horizontal";
   private const string VERTICAL_PARAM = "Vertical";
   private const string ATTACK_PARAM = "Attack";

   private Animator animatorController;

   private int hHash = Animator.StringToHash(HORIZONTAL_PARAM);
   private int vHash = Animator.StringToHash(VERTICAL_PARAM);
   private int attackHash = Animator.StringToHash(ATTACK_PARAM);

   // Use this for initialization
   void Start () {
      InitializeAnimatorController();
   }

   /// <summary>
   /// Initializes the underlying Animator Controller.
   /// </summary>
   private void InitializeAnimatorController() {
      animatorController = GetComponent<Animator>();
      animatorController.runtimeAnimatorController = Resources.Load("InputAuthority") as RuntimeAnimatorController;
      animatorController.applyRootMotion = true;
   }

   // Update is called once per frame
   void Update () {
      float xAxis = Input.GetAxis(HORIZONTAL_PARAM) * 2.32f;
      float yAxis = Input.GetAxis(VERTICAL_PARAM) * 5.66f;
      
      if (Input.GetMouseButtonDown(0)) {
         this.animatorController.SetTrigger(attackHash);
      }

      this.animatorController.SetFloat(hHash, xAxis, 0.1f, Time.deltaTime);
      this.animatorController.SetFloat(vHash, yAxis, 1.0f, Time.deltaTime);
   }
}
