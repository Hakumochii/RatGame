using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneData
    {
        public PlayableDirector director;

        public bool useCutsceneRat;
    }

    public CutsceneData[] cutscenes;

    public RatBehaviour playerMovement;

    public GameObject player;

    public GameObject cutsceneRat;

    public void PlayCutscene(int index)
    {
        if (index >= 0 && index < cutscenes.Length)
        {
            CutsceneData cutscene = cutscenes[index];

            // Disable gameplay movement
            playerMovement.enabled = false;

            // Optional cinematic rat setup
            if (cutscene.useCutsceneRat)
            {
                // Match position/rotation BEFORE hiding player
                cutsceneRat.transform.position = player.transform.position;
                cutsceneRat.transform.rotation = player.transform.rotation;

                // Hide gameplay player
                player.SetActive(false);

                // Show cinematic rat
                cutsceneRat.SetActive(true);
            }

            cutscene.director.stopped += OnCutsceneFinished;

            cutscene.director.Play();
        }
    }

    void OnCutsceneFinished(PlayableDirector director)
    {
        foreach (CutsceneData cutscene in cutscenes)
        {
            if (cutscene.director == director)
            {
                if (cutscene.useCutsceneRat)
                {
                    // Hide cinematic rat
                    cutsceneRat.SetActive(false);

                    // Show gameplay player again
                    player.SetActive(true);
                }

                break;
            }
        }

        // Re-enable gameplay movement
        playerMovement.enabled = true;

        director.stopped -= OnCutsceneFinished;
    }
}