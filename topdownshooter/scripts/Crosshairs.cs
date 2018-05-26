using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour {

   [SerializeField] private LayerMask targetMask;
   [SerializeField] private SpriteRenderer dot;
   [SerializeField] private Color highlightColor;

   private Color originalColor;

   // Use this for initialization
   void Start () {
      Cursor.visible = false;
      this.originalColor = dot.color;
	}
	
	// Update is called once per frame
	void Update () {
      transform.Rotate(Vector3.forward * 40 * Time.deltaTime);
	}

   public void DetectTarget(Ray ray) {
      if (Physics.Raycast(ray, 100, targetMask)) {
         dot.color = highlightColor;
      } else {
         dot.color = originalColor;
      }
   }
}
