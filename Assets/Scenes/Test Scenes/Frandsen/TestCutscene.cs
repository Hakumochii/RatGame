using UnityEngine;
using System.Collections;

public class CutsceneTest : MonoBehaviour
{
    public CutsceneController cutsceneController;

    void Start()
    {
        StartCoroutine(StartCutsceneAfterDelay());
    }

    IEnumerator StartCutsceneAfterDelay()
    {
        // Wait 3 seconds
        yield return new WaitForSeconds(3f);

        // Play first cutscene
        cutsceneController.PlayCutscene(1);
    }
}