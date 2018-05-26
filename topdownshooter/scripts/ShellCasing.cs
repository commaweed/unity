using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShellCasing : MonoBehaviour {

   [SerializeField] private Rigidbody shellBody;
   [SerializeField] private float forceMin;
   [SerializeField] private float forceMax;

   private float lifeTime = 4f;
   private float fadeTime = 2f;

   // Use this for initialization
   void Start () {
      float force = Random.Range(forceMin, forceMax);
      shellBody.AddForce(transform.right * force);
      shellBody.AddTorque(Random.insideUnitSphere * force);
      StartCoroutine(FadeRoutine());
   }

   IEnumerator FadeRoutine() {
      yield return new WaitForSeconds(lifeTime);

      float percent = 0;
      float fadeSpeed = 1 / fadeTime;
      Material mat = GetComponent<Renderer>().material;
      Color initialColor = mat.color;

      while (percent < 1) {
         percent += Time.deltaTime * fadeSpeed;
         mat.color = Color.Lerp(initialColor, Color.clear, percent);
         yield return null;
      }

      Destroy(gameObject);
   }
}
