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

    public VideoPlayer cutscenePlayer;

    [Header("Cutscenes")]
    public bool dontPlayFirstCutscene;
    public VideoClip intro;
    public VideoClip shelfIntro;
    public VideoClip kitchenIntro;
    public VideoClip powerIntro;
    public VideoClip Ending1;

    [Header("Objects")]
    public GameObject lampBefore;
    public GameObject lampAfter;
    public GameObject player;

    [Header("Selfassigned")]
    //levels
    public Tutorial _tutorial;
    public Shelf _shelf;
    public Kitchen _kitchen;
    public Power _power;
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
        _tutorial = FindFirstObjectByType<Tutorial>(); 
        _shelf = FindFirstObjectByType<Shelf>(); 
        _kitchen = FindFirstObjectByType<Kitchen>(); 
        _power = FindFirstObjectByType<Power>();

        //start gameloop
        if (!dontPlayFirstCutscene)
        {
            _tutorial.PrepareLevel();
        }
        
   
    }

    public void ChangeLevel()
    {
        currentLevel += 1;
    }

    public void PlayCutscene(VideoClip cutscene)
    {
        StartCoroutine(Play(cutscene));
    }

    IEnumerator Play(VideoClip cutscene)
    {
        _rat.enabled = false;
        double playTimeInSeconds = cutscene.length;
        cutscenePlayer.clip = cutscene;
        cutscenePlayer.Play(); 
        yield return new WaitForSeconds((float)playTimeInSeconds);
        cutscenePlayer.clip = null;
        _rat.enabled = true;
    }

    public void DetermineAndPlayEnding()
    {
        //
    }

}
