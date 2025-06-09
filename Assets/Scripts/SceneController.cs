using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    private void Start()
    {

        DontDestroyOnLoad(this.gameObject);
    }

    public void StartGame()
    {

        SceneManager.LoadScene("Main", LoadSceneMode.Single);
        RemoveMenuObjectsFromDontDestroyOnLoad();
    }

    private void RemoveMenuObjectsFromDontDestroyOnLoad()
    {
        var objectsToDestroy = new string[] { "Menu" }; 

        foreach (var objName in objectsToDestroy)
        {
            GameObject obj = GameObject.Find(objName);
            if (obj != null)
            {
                Destroy(obj);
            }
        }
    }
}