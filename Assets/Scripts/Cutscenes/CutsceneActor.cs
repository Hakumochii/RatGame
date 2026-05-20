using UnityEngine;

public class CutsceneActor : MonoBehaviour
{
    public Renderer[] renderersToToggle;

    public void ShowActor()
    {
        foreach (Renderer r in renderersToToggle)
        {
            r.enabled = true;
        }
    }

    public void HideActor()
    {
        foreach (Renderer r in renderersToToggle)
        {
            r.enabled = false;
        }
    }
}
