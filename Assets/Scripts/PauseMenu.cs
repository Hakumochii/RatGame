using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    private GameManager _gameManager;

    public GameObject menuPanelControl;
    public GameObject menuPanelKey;

    private GameObject menuPanel;
    public bool isOpen;

    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _playerInput = FindFirstObjectByType<PlayerInput>();
    }

    public void OnMenu(InputAction.CallbackContext ctx)
    {
        if (!ctx.started) return;

        bool isController = _gameManager._rat.IsUsingController();

        // Hide whichever panel is currently open before reassigning
        if (menuPanel != null) menuPanel.SetActive(false);

        menuPanel = isController ? menuPanelControl : menuPanelKey;
        isOpen = !isOpen;

        menuPanel.SetActive(isOpen);
        _gameManager._rat.enabled = !isOpen;
        _gameManager._interaction.enabled = !isOpen;

        if (isOpen) SoundManager.Instance.StopContinuosly();
    }
}
