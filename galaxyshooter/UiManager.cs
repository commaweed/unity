using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour {

    #region Singleton
    public static UiManager Instance;
    public void Awake() {
        Instance = this;
    }
    #endregion 

    [SerializeField]
    private Sprite[] livesSprites;

    [SerializeField]
    private Image playerLivesDisplay;

    [SerializeField]
    private GameObject titleView;

    [SerializeField]
    private Text scoreTextField;

    private int score;

    // initialize
    public void initializeToEndGame() {
        updateLives(0);
        this.titleView.SetActive(true);
    }

    public void initializeToStartGame() {
        resetScore();
        // max lives is really controlled by the # of live sprites (minus the zero state) (versus player lives)
        updateLives(livesSprites.Length - 1);   
        this.titleView.SetActive(false);
    }

    public void updateLives(int numLives) {
        if (numLives >=0 && numLives < this.livesSprites.Length) {
            playerLivesDisplay.sprite = livesSprites[numLives];
        }
    }

    private void updateScoreDisplay() {
        scoreTextField.text = "Score: " + score;
    }

    public void incrementScore() {
        score += 1;
        updateScoreDisplay();
    }

    private void resetScore() {
        score = 0;
        updateScoreDisplay();
    }

}
