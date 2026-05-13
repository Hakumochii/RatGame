using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    public PlayableDirector[] cutscenes;

    public void PlayCutscene(int index)
    {
        if (index >= 0 && index < cutscenes.Length)
        {
            cutscenes[index].Play();
        }
    }
    //Cutscenes can be calles with eg.: cutsceneController.PlayCutscene(0);
}
