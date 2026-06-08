using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Computer : MonoBehaviour
{
    public GameObject chargerIn;
    public GameObject chargerOut;
    private GameManager _gameManager;
    public CutsceneController _cutsceneController;

    public GameObject mouseCursor; //gameobject with rawimage called cursor (placed under canvas)
    [SerializeField] private float cursorSpeed = 0.05f;
    [SerializeField] private Vector2 controllerCursorSpeed = new Vector2(0.05f, 0.005f);
    private RectTransform _cursorRect;
    private RectTransform _canvasRect;
    private Vector2 _cursorPos;

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
    private bool _isInteracting = false;

    public GameObject quitText;
    public GameObject controllerQuit;
    public GameObject keyboardQuit;


    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _cutsceneController = FindFirstObjectByType<CutsceneController>();
        _cursorRect = mouseCursor.GetComponent<RectTransform>();
        _canvasRect = mouseCursor.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    void Update()
    {
        if (_gameManager.usingComputer)
        {
            MoveCursor();
        }
    }

    void MoveCursor()
    {
        Vector2 lookInput = _gameManager._rat.look;
        lookInput.y = -lookInput.y;

        bool isController = _gameManager._rat.IsUsingController();
        
        if (isController)
        {
            _cursorPos.x += lookInput.x * controllerCursorSpeed.x * Time.deltaTime;
            _cursorPos.y += lookInput.y * controllerCursorSpeed.y * Time.deltaTime;
        }
        else
        {
            _cursorPos += lookInput * cursorSpeed * Time.deltaTime;
        }

        Vector2 halfSize = _canvasRect.sizeDelta / 2f;
        _cursorPos.x = Mathf.Clamp(_cursorPos.x, -halfSize.x, halfSize.x);
        _cursorPos.y = Mathf.Clamp(_cursorPos.y, -halfSize.y, halfSize.y);

        _cursorRect.anchoredPosition = _cursorPos;

        if (_gameManager._rat.click)
        {
            _gameManager._rat.click = false;
            CheckCursorOverUI();
        }
    }

    void CheckCursorOverUI()
    {
        Button[] buttons = GetComponentsInChildren<Button>(false); // only active buttons

        foreach (Button button in buttons)
        {
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            Vector2 cursorLocal = _cursorRect.anchoredPosition;
            Vector2 buttonLocal = buttonRect.anchoredPosition;
            Vector2 buttonSize = buttonRect.sizeDelta;

            // Simple AABB overlap in canvas local space
            if (cursorLocal.x > buttonLocal.x - buttonSize.x / 2f &&
                cursorLocal.x < buttonLocal.x + buttonSize.x / 2f &&
                cursorLocal.y > buttonLocal.y - buttonSize.y / 2f &&
                cursorLocal.y < buttonLocal.y + buttonSize.y / 2f)
            {
                button.onClick.Invoke();
                break;
            }
        }
    }

  

    //runs when clicking a button
    public void ConfirmPassword()
    {    
        loginScreen.SetActive(false);
        hintScreen.SetActive(false);
        wrongPasswordScreen.SetActive(true);
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
        paymentScreen.SetActive(false);
        declinedPaymentScreen.SetActive(true);   
    }

    private IEnumerator LoadingSequence()
    {
        yield return new WaitForSeconds(loadingTime);

        loadingScreen.SetActive(false);
        lowBatteryScreen.SetActive(true);
        yield return new WaitForSeconds(2);

        _gameManager._interaction.enabled = true; // ← add this
        CloseComputer();
        chargerIn.SetActive(false);
        chargerOut.SetActive(true);
        _cutsceneController.PlayCutscene(2);
    }

    public void InteractWithComputer()
    {
        if (_isInteracting) return;
        _isInteracting = true;
        StartCoroutine(StartInteraction());
    }

    IEnumerator StartInteraction()
    {
        _isInteracting = true;
        if (!_gameManager.usingComputer)
        {
            SoundManager.Instance.StopContinuosly();
            //last interaction
            if (_gameManager.currentLevel == 3 && _gameManager.hasPower)
            {
                _gameManager.DetermineAndPlayEnding();
            }

            // Skift til computer kamera
            playerCameraObj.SetActive(false);
            computerCameraObj.SetActive(true);
            _gameManager.usingComputer = true;

            //first time interaction
            if (_gameManager.currentLevel == 0)
            {
                bool isController = _gameManager._rat.IsUsingController();
                quitText = isController ? controllerQuit : keyboardQuit;

                if(quitText != null)
                {
                    FindFirstObjectByType<UIManager>().ShowText(quitText);
                }
            
                _gameManager.ChangeLevel();
                if (_gameManager.hasPassword == true)
                {
                    _gameManager.ChangeLevel();
                }
            }

            mouseCursor.SetActive(true);
            _cursorPos = Vector2.zero; 

            if (_gameManager.hasCreditCard == true)
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
            else if (_gameManager.hasPassword == true)
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
        
        yield return null;
        
    }

    public void CloseComputer()
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
            
            mouseCursor.SetActive(false);
            _isInteracting = false;
    }
    
}
