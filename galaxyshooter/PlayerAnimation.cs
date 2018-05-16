using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour {

    private Animator animator;

	// Use this for initialization
	void Start () {
        this.animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
            animator.SetBool("turnLeft", true);
            animator.SetBool("turnRight", false);
        } else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
            animator.SetBool("turnLeft", false);
            animator.SetBool("turnRight", true);
        } else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow)) {
            animator.SetBool("turnLeft", false);
            animator.SetBool("turnRight", false);
        } 

    }
}
