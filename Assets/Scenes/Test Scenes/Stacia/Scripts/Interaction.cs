using UnityEngine;

public class Interaction : CharacterMovement
{
    /*
    private GameManager _gameManager;
    public bool interact;
    private bool nearComputer = false;
    private bool usingComputer = false;

    [Header("Scripts")]

    Computer computerScript;
    
    public void OnInteract(InputAction.CallbackContext ctx)
    {
        interact = ctx.ReadValueAsButton();
    }

    void Awake()
    {
        _gameManager = FindFirstObjectByType<GameManager>();
    }

    void Update()
    {
        if(interact && nearComputer)
        {
            computerScript.InteractWithComputer();
            if(_gameManager.currentLevel == 0 && !usingComputer)
            {
                //start level 1
            }
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
    */
}

