using UnityEngine;

public class Interaction : MonoBehaviour
{
    private GameManager _gameManager;
    private RatBehaviour _rat;
    private bool nearComputer = false;

    [Header("Scripts")]

    Computer _computer;

    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _rat = FindFirstObjectByType<RatBehaviour>();
        _computer = FindFirstObjectByType<Computer>();
    }

    void Update()
    {
        if(_rat.interact && nearComputer)
        {
            _computer.InteractWithComputer();
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Computer"))
        {
            //pressEText.SetActive(true);
            nearComputer = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Computer"))
        {
            //pressEText.SetActive(false);
            nearComputer = false;
        }
    }
    
}

