using UnityEngine;

public class Level : MonoBehaviour
{
    public GameManager _gameManager;
    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    public virtual void PrepareLevel()
    {
        _gameManager.PlayCutscene(_gameManager.intro);
        ToggleRatBehaviour();
    }


    public virtual void ToggleRatBehaviour(){}

}
