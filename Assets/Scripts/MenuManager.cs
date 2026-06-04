using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject settings;

    public void ChangeToScene2()
    {
        SceneManager.LoadScene(2);
    }

    public void OpenSettings()
    {
        settings.SetActive(true);
    }

    public void CloseSettings()
    {
        settings.SetActive(false);
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
