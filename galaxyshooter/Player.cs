using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the player ship and all it's behavior.
/// </summary>
public class Player : MonoBehaviour {

    // the x and y boundary threshold values (i.e. the position limit from the origin)
    private const float xThreshold = 10.5f;
    private const float yThreshold = 5.0f;

    // represents the speed of the player in meters
    [SerializeField]
    private float speed = 5.0f;

    // the amount of time in seconds that must pass before player can fire again
    [SerializeField]
    private float laserFireRate = 0.25f;

    // can also use Transform in place of GameObject
    [SerializeField]
    private GameObject laserPrefab;

    [SerializeField]
    private GameObject tripleShotPreFab;

    [SerializeField]
    private GameObject playerExplosionPrefab;

    [SerializeField]
    private GameObject playerShieldPrefab;

    [SerializeField]
    private GameObject leftEnginePrefab;

    [SerializeField]
    private GameObject rightEnginePrefab;

    private int numLives = 3;

    private GameObject galaxy;

    // tracks the time for when the player can fire again
    private float nextFireTime = 0.0f;

    List<PowerUp.PowerUpType> activePowerUps = new List<PowerUp.PowerUpType>();

    private AudioSource audioSource;

    /// <summary>
    /// Indicates whether or not the given powerUpType is active.
    /// </summary>
    /// <param name="powerUpType">The power up type to test.</param>
    /// <returns>true if the given power up type is currently active.</returns>
    private bool isPowerUpActive(PowerUp.PowerUpType powerUpType) {
        return activePowerUps.Contains(powerUpType);
    }

    /// <summary>
    /// Decrement the player life by one until end game is reached, but not if shield is present.
    /// </summary>
    public void decrementLife() {
        if (isPowerUpActive(PowerUp.PowerUpType.shield)) {
            activePowerUps.Remove(PowerUp.PowerUpType.shield);
        } else {
            this.numLives--;

            switch (this.numLives) {
                case 0:
                    // destroy the player
                    Destroy(this.gameObject);
               
                    // create and destroy explosion effect in one call
                    Destroy(
                        // create the explosion effect
                        Instantiate(
                            playerExplosionPrefab,       // the prefab for the explosion
                            this.transform.position,    // the enemy position
                            Quaternion.identity         // default rotation
                        ),

                        // wait some time before destroying so animation can complete
                        3
                    );

                    GameManager.Instance.EndGame();
                    break;
                case 1:
                    this.leftEnginePrefab.SetActive(true);
                    break;
                case 2:
                    this.rightEnginePrefab.SetActive(true);
                    break;
 
            } 
        }

        UiManager.Instance.updateLives(numLives);
    }

    /// <summary>
    /// Enables the triple shot power up for a certain amount of time.
    /// </summary>
    public void powerUpOccurred(PowerUp.PowerUpType powerUpType) {
        activePowerUps.Add(powerUpType);
        StartCoroutine(delayPowerUp(powerUpType));    
    }

    /// <summary>
    /// Apply the given power up type, but only after a certain amount has elapsed, then power down.
    /// </summary>
    /// <returns></returns>
    private IEnumerator delayPowerUp(PowerUp.PowerUpType powerUpType) {
        yield return new WaitForSeconds(5.0f);
        activePowerUps.Remove(powerUpType);
    }

    /// <summary>
    /// Spawns a new laser at the player's position using the appropriate prefab according to the current state of hasPowerUp.
    /// </summary>
    private void spawnLaser() {
        Vector3 position = this.transform.position;
        GameObject prefab = this.tripleShotPreFab;
        if (!isPowerUpActive(PowerUp.PowerUpType.tripleShot)) {
            prefab = this.laserPrefab;
            position += new Vector3(0, 1.53f, 0);
        }

        Instantiate(
            prefab,
            position,               // the player's position + the desired position of the laser
            Quaternion.identity     // default rotation
        );

        audioSource.Play();
    }

    /// <summary>
    /// Fire the laser, but only if the cooldown has been reached.
    /// </summary>
    /// <returns>true if the laser was actually fired and false if it is on cooldown.</returns>
    private bool fireLaser() {
        bool wasFired = Time.time > nextFireTime;

        if (Time.time >= nextFireTime) {
            nextFireTime = Time.time + laserFireRate;
            spawnLaser();
        }

        return wasFired;
    }

    /// <summary>
    /// Handles the player's shooting.
    /// </summary>
    private void handleShooting() {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))) {
            bool wasFired = fireLaser();  // not sure we care about the return value
        }
    }

    /// <summary>
    /// Handles the player's movement.
    /// </summary>
    private void handleMovement() {
        // TODO: see if they have an enum or constant for these error-prone strings
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");

        float speedToUse = speed;
        if (isPowerUpActive(PowerUp.PowerUpType.speed)) {
            speedToUse += 10.0f;
        }

        // tranform position based upon input
        this.transform.Translate(Time.deltaTime * Vector3.right * speedToUse * hInput);
        this.transform.Translate(Time.deltaTime * Vector3.up * speedToUse * vInput);

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

        if (position.x > X_BOUND) {
            newPosition = new Vector3(N_X_BOUND, position.y, 0);
        } else if (position.x < N_X_BOUND) {
            newPosition = new Vector3(X_BOUND, position.y, 0);
        }

        if (position.y > Y_BOUND) {
            newPosition = new Vector3(position.x, Y_BOUND, 0);
        } else if (position.y < N_Y_BOUND) {
            newPosition = new Vector3(position.x, N_Y_BOUND, 0);
        }

        return newPosition;
    }

    private void handleVisuals() {
        playerShieldPrefab.SetActive(isPowerUpActive(PowerUp.PowerUpType.shield));
    }

    /// <summary>
    /// Callback that fires a player is instantiated.   Use this for initialization - called one time.
    /// </summary>
    void Start() {
        Debug.Log(this.ToString() + " start()");
        audioSource = GetComponent<AudioSource>();

        this.leftEnginePrefab.SetActive(false);
        this.rightEnginePrefab.SetActive(false);
    }

    /// <summary>
    /// Callback that fires for every frame.  Use it to update the current frame.  Not meant to be used with physics-related rigid bodies.
    /// </summary>
    void Update() {
        handleMovement();
        handleShooting();
        handleVisuals();
    }
}

// save references to certain game objectgs
//this.galaxy = GameObject.FindWithTag("GalaxyTag");
// GameObject.Find("Canvas").GetComponent<UiManager>();


