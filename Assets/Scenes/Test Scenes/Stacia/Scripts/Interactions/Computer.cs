using UnityEngine;
using System.Collections;

public class Computer : Interaction
{
    private GameManager _gameManager;

    public GameObject playerCameraObj;
    public GameObject computerCameraObj;
    //public MonoBehaviour playerMovementScript; // reference til dit movement script
    
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

    public void InteractWithComputer()
    {
        if (!usingComputer)
        {
            // Skift til computer kamera
            playerCameraObj.SetActive(false);
            computerCameraObj.SetActive(true);
            usingComputer = true;

            // Lås player movement
            //if (playerMovementScript != null)
            ///    playerMovementScript.enabled = false;

            // Lås cursor op
            //Cursor.lockState = CursorLockMode.None;
            //Cursor.visible = true;
        }
        else
        {
            // Skift tilbage til player kamera
            playerCameraObj.SetActive(true);
            computerCameraObj.SetActive(false);
            usingComputer = false;
            /*
            // Lås player movement op
            if (playerMovementScript != null)
                playerMovementScript.enabled = true;

            // Lås cursor tilbage til FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;*/

        }
        
    }
}
