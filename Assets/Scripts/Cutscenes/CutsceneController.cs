using UnityEngine;
using UnityEngine.Playables;
using Cinemachine;

public class CutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneData
    {
        public GameObject setUp;
        public PlayableDirector director;

        public bool useCutsceneRat;

        public bool useCutsceneCat;
    }

    public CutsceneData[] cutscenes;

    [Header("Gameplay References")]
    public RatBehaviour playerMovement;
    public Interaction _interaction;

    public GameObject player;

    public GameObject gameplayCat;
    public GameManager _gameManager;

    [Header("Cutscene Actors")]
    public CutsceneActor cutsceneRatActor;

    public CutsceneActor cutsceneCatActor;

    public PlayableDirector _activeCutsceneDirector;
    
    public GameObject _activeSetUp;



    void Start()
    {
        // Hide cutscene actors at gameplay start
        cutsceneRatActor.HideActor();

        cutsceneCatActor.HideActor();

        _gameManager = FindFirstObjectByType<GameManager>();
    }

    public void PlayCutscene(int index)
    {
        if (index >= 0 && index < cutscenes.Length)
        {
            CutsceneData cutscene = cutscenes[index];

            // Disable gameplay movement
            //playerCamera.SetActive(false);
            playerMovement.enabled = false;
            _interaction.enabled = false;
            _gameManager.cutscenePlaying = true;

            // CUTSCENE RAT
            if (cutscene.useCutsceneRat)
            {
                // Match cutscene rat to gameplay player
                cutsceneRatActor.transform.position = player.transform.position;

                cutsceneRatActor.transform.rotation = player.transform.rotation;

                // Hide gameplay player
                player.SetActive(false);

                // Show cutscene rat
                cutsceneRatActor.ShowActor();
            }

            // CUTSCENE CAT
            if (cutscene.useCutsceneCat)
            {
                // Hide gameplay cat
                gameplayCat.SetActive(false);

                // Show cutscene cat
                cutsceneCatActor.ShowActor();
            }

            _activeCutsceneDirector = cutscene.director; // store reference
            _activeSetUp = cutscene.setUp;

            // Listen for cutscene ending
            cutscene.director.stopped += OnCutsceneFinished;

            // Play timeline
            cutscene.director.Play();
        }
    }

    public void SkipTimelineCutscene()
    {
        if (_activeCutsceneDirector == null) return;

        // Seek to the very end — this fires the stopped event naturally
        _activeCutsceneDirector.time = _activeCutsceneDirector.duration;
        _activeCutsceneDirector.Evaluate();
        _activeCutsceneDirector.Stop(); // triggers OnCutsceneFinished
        ActivateObjects();
        _activeSetUp.SetActive(false);
    }
    
    void OnCutsceneFinished(PlayableDirector director)
    {
        foreach (CutsceneData cutscene in cutscenes)
        {
            if (cutscene.director == director)
            {
                // CUTSCENE RAT
                if (cutscene.useCutsceneRat)
                {
                    // Hide cutscene rat
                    cutsceneRatActor.HideActor();

                    // Show gameplay player
                    player.SetActive(true);
                }

                // CUTSCENE CAT
                if (cutscene.useCutsceneCat)
                {
                    // Hide cutscene cat
                    cutsceneCatActor.HideActor();

                    // Show gameplay cat
                    gameplayCat.SetActive(true);
                }

                break;
            }
        }

        // Re-enable gameplay movement
        playerMovement.enabled = true;
        _interaction.enabled = true;
        _gameManager.cutscenePlaying = false;

        // Stop listening
        director.stopped -= OnCutsceneFinished;

        // Reset timeline completely
        director.time = 0;

        director.Evaluate();

        //playerCamera.SetActive(true);
        ActivateObjects();
        _activeSetUp.SetActive(false);

    }

    public void ActivateObjects()
    {
        if (_activeSetUp.name == "CreditcardCutscene")
        {
            _gameManager.cat.SetActive(true);
            _gameManager.catOnFloor = true;
            _gameManager.doorOpen.SetActive(true);
        }
    }
}