using UnityEngine;

public class CutsceneActor : MonoBehaviour
{
    public Renderer[] renderersToToggle;

    public void ShowActor()
    {
        Debug.Log("SHOW ACTOR CALLED");

        foreach (Renderer r in renderersToToggle)
        {
            Debug.Log("Enabling renderer: " + r.name);

            r.enabled = true;
        }
    }

    public void HideActor()
    {
        Debug.Log("HIDE ACTOR CALLED");

        foreach (Renderer r in renderersToToggle)
        {
            Debug.Log("Disabling renderer: " + r.name);

            r.enabled = false;
        }
    }
}
