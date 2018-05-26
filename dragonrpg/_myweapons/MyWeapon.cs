using UnityEngine;
using System.Collections;
using System;

namespace Rpg.Weapon {
   [CreateAssetMenu(menuName = ("RPG/Weapon"))]
   public class MyWeapon : ScriptableObject {

      private static readonly AnimationEvent[] ZERO_ANIMATIONS = new AnimationEvent[0];

      [SerializeField]
      private Transform gripTransform;
      public Transform GripTransform {
         get { return this.gripTransform; }
      }

      [SerializeField]
      private float attackRadius = 2f;
      public float AttackRadius {
         get { return this.attackRadius; }
      }

      [SerializeField]
      private float minSecondsBetweenHits = 0.5f;
      public float MinSecondsBetweenHits {
         get { return this.minSecondsBetweenHits; } // TODO: determine if we want animation time to impact this
      }

      [SerializeField] GameObject weaponPrefab;
      [SerializeField] AnimationClip[] attackAnimations;

      public GameObject GetWeaponPrefab() {
         return this.weaponPrefab;
      }

      public AnimationClip GetAttackAnimation() {
         AnimationClip animation = this.attackAnimations[UnityEngine.Random.Range(0, attackAnimations.Length)];
         animation.events = ZERO_ANIMATIONS;
         return animation;
      }
   }
}
