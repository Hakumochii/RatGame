using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    /*
    public bool cutsceneIsPlaying;

    public int currentLevel = 0;

    public bool hasPassword = false;
    public bool hasCreditCard = false;

    public VideoPlayer cutscenePlayer;

    [Header("Cutscenes")]
    public VideoClip intro;
    public VideoClip selfIntro;
    public VideoClip selfComplete;
    public VideoClip kitchenIntro;
    public VideoClip kitchenComplete;
    public VideoClip powerIntro;
    public VideoClip powerComplete;
    public VideoClip Ending1;

    //Scripts
    private CharacterMovement _movement;

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
        _movement = FindFirstObjectByType<CharacterMovement>();
    }

    void ChangeToLevel(int currentLevel)
    {
        if(currentLevel == 1)
        {
            
        }
    }

    void ChangeLevel()
    {
        currentLevel += 1;
    }

    void PlayCutscene(VideoClip cutscene)
    {
        StartCoroiutone(Play(cutscene));
    }

    IEnumerator Play(VideoClip cutscene)
    {
        cutsceneIsPlaying = true;
        float playTimeInSeconds = cutscene.length;
        cutscenePlayer.Play(cutscene); 
        yield return new WaitForSeconds(playTimeInSeconds);
        cutsceneIsPlaying = false;
    }*/

}
