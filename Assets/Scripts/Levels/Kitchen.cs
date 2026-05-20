using UnityEngine;

public class Kitchen : Level
{
    public override void PrepareLevel()
    {
        _cutsceneController.PlayCutscene(1);

    }
}
