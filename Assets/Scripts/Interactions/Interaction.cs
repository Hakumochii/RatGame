using UnityEngine;
using System.Collections;

public class Interaction : MonoBehaviour
{
    private GameManager _gameManager;
    private RatBehaviour _rat;
    private bool nearComputer = false;
    private Computer _computer;
    private bool stoveStarted = false;
    private bool stoveOn = false;
    private bool booksFallen = false;

    [Header("Objects")]
    public GameObject stickyNote;
    public GameObject creditCard;
    public GameObject plant;
    public GameObject[] books;
    public GameObject bookDrag;
    public GameObject stopCollider;
    public GameObject swingCollider;
    [SerializeField] private Transform swingLandingPoint;
    public Material stoveMaterial;

    [Header("Stove timer")]
    public float duration = 5f;
    private float timer = 0f;
    

    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
        _rat = FindFirstObjectByType<RatBehaviour>();
        _computer = FindFirstObjectByType<Computer>();
        stoveMaterial.color = new Color(stoveMaterial.color.r, stoveMaterial.color.g, stoveMaterial.color.b, 0f);

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

        if (other.CompareTag("Water"))
        {
            StartCoroutine(KillWater());
        }

        if (other.CompareTag("CanDrag"))
        {
            _rat.canDrag = true;
        }

        if (other.CompareTag("BookDragPass") && booksFallen)
        {
            bookDrag.SetActive(false);
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
                //_rat._dragDirectionMultiplier = -1f;
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

        if (other.CompareTag("Stove"))
        {
            if (_rat.canDrag)
            {
                if (stoveStarted == false)
                {
                    stoveStarted = true;
                    StartCoroutine(TurnOnStove());
                }
                else if (stoveOn && !_rat.isSwinging)
                {
                    StartCoroutine(_rat.KnockBack());
                }    
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
        yield return new WaitForSeconds(2f);
        booksFallen = true;
        yield return new WaitForSeconds(3f);
        Debug.Log("freezing books");
        foreach (GameObject book in books)
        {
            book.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        } 
          
    }

    IEnumerator TurnOnStove()
    {
        while (stoveStarted)
        {
            // Fade in
            yield return StartCoroutine(FadeStove(0f, 1f));
            stoveOn = true;

            yield return new WaitForSeconds(2f);

            // Fade out
            stoveOn = false;
            yield return StartCoroutine(FadeStove(1f, 0f));

            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator FadeStove(float startA, float endA)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            Color c = stoveMaterial.color;
            stoveMaterial.color = new Color(c.r, c.g, c.b, Mathf.Lerp(startA, endA, t));

            yield return null; // Wait one frame, then continue the loop
        }

        // Ensure we land exactly on the target alpha
        Color final = stoveMaterial.color;
        stoveMaterial.color = new Color(final.r, final.g, final.b, endA);
    }

    IEnumerator KillWater()
    {
        yield return new WaitForSeconds(0.5f);
        _gameManager.KillAndRespawn();
    }
    
}

