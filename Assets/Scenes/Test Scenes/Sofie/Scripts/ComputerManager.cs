using UnityEngine;

public class ComputerManager : MonoBehaviour
{
    public bool hasPassword = false;

    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject wrongPasswordScreen;
    [SerializeField] private GameObject hintScreen;
    [SerializeField] private GameObject desktopScreen;

    public void ConfirmPassword()
    {
        if (hasPassword == false)
        {
            loginScreen.SetActive(false);
            wrongPasswordScreen.SetActive(true);
            hasPassword = true;
        }
        else
        {
            loginScreen.SetActive(false);
            desktopScreen.SetActive(true);
        }

    }
}
