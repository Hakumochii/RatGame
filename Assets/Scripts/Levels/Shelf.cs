using UnityEngine;

public class Shelf : Level
{
    public override void PrepareLevel()
    {
        _cutsceneController.PlayCutscene(0);
        //make rat okay with going to shelf and able to drag
    }
}
