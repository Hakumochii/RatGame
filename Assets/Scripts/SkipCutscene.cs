using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class SkipCutscene : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    public bool quit;
    private GameManager _gameManager;
    private CutsceneController _cutsceneController;
    private Computer _computer;
    private float _quitCooldownTimer = 0f;

    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _playerInput = FindFirstObjectByType<PlayerInput>();
        _cutsceneController = FindFirstObjectByType<CutsceneController>();
        _computer = FindFirstObjectByType<Computer>();
    }

    public void OnQuit(InputAction.CallbackContext ctx)
    {
        quit = ctx.ReadValueAsButton();
    }
  
    [SerializeField] private float quitCooldown = 2f;
    
    private void Update()
    {
        if (_quitCooldownTimer > 0f)
        {
            _quitCooldownTimer -= Time.deltaTime;
        }

        if (quit && _quitCooldownTimer <= 0f)
        {
            if (_gameManager.cutscenePlaying)
            {
                quit = false;
                _quitCooldownTimer = quitCooldown;

                if (_cutsceneController._activeCutsceneDirector != null && 
                    _cutsceneController._activeCutsceneDirector.state == PlayState.Playing)
                {
                    _cutsceneController.SkipTimelineCutscene();
                }
                else
                {
                    _gameManager.EndVideoCutscene();
                }
            }
            else if (_gameManager.usingComputer)
            {
                quit = false;
                _quitCooldownTimer = quitCooldown;
                _computer.CloseComputer();
            }
        }
        else
        {
            quit = false;
        }
    }
    
}
