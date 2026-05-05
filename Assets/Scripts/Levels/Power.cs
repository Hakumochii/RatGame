using UnityEngine;

public class Power : Level
{
    public override void PrepareLevel()
    {
        _gameManager.PlayCutscene(_gameManager.powerIntro);
        ToggleRatBehaviour();
        //remove lamp toggle new lamp position
        //_gamemanager.lamp etc.
    }

    public override void ToggleRatBehaviour()
    {
        //make rat okay with going to kitchen 
        //make floor deadly
    }

}
