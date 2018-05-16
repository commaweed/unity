using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the behavior for laser objects.
/// </summary>
public class Laser : MonoBehaviour {

    private const float Y_THRESHOLD = 6.89f;

    [SerializeField]
    private float speed = 10f;

    /// <summary>
    /// Destroy the game object that is this laser or that contains this laser (i.e. parent).
    /// </summary>
    private void destroyGameObject() {
        // deprecate this method because this.transform.root should do the trick even for nested things.
        Transform parent = this.transform.parent;
        if (parent != null) {
            Destroy(parent.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// Callback that fires whenever the laser is spawned.  
    /// </summary>
	void Start () {
       
    }
	
	/// <summary>
    /// Callback that fires whenever a frame is updated for the laser.
    /// </summary>
	void Update () {
        // fire the laser upwards (it will keep going forever unless we destroy it once it hits a threshold)
        transform.Translate(Time.deltaTime * Vector3.up * speed);

        if (this.transform.position.y >= Y_THRESHOLD) {
            Destroy(this.transform.root.gameObject);
        }
    }
}
