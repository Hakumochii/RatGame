using UnityEngine;

public class Level : MonoBehaviour
{
    public GameManager _gameManager;
    public CutsceneController _cutsceneController;
    void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _cutsceneController = FindFirstObjectByType<CutsceneController>();
    }

    public virtual void PrepareLevel(){}

}
