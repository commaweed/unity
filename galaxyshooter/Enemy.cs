using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Enemy : MonoBehaviour {

    // represents the speed of the enemy in meters
    [SerializeField]
    private float speed = 6.0f;

    [SerializeField]
    private GameObject enemyExplosionPrefab;

    [SerializeField]
    private AudioClip explosionClip;

    // the x and y boundary threshold values (i.e. the position limit from the origin)
    public static readonly float xThreshold = 9.15f;
    public static readonly float yThreshold = 7.5f;

    // TODO: figure out if c# has an enum like java
    // this is a home grown enum with 
    private static class ValidCollisionTag {
        public enum Tag { Laser, Player }

        private static Tag[] values() {
            return new Tag[] {
                Tag.Laser,
                Tag.Player
            };
        }

        // allow null as a return value
        public static Tag? ValueOf(string tag) {
            return values().First(e => e.ToString() == tag);
        }
    }

    /// <summary>
    /// Handles the player's movement.
    /// </summary>
    private void handleMovement() {
        // tranform position based upon input
        this.transform.Translate(Time.deltaTime * Vector3.down * speed);

        this.transform.position = enforceBoundaries(this.transform.position, xThreshold, yThreshold);
    }

    /// <summary>
    /// Enforce the boundaries for the given position and returns a new position based upon the provided xBoundary and yBoundary thresholds.
    /// </summary>
    /// <param name="position">The player's current position.</param>
    /// <param name="xThreshold">The absolute value of the x-boundary threshold.</param>
    /// <param name="yThreshold">The absolute value of the y-boundary threshold.</param>
    /// <returns>The player's current position, unless one of the xBoundary threshold values was crossed.</returns>
    private static Vector3 enforceBoundaries(Vector3 position, float xThreshold, float yThreshold) {
        // TODO: learn how to use galaxy boundaries to limit player boundary
        float X_BOUND = xThreshold, N_X_BOUND = -1 * X_BOUND;
        float Y_BOUND = yThreshold, N_Y_BOUND = -1 * Y_BOUND; ;

        Vector3 newPosition = position;

        if (position.y < N_Y_BOUND) {
            newPosition = new Vector3(Random.Range(N_X_BOUND, X_BOUND), Y_BOUND, 0);
        }

        return newPosition;
    }

    // Use this for initialization
    void Start () {
        /*
        GameObject explosionClone = Instantiate(
            enemyExplosionPrefab,       // the prefab for the explosion
            this.transform.position,    // the enemy position
            Quaternion.identity         // default rotation
        );
        */
        //this.enemyExplosionPrefab.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        handleMovement();
    }

    /// <summary>
    /// Callback that fires when the this object collides with another object using trigger collision.
    /// </summary>
    /// <param name="otherObject">The other object that has collision enabled.</param>
    private void OnTriggerEnter2D(Collider2D otherObject) {
        string tag = otherObject.gameObject.tag;

        ValidCollisionTag.Tag? lookupTag = ValidCollisionTag.ValueOf(tag);
        if (lookupTag == null) {
            Debug.Log(tag + " not valid");
        } else {
            Debug.Log(lookupTag + " hit me");
            switch (lookupTag) {
                case ValidCollisionTag.Tag.Laser:
                    Destroy(otherObject.gameObject); // destroy laser
                    break;
                case ValidCollisionTag.Tag.Player:
                    Player player = otherObject.GetComponent<Player>();
                    player.decrementLife();
                    break;
            }

            AudioSource.PlayClipAtPoint(this.explosionClip, Camera.main.transform.position, 1f);

            this.gameObject.SetActive(false);  // no longer need to destroy because we are using spawning pool
            UiManager.Instance.incrementScore();

            // create and destroy explosion effect in one call
            // TODO: maybe add a script like the instructor did to modularize code for this and player
            Destroy(
                // create the explosion effect
                Instantiate(
                    enemyExplosionPrefab,       // the prefab for the explosion
                    this.transform.position,    // the enemy position
                    Quaternion.identity         // default rotation
                ),

                // wait some time before destroying so animation can complete
                3
            );
        }
    }
}
