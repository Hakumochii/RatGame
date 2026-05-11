using UnityEngine;

public class Level : MonoBehaviour
{
    public GameManager _gameManager;
    void Start()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    public virtual void PrepareLevel(){}

}
