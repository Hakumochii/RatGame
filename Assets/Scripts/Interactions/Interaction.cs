using UnityEngine;

public class Interaction : MonoBehaviour
{
    private GameManager _gameManager;
    private RatBehaviour _rat;
    private bool nearComputer = false;
    private Computer _computer;

    [Header("Objects")]
    public GameObject stickyNote;
    public GameObject creditCard;
    public GameObject plant;

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
            _rat.interact = false;
        }
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Computer"))
        {
            //pressEText.SetActive(true);
            nearComputer = true;
        }

        if (other.CompareTag("Password") && _gameManager.currentLevel > 0)
        {
            //pressEText.SetActive(true);
            _gameManager.hasPassword = true;
            stickyNote.SetActive(false);
        }

        if (other.CompareTag("Card") && _gameManager.currentLevel > 1)
        {
            //pressEText.SetActive(true);
            _gameManager.hasCreditCard = true;
            creditCard.SetActive(false);
        }

        if (other.CompareTag("Floor") && _gameManager.catOnFloor)
        {
            if (_gameManager.currentLevel == 2)
            {
                _gameManager.hasCreditCard = false;
                creditCard.SetActive(true);
            }
            _gameManager.KillAndRespawn();
        }

        if (other.CompareTag("Plant") && _gameManager.currentLevel == 3)
        {
            _gameManager.KnockOverPlant();
            plant.SetActive(false);
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

