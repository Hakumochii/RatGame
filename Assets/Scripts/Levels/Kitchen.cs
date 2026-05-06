using UnityEngine;

public class Kitchen : Level
{
    public override void PrepareLevel()
    {
        _gameManager.PlayCutscene(_gameManager.kitchenIntro);
        _gameManager.cat.SetActive(true);
        _gameManager.catOnFloor = true;
    }
}
