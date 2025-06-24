using UnityEngine;
using UnityEngine.SceneManagement;

public class ExpLoader : MonoBehaviour
{
    public void GoToSceneOne()
    {
        SceneManager.LoadScene(1);
    }
}