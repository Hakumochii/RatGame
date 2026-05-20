using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneData
    {
        public PlayableDirector director;

        public bool useCutsceneRat;

        public bool useCutsceneCat;
    }

    public CutsceneData[] cutscenes;

    [Header("Gameplay References")]
    public RatBehaviour playerMovement;

    public GameObject player;

    public GameObject gameplayCat;

    [Header("Cutscene Actors")]
    public GameObject cutsceneRat;

    public GameObject cutsceneCat;

    public void PlayCutscene(int index)
    {
        if (index >= 0 && index < cutscenes.Length)
        {
            CutsceneData cutscene = cutscenes[index];

            // Disable gameplay movement
            playerMovement.enabled = false;

            // CUTSCENE RAT
            if (cutscene.useCutsceneRat)
            {
                // Match cutscene rat to gameplay player
                cutsceneRat.transform.position = player.transform.position;
                cutsceneRat.transform.rotation = player.transform.rotation;

                // Hide gameplay player
                player.SetActive(false);

                // Show cutscene rat
                cutsceneRat.SetActive(true);
            }

            // CUTSCENE CAT
            if (cutscene.useCutsceneCat)
            {
                // Hide gameplay cat
                gameplayCat.SetActive(false);

                // Show cutscene cat
                cutsceneCat.SetActive(true);
            }

            // Listen for cutscene ending
            cutscene.director.stopped += OnCutsceneFinished;

            // Play timeline
            cutscene.director.Play();
        }
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
                    cutsceneRat.SetActive(false);

                    // Show gameplay player
                    player.SetActive(true);
                }

                // CUTSCENE CAT
                if (cutscene.useCutsceneCat)
                {
                    // Hide cutscene cat
                    cutsceneCat.SetActive(false);

                    // Show gameplay cat
                    gameplayCat.SetActive(true);
                }

                break;
            }
        }

        // Re-enable gameplay movement
        playerMovement.enabled = true;

        // Stop listening
        director.stopped -= OnCutsceneFinished;
    }
}