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

    public bool cutscenePlaying;
    private Coroutine _currentCutscene;

    [Header("Cutscenes")]
    public bool dontPlayFirstCutscene;
    public VideoClip intro;
    public VideoClip death;
    public VideoClip ending1;

    [Header("Objects")]
    public GameObject lampBefore;
    public GameObject lampAfter;
    public GameObject player;
    public GameObject cat;
    public GameObject catPowerCord;
    public GameObject endPicture;
    public GameObject doorOpen;
    public GameObject doorClosed;

    [Header("Selfassigned")]
    public GameObject respawnArea;
    public Interaction _interaction;
    private RatBehaviour _rat;
    public CutsceneController _cutsceneController;

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
        _interaction = FindFirstObjectByType<Interaction>();
        _cutsceneController = FindFirstObjectByType<CutsceneController>();

        //start gameloop
        if (!dontPlayFirstCutscene)
        {
            PlayCutscene(intro);
        }
        
   
    }

    public void ChangeLevel()
    {
        currentLevel += 1;
        cutsceneHasBeenSeen = false;
    }

    // Game Manager Script
    IEnumerator Play(VideoClip cutscene)
    {
        cutscenePlaying = true;
        _rat.enabled = false;
        _interaction.enabled = false;

        cutscenePlayer.clip = cutscene;
        cutscenePlayer.time = 0;

        // Prepare the video first, then wait until it's ready
        cutscenePlayer.Prepare();
        yield return new WaitUntil(() => cutscenePlayer.isPrepared);

        cutscenePlayer.Play();

        // Wait until the video actually finishes playing
        yield return new WaitUntil(() => !cutscenePlayer.isPlaying);

        EndVideoCutscene();
    }

    // Make sure you're storing the coroutine reference when starting it
    public void PlayCutscene(VideoClip cutscene)
    {
        if (_currentCutscene != null)
            StopCoroutine(_currentCutscene);

        _currentCutscene = StartCoroutine(Play(cutscene));
    }

    public void EndVideoCutscene()
    {
        if (_currentCutscene != null)
        {
            StopCoroutine(_currentCutscene);
            _currentCutscene = null;
        }

        cutscenePlayer.Stop();
        cutscenePlayer.time = 0;        // rewind to start
        cutscenePlayer.frame = 0;       // ensure frame is reset too
        cutscenePlayer.clip = null;
        _rat.enabled = true;
        _interaction.enabled = true;
        cutscenePlaying = false;
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
        _cutsceneController.PlayCutscene(3);
        hasPower = true;
        cat.SetActive(false);
        catOnFloor = false;
        player.transform.position = respawnArea.transform.position;
    }

}
