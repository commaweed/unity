using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rpg.Character {
   public class FindPlayer : MonoBehaviour {

      GameObject player;

      // Use this for initialization
      void Start() {
         this.player = GameObject.FindWithTag("Player");
      }

      // Update is called once per frame
      void Update() {
         this.transform.LookAt(this.player.transform);
      }
   }
}
