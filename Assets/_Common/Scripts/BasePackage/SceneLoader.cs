using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField, Scene] private string sceneToLoad;
    [SerializeField] private bool loadAsync;

    public void Load()
    {
        if (loadAsync)
            SceneManager.LoadSceneAsync(sceneToLoad);
        else
            SceneManager.LoadScene(sceneToLoad);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
	    Application.Quit();
#endif
    }
}