using UnityEngine;
using System.Collections;

public class ComputerManager : MonoBehaviour
{
    private GameManager _gameManager;
    
    [SerializeField] private GameObject loginScreen;
    [SerializeField] private GameObject wrongPasswordScreen;
    [SerializeField] private GameObject hintScreen;
    [SerializeField] private GameObject desktopScreen;

    [SerializeField] private GameObject paymentScreen;
    [SerializeField] private GameObject declinedPaymentScreen;
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject lowBatteryScreen;

     [SerializeField] private float loadingTime = 5f;

    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    public void ConfirmPassword()
    {
        if (_gameManager.hasPassword == false)
        {
            loginScreen.SetActive(false);
            wrongPasswordScreen.SetActive(true);
        }
        else
        {
            loginScreen.SetActive(false);
            hintScreen.SetActive(false);
            desktopScreen.SetActive(true);
        }

    }

    public void ConfirmPayment()
    {
        if (_gameManager.hasCreditCard == false)
        {
            paymentScreen.SetActive(false);
            declinedPaymentScreen.SetActive(true);
        }
        else
        {
            declinedPaymentScreen.SetActive(false);
            paymentScreen.SetActive(false);
            loadingScreen.SetActive(true);

            StartCoroutine(LoadingSequence());
        }
    }

    private IEnumerator LoadingSequence()
    {
        yield return new WaitForSeconds(loadingTime);

        loadingScreen.SetActive(false);
        lowBatteryScreen.SetActive(true);
    }
}
