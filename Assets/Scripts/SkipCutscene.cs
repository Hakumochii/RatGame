using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;

public class SkipCutscene : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    public bool skip;
    private GameManager _gameManager;
    private CutsceneController _cutsceneController;
    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _playerInput = FindFirstObjectByType<PlayerInput>();
        _cutsceneController = FindFirstObjectByType<CutsceneController>();
    }

    public void OnSkip(InputAction.CallbackContext ctx)
    {
        skip = ctx.ReadValueAsButton();
    }
    private void Update()
    {
        if (skip && _gameManager.cutscenePlaying)
        {
            // Timeline cutscene takes priority check
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
    }
}
