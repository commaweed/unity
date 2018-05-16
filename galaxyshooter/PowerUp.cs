using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the behavior for power up objects.
/// </summary>
public class PowerUp : MonoBehaviour {

    // the x and y boundary threshold values (i.e. the position limit from the origin)
    public static readonly float xThreshold = 9.15f;
    public static readonly float yThreshold = 6.89f;

    public enum PowerUpType {
        tripleShot, speed, shield, UNSET
    }

    [SerializeField]
    private float speed = 0.3f;

    [SerializeField]
    private int powerUpTime = 5;

    [SerializeField]
    private PowerUpType powerUpType = PowerUpType.UNSET;

    [SerializeField]
    private AudioClip audioClip;

    /// <summary>
    /// Handles the player's movement.
    /// </summary>
    private void handleMovement() {
        this.transform.Translate(Time.deltaTime * Vector3.down * speed * 5.0f);

        // if the power up hits the threshold, destroy it
        if (this.transform.position.y <= -1 * yThreshold) {
            this.gameObject.SetActive(false);
        }
    }

    // Use this for initialization
    void Start () {
		
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
        // TODO: not using tag like they did in the video; determine if it is necessary when a powerup collides with something other than player

        // get the component of type player off of the object that collided with me
        Player player = otherObject.GetComponent<Player>();
        if (player != null) {
            if (PowerUpType.UNSET == this.powerUpType) {
                Debug.LogError("Invalid powerUpType; did you forget to set it in the component!");
            } else {
                player.powerUpOccurred(this.powerUpType);
            }

            AudioSource.PlayClipAtPoint(this.audioClip, Camera.main.transform.position, 1f);

            this.gameObject.SetActive(false);
        }
    }
}
