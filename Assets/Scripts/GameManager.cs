using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public int currentLevel = 0;

    public bool hasPassword = false;
    public bool hasCreditCard = false;

    public bool hasPower = false;

    public bool usingComputer = false;

    public bool cutsceneHasBeenSeen = false;

    public bool catOnFloor = false;

    public VideoPlayer cutscenePlayer;

    [Header("Cutscenes")]
    public bool dontPlayFirstCutscene;
    public VideoClip intro;
    public VideoClip shelfIntro;
    public VideoClip kitchenIntro;
    public VideoClip powerIntro;
    public VideoClip scareCat;
    public VideoClip death;
    public VideoClip ending1;

    [Header("Objects")]
    public GameObject lampBefore;
    public GameObject lampAfter;
    public GameObject player;
    public GameObject respawnArea;
    public GameObject cat;
    public GameObject catPowerCord;
    public GameObject endPicture;

    [Header("Selfassigned")]
    //levels
    public Intro _intro;
    public Shelf _shelf;
    public Kitchen _kitchen;
    public Power _power;
    public Interaction _interaction;
    //Scripts
    private RatBehaviour _rat;

    // Singleton pattern because there should only be one and many scripts acess it
    private static GameManager instance;
    public static GameManager Instance
    {
        // Ensure there is always an instance of the sound manager
        get
        {
            // Check if the instance is null or has been destroyed
            if (instance == null || instance.gameObject == null)
            {
                // Find an existing instance in the scene
                instance = FindFirstObjectByType<GameManager>();

                // If no instance exists, create a new one
                if (instance == null)
                {
                    GameObject obj = new GameObject(nameof(GameManager));
                    instance = obj.AddComponent<GameManager>();
                }
            }
            return instance;
        }
    }

    
    private void Awake()
    {
        // Ensure the instance isn't destroyed when loading new scenes
        if (instance == null || instance.gameObject == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            // If another instance exists, destroy this one
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        //find scripts
        _rat = FindFirstObjectByType<RatBehaviour>();
        _intro = FindFirstObjectByType<Intro>(); 
        _shelf = FindFirstObjectByType<Shelf>(); 
        _kitchen = FindFirstObjectByType<Kitchen>(); 
        _power = FindFirstObjectByType<Power>();
        _interaction = FindFirstObjectByType<Interaction>();

        //start gameloop
        if (!dontPlayFirstCutscene)
        {
            _intro.PrepareLevel();
        }
        
   
    }

    public void ChangeLevel()
    {
        currentLevel += 1;
        cutsceneHasBeenSeen = false;
    }

    public void PlayCutscene(VideoClip cutscene)
    {
        StartCoroutine(Play(cutscene));
    }

    IEnumerator Play(VideoClip cutscene)
    {
        _rat.enabled = false;
        _interaction.enabled = false;
        double playTimeInSeconds = cutscene.length;
        cutscenePlayer.clip = cutscene;
        cutscenePlayer.Play(); 
        yield return new WaitForSeconds((float)playTimeInSeconds);
        cutscenePlayer.clip = null;
        _rat.enabled = true;
        _interaction.enabled = true;
        
    }

    public void DetermineAndPlayEnding()
    {
        _rat.enabled = false;
        _interaction.enabled = false;
        endPicture.SetActive(true);
        //defalut ending
        //PlayCutscene(ending1);
        //different conditions for different endings
    }

    public void KillAndRespawn()
    {
        PlayCutscene(death);
        player.transform.position = respawnArea.transform.position;
    }

    public void KnockOverPlant()
    {
        PlayCutscene(scareCat);
        hasPower = true;
        cat.SetActive(false);
        catOnFloor = false;
        player.transform.position = respawnArea.transform.position;
    }

}
