using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

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

    private bool lastCutscene = false;

    [Header("Cutscenes")]
    public bool dontPlayFirstCutscene;
    public VideoClip intro;
    public VideoClip catDeath;
    public VideoClip drownDeath;
    public VideoClip ending1;

    [Header("Objects")]
    public GameObject lampBefore;
    public GameObject lampAfter;
    public GameObject player;
    public GameObject cat;
    public GameObject catPowerCord;
    public GameObject doorOpen;
    public GameObject doorClosed;
    public GameObject startControls;
    public GameObject plant;
    public GameObject plantFallen;

    [Header("Selfassigned")]
    public GameObject respawnArea;
    public Interaction _interaction;
    public RatBehaviour _rat;
    public CutsceneController _cutsceneController;
    
    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        //find scripts
        _rat = FindFirstObjectByType<RatBehaviour>();
        _interaction = FindFirstObjectByType<Interaction>();
        _cutsceneController = FindFirstObjectByType<CutsceneController>();

        //start gameloop
        if (!dontPlayFirstCutscene)
        {
            PlayCutscene(intro);
        }
        else
        {
            startControls.SetActive(true);
            SoundManager.Instance.PlayMusic(SoundManager.Instance.catBurglarMusic);
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
        SoundManager.Instance.StopContinuosly();
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

        if (lastCutscene)
        {
            PlayCredits();
        }
        else
        {
            EndVideoCutscene();
        }

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
        cutscenePlayer.time = 0;
        cutscenePlayer.frame = 0;
        cutscenePlayer.clip = null;
        _rat.enabled = true;
        _interaction.enabled = true;
        cutscenePlaying = false;

        // add null check
        if (startControls != null)
        {
            startControls.SetActive(true);
        }
        
        SoundManager.Instance.PlayMusic(SoundManager.Instance.catBurglarMusic);
    }

    public void DetermineAndPlayEnding()//would be here to chcek is multiple endings
    {
        lastCutscene = true;
        PlayCutscene(ending1);
    }

    public void KillAndRespawn(VideoClip death)
    {
        if (currentLevel == 2 && !_interaction.cardSafe && hasCreditCard)
        {
            hasCreditCard = false;
            _interaction.creditCard.SetActive(true);
            FindFirstObjectByType<UIManager>().ShowText(_interaction.LostText);
        }
        PlayCutscene(death);
        player.transform.position = respawnArea.transform.position;
    }

    public void KnockOverPlant()
    {
        _cutsceneController.PlayCutscene(3);
        cat.SetActive(false);
        catOnFloor = false;
    }

    void PlayCredits()
    {
        //play credits and reset game
        SceneManager.LoadScene(0);
    }


}
