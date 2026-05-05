using UnityEngine;

public class Tutorial : Level
{
    public override void PrepareLevel()
    {
        _gameManager.PlayCutscene(_gamemanager.intro);
        ToggleRatBehaviour();
    }

    public override void ToggleRatBehaviour()
    {
        //make rat unable to drag + not want to go to anythin ecept computer
    }

}
