using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Assertions;

public class TrackInfo {
   public string name;
   public AudioMixerGroup group;
   public IEnumerator trackFader;
}

public class AudioManager : MonoBehaviour {

   #region Singleton
   // this works even when not in scene and is never appears to be null afterwards
   public static AudioManager Instance;

   /// <summary>
   /// Initialize at the beginning of the life cycle.  Instantiate the AudioManager one time such that it follows the
   /// Singleton pattern.
   /// </summary>
   public void Awake() {
      if (Instance == null) {
         Instance = this;
      }
   }
   #endregion

   [SerializeField] AudioMixer audioMixer;

   // Use this for initialization
   void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
