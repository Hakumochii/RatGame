using UnityEngine;

public class Level : MonoBehaviour
{
    private GameManager _gameManager;
    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    public virtual void PrepareLevel()
    {
        _gameManager.PlayCutscene();//fill this
        ToggleRatBehaviour();
    }


    public virtual void ToggleRatBehaviour(){}

}
