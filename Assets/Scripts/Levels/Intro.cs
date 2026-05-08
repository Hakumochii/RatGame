using UnityEngine;

public class Intro : Level
{
    public override void PrepareLevel()
    {
        _gameManager.PlayCutscene(_gameManager.intro);
    }

}
