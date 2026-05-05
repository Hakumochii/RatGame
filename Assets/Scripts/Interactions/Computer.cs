using UnityEngine;
using System.Collections;

public class Computer : MonoBehaviour
{
    private GameManager _gameManager;

    public GameObject playerCameraObj;
    public GameObject computerCameraObj;
    //public MonoBehaviour playerMovementScript; // reference til dit movement script

    private bool usingComputer = false;
    
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

    //runs when clicking a button
    public void ConfirmPassword()
    {    
        if (_gameManager.hasPassword == false)
        {
            loginScreen.SetActive(false);
            wrongPasswordScreen.SetActive(true);
        }
        else
        {
            if (_gameManager.currentLevel == 1)
            {
                _gameManager.ChangeLevel();
            }
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
            if (_gameManager.currentLevel == 2)
            {
                _gameManager.ChangeLevel();
            }
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

            //first time interaction
            if (_gameManager.currentLevel == 0)
            {
                _gameManager.ChangeLevel();
            }
            
            //last interaction
            if (_gameManager.currentLevel == 3 && _gameManager.hasPower)
            {
                _gameManager.DetermineAndPlayEnding();
            }

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

            //determine level
            if (_gameManager.currentLevel == 1)
            {
                _gameManager._shelf.PrepareLevel();
            } 
            else if (_gameManager.currentLevel == 2)
            {
                _gameManager._kitchen.PrepareLevel();
            } 
            else if (_gameManager.currentLevel == 3)
            {
                _gameManager._power.PrepareLevel();
            } 
            else
            {
                Debug.Log("somethng is wrong with level count");
            }
            
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
