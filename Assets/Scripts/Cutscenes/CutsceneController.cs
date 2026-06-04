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

        public CutsceneActor cutsceneCat;
    }

    public CutsceneData[] cutscenes;

    [Header("Gameplay References")]
    public RatBehaviour playerMovement;

    public Interaction _interaction;

    public GameObject player;

    public Animator gameCatAnimator;

    public GameManager _gameManager;


    [Header("Cutscene Actors")]
    public CutsceneActor cutsceneRatActor;

    // These stay public because SkipCutscene.cs needs access
    public PlayableDirector _activeCutsceneDirector;

    public GameObject _activeSetUp;

    void Start()
    {
        Debug.Log("CUTSCENE CONTROLLER START");

        // Hide cutscene actors at gameplay start
        cutsceneRatActor.HideActor();

        foreach (CutsceneData cutscene in cutscenes)
        {
            if (cutscene.cutsceneCat != null)
            {
                cutscene.cutsceneCat.HideActor();
            }
        }

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
            if (cutscene.useCutsceneCat && cutscene.cutsceneCat != null)
            {
                Debug.Log("SHOWING CUTSCENE CAT");

                _gameManager.cat.SetActive(false);

                cutscene.cutsceneCat.ShowActor();
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

        // Unsubscribe first to prevent OnCutsceneFinished firing twice
        _activeCutsceneDirector.stopped -= OnCutsceneFinished;

        _activeCutsceneDirector.time = _activeCutsceneDirector.duration;
        _activeCutsceneDirector.Evaluate();
        _activeCutsceneDirector.Stop();

        // Restore actors
        foreach (CutsceneData cutscene in cutscenes)
        {
            if (cutscene.director == _activeCutsceneDirector)
            {
                if (cutscene.useCutsceneRat)
                {
                    cutsceneRatActor.HideActor();
                }

                // Always restore player/camera
                player.SetActive(true);

                if (cutscene.useCutsceneCat && cutscene.cutsceneCat != null)
                {
                    cutscene.cutsceneCat.HideActor();
                    _gameManager.cat.SetActive(true);
                }

                break;
            }
        }

        ActivateObjects();
        _activeSetUp.SetActive(false);

        playerMovement.enabled = true;
        _interaction.enabled = true;
        _gameManager.cutscenePlaying = false;
    }

    void OnCutsceneFinished(PlayableDirector director)
    {
        // Stop listening first
        director.stopped -= OnCutsceneFinished;

        // ActivateObjects BEFORE resetting timeline
        // so position is set after timeline stops driving the object
        ActivateObjects();

        _activeSetUp.SetActive(false);

        foreach (CutsceneData cutscene in cutscenes)
        {
            if (cutscene.director == director)
            {
                if (cutscene.useCutsceneRat)
                {
                    cutsceneRatActor.HideActor();
                }

                // Always restore player/camera
                player.SetActive(true);

                if (cutscene.useCutsceneCat && cutscene.cutsceneCat != null)
                {
                    cutscene.cutsceneCat.HideActor();
                    _gameManager.cat.SetActive(true);
                }

                break;
            }
        }

        playerMovement.enabled = true;
        _interaction.enabled = true;
        _gameManager.cutscenePlaying = false;

        // Reset AFTER ActivateObjects so timeline doesn't overwrite the position
        director.time = 0;
        director.Evaluate();
    }

    public void ActivateObjects()
    {
        if (_activeSetUp.name == "CreditcardCutscene")
        {
            // Remove cat from timeline bindings so it stops being driven
            foreach (var output in cutscenes[1].director.playableAsset.outputs)
            {
                if (cutscenes[1].director.GetGenericBinding(output.sourceObject) == _gameManager.cat)
                {
                    cutscenes[1].director.ClearGenericBinding(output.sourceObject);
                }
            }

            _gameManager.cat.SetActive(true);

            _gameManager.catOnFloor = true;

            _gameManager.doorClosed.SetActive(false);

            _gameManager.doorOpen.SetActive(true);
        }

        if (_activeSetUp.name == "ChargerCutscene")
        {
            _gameManager.lampBefore.SetActive(false);

            _gameManager.lampAfter.SetActive(true);

            _gameManager.cat.transform.position = _gameManager.catPowerCord.transform.position;
            _gameManager.cat.transform.rotation = _gameManager.catPowerCord.transform.rotation;

            gameCatAnimator.SetBool("Playing", true);

            _gameManager.cat.SetActive(true);

            _gameManager.lampBefore.SetActive(false);
            _gameManager.lampAfter.SetActive(true);
        }
    }
}