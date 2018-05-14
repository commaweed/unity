using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Another type of main camera script that we used at one point or another.
/// </summary>
public class SmoothCameraMount : MonoBehaviour {

   public Transform mount = null;
   public float speed = 5.0f;

	// Update is called once per frame
	void LateUpdate () {
      transform.position = Vector3.Lerp(transform.position, mount.position, Time.deltaTime * speed);
      transform.rotation = Quaternion.Slerp(transform.rotation, mount.rotation, Time.deltaTime * speed);
	}
}
