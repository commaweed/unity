using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MyLevelLoader : MonoBehaviour {

    [SerializeField]
    private GameObject loadingBarPanel;

    [SerializeField]
    private Slider loadingBar;

    public void LoadLevel(int sceneIndex) {
        
        StartCoroutine(LoadSceneAsynchronously(sceneIndex));
    }

    private IEnumerator LoadSceneAsynchronously(int sceneIndex) {
        yield return new WaitForSeconds(1);
        Debug.Log("loadingbar.value=" + loadingBar.value);

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneIndex);

        operation.allowSceneActivation = false;

        //loadingBar.value = 0;


        while (!operation.isDone) {
            
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log("progress=" + progress + "|" + operation.progress);
            loadingBar.value = progress;
            //loadingText.text = progress * 100f + "%";
            yield return null;
        }

    }


}
