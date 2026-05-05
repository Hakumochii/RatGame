using UnityEngine;

public class Shelf : Level
{
    public override void PrepareLevel()
    {
        _gameManager.PlayCutscene(_gamemanager.shelfIntro);
        ToggleRatBehaviour();
    }

    public override void ToggleRatBehaviour()
    {
        //make rat okay with going to shelf and able to drag
    }
}
