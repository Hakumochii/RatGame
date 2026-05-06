using UnityEngine;

public class Power : Level
{
    public override void PrepareLevel()
    {
        _gameManager.PlayCutscene(_gameManager.powerIntro);
        _gameManager.cat.transform.position = _gameManager.catPowerCord.transform.position;
        //remove lamp toggle new lamp position
        //_gamemanager.lamp etc.
    }

}
