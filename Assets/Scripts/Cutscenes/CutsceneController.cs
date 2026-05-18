using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [Header("Cutscenes")]
    public PlayableDirector[] cutscenes;

    [Header("Player")]
    public RatBehaviour playerMovement;

    public void PlayCutscene(int index)
    {
        if (index >= 0 && index < cutscenes.Length)
        {
            // Disable player movement
            playerMovement.enabled = false;

            // Get selected cutscene
            PlayableDirector director = cutscenes[index];

            // Listen for when cutscene ends
            director.stopped += OnCutsceneFinished;

            // Play cutscene
            director.Play();
        }
    }

    private void OnCutsceneFinished(PlayableDirector director)
    {
        // Enable movement again
        playerMovement.enabled = true;

        // Remove listener
        director.stopped -= OnCutsceneFinished;
    }
}
