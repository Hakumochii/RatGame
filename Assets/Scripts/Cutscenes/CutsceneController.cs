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

    // These stay public because SkipCutscene.cs needs access
    public PlayableDirector _activeCutsceneDirector;

    public GameObject _activeSetUp;

    void Start()
    {
        Debug.Log("CUTSCENE CONTROLLER START");

        // Hide cutscene actors at gameplay start
        cutsceneRatActor.HideActor();

        cutsceneCatActor.HideActor();

        _gameManager = FindFirstObjectByType<GameManager>();
    }

    public void PlayCutscene(int index)
    {
        Debug.Log("PLAY CUTSCENE CALLED");

        if (index >= 0 && index < cutscenes.Length)
        {
            CutsceneData cutscene = cutscenes[index];

            // Disable gameplay systems
            playerMovement.enabled = false;

            _interaction.enabled = false;

            _gameManager.cutscenePlaying = true;

            // CUTSCENE RAT
            if (cutscene.useCutsceneRat)
            {
                Debug.Log("SHOWING CUTSCENE RAT");

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
                Debug.Log("SHOWING CUTSCENE CAT");

                // Hide gameplay cat
                gameplayCat.SetActive(false);

                // Show cutscene cat
                cutsceneCatActor.ShowActor();
            }

            // Store active references
            _activeCutsceneDirector = cutscene.director;

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

        // Jump to end
        _activeCutsceneDirector.time = _activeCutsceneDirector.duration;

        _activeCutsceneDirector.Evaluate();

        // This triggers OnCutsceneFinished
        _activeCutsceneDirector.Stop();

        ActivateObjects();

        _activeSetUp.SetActive(false);
    }

    void OnCutsceneFinished(PlayableDirector director)
    {
        Debug.Log("CUTSCENE FINISHED");

        foreach (CutsceneData cutscene in cutscenes)
        {
            if (cutscene.director == director)
            {
                // CUTSCENE RAT
                if (cutscene.useCutsceneRat)
                {
                    Debug.Log("HIDING CUTSCENE RAT");

                    // Hide cutscene rat
                    cutsceneRatActor.HideActor();

                    // Show gameplay player
                    player.SetActive(true);
                }

                // CUTSCENE CAT
                if (cutscene.useCutsceneCat)
                {
                    Debug.Log("HIDING CUTSCENE CAT");

                    // Hide cutscene cat
                    cutsceneCatActor.HideActor();

                    // Show gameplay cat
                    gameplayCat.SetActive(true);
                }

                break;
            }
        }

        // Re-enable gameplay systems
        playerMovement.enabled = true;

        _interaction.enabled = true;

        _gameManager.cutscenePlaying = false;

        // Stop listening
        director.stopped -= OnCutsceneFinished;

        // Reset timeline state
        director.time = 0;

        director.Evaluate();

        ActivateObjects();

        _activeSetUp.SetActive(false);
    }

    public void ActivateObjects()
    {
        if (_activeSetUp.name == "CreditcardCutscene")
        {
            _gameManager.cat.SetActive(true);

            _gameManager.catOnFloor = true;

            _gameManager.doorClosed.SetActive(false);

            _gameManager.doorOpen.SetActive(true);
        }

        if (_activeSetUp.name == "ChargerCutscene")
        {
            _gameManager.lampBefore.SetActive(false);

            _gameManager.lampAfter.SetActive(true);
        }
    }
}