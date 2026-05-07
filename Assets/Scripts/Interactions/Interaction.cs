using UnityEngine;
using System.Collections;

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
    public GameObject[] books;
    public GameObject stopCollider;
    public GameObject swingCollider;
    [SerializeField] private Transform swingLandingPoint;

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

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("KnockOverBooks"))
        {
            if(_rat.dragging)
            {
                foreach (GameObject book in books)
                {
                    book.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                }
                StartCoroutine(FreezeBooks());
            }
        }
        
        if(other.CompareTag("Swing"))
        {
            _rat.inSwingZone = true;

            if(_rat.dragging)
            {
                _rat._dragDirectionMultiplier = -2f;
                stopCollider.SetActive(true);
                swingCollider.SetActive(true);
            }
            else
            {
                stopCollider.SetActive(false);
                swingCollider.SetActive(false);
            }
        }

        if (other.CompareTag("StartSwing"))
        {
            if (_rat.dragStopped && !_rat.isSwinging && !_rat.dragging)
            {
                StartCoroutine(_rat.SwingToPosition(swingLandingPoint.position));
            }
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

    IEnumerator FreezeBooks()
    {
        yield return new WaitForSeconds(5.0f);
        Debug.Log("freezing books");
        foreach (GameObject book in books)
        {
            book.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }   
    }
    
}

