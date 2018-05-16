using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    #region Singleton
    public static GameManager Instance;
    public void Awake() {
        Instance = this;
    }
    #endregion

    [SerializeField]
    private GameObject player;

    private bool isGameOver;

    public void EndGame() {
        this.isGameOver = true;
    }

	// Use this for initialization
	void Start () {
        isGameOver = true;
    }
	
	// Update is called once per frame
	void Update () {
        if (isGameOver) {
            UiManager.Instance.initializeToEndGame();
            SpawnManager.Instance.DisableSpawners();
            if (Input.GetKeyDown(KeyCode.Return)) {
                this.isGameOver = false;
                Instantiate(player, new Vector3(0, 0, 0), Quaternion.identity);
                UiManager.Instance.initializeToStartGame();
                SpawnManager.Instance.EnableSpawners();
            } else if (Input.GetKey(KeyCode.Escape)) {
                Application.Quit();
            }
        }
	}
}
