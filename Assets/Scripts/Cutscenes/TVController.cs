using UnityEngine;
using UnityEngine.Video;

public class TVController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    public void TurnOnTV()
    {
        videoPlayer.Play();
    }
}