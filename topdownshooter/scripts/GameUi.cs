using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameUi : MonoBehaviour {

   [SerializeField] private Image fadeScreen;

   [SerializeField] private GameObject gameOverUi;

	// Use this for initialization
	void Start () {
      Cursor.visible = true;
      FindObjectOfType<Player>().OnDeath += OnGameOver;
	}
	
   private void OnGameOver() {
      StartCoroutine(FadeRoutine(Color.clear, Color.black, 1f));
      gameOverUi.SetActive(true);
   }

   IEnumerator FadeRoutine(Color from, Color to, float time) {
      float speed = 1 / time;
      float percent = 0;

      while (percent < 1) {
         percent += Time.deltaTime * speed;
         fadeScreen.color = Color.Lerp(from, to, percent);
         yield return null;
      }
   }

   public void StartNewGame() {
      SceneManager.LoadScene(0);
   }
}
