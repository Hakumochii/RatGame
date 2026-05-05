using UnityEngine;

public class Kitchen : Level
{
    public override void PrepareLevel()
    {
        _gameManager.PlayCutscene(_gameManager.kitchenIntro);
        ToggleRatBehaviour();
    }

    public override void ToggleRatBehaviour()
    {
        //make rat okay with going to kitchen 
        //make floor deadly
    }
}
