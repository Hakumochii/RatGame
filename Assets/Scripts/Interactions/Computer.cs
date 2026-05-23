using UnityEngine;
using System.Collections;

public class Computer : MonoBehaviour
{
    private GameManager _gameManager;
    public CutsceneController _cutsceneController;

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

    private bool hasSeenHint = false;

    private bool hasSeenPayment = false;

    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _cutsceneController = FindFirstObjectByType<CutsceneController>();
    }

    //runs when clicking a button
    public void ConfirmPassword()
    {    
        if (_gameManager.hasPassword == false)
        {
            loginScreen.SetActive(false);
            hintScreen.SetActive(false);
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

    public void BuyCheeseButton()
    {
        hasSeenPayment = true;
    }

    public void HintButton()
    {
        hasSeenHint = true;
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
            _gameManager._interaction.enabled = false;
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
        yield return new WaitForSeconds(2);
        InteractWithComputer();
        _gameManager.lampBefore.SetActive(false);
        _gameManager.lampAfter.SetActive(true);
        _gameManager.PlayCutscene(_gameManager.powerIntro);
    }

    public void InteractWithComputer()
    {
        StartCoroutine(StartInteraction());
    }

    IEnumerator StartInteraction()
    {
        if (!_gameManager.usingComputer)
        {
            // Skift til computer kamera
            playerCameraObj.SetActive(false);
            computerCameraObj.SetActive(true);
            _gameManager.usingComputer = true;

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

            // Lås cursor op
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (hasSeenHint && !_gameManager.cutsceneHasBeenSeen || _gameManager.currentLevel > 1)
            {
                //determine level
                if (_gameManager.currentLevel == 1)
                {
                    _cutsceneController.PlayCutscene(0);
                } 
                else if (_gameManager.currentLevel == 2 && hasSeenPayment && !_gameManager.cutsceneHasBeenSeen)
                {
                    _cutsceneController.PlayCutscene(1);
                }
                
                _gameManager.cutsceneHasBeenSeen = true;
                
            }
            

            // Skift tilbage til player kamera
            playerCameraObj.SetActive(true);
            computerCameraObj.SetActive(false);
            _gameManager.usingComputer = false;
            
            // Lås cursor tilbage til FPS
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        
        yield return null;
        
    }
    
}
