using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    // Singleton
    private static SoundManager _instance;
    public static SoundManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SoundManager>();

                if (_instance == null)
                {
                    GameObject obj = new GameObject(nameof(SoundManager));
                    _instance = obj.AddComponent<SoundManager>();
                }
            }
            return _instance;
        }
    }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource continuouslySource; 

    [Header("Music")]
    public AudioClip catBurglarMusic;
    public AudioClip chaseMusic;
    public AudioClip movingInFramesMusic;
    public AudioClip sneakySnakyMusic;

    [Header("SFX")]
    public AudioClip ratRunSFX;
    public AudioClip ratPushPullSFX;
    public AudioClip ratClimbSFX;
    public AudioClip ratFallingLongSFX;
    public AudioClip ratFallingShortSFX;
    public AudioClip ratJumpSFX;
    public AudioClip ratLandingSFX;
    public AudioClip ratBurnedSFX;

    private void Awake()
    {
        if (_instance == null || _instance.gameObject == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    // ── Music ─────────────────────────────────────────────────────────────────
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource.clip == clip) return; // already playing

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
        musicSource.clip = null;
    }

    public void FadeMusic(float targetVolume, float duration)
    {
        StartCoroutine(FadeCoroutine(musicSource, targetVolume, duration));
    }

    // ── SFX ───────────────────────────────────────────────────────────────────
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayContinuosly(AudioClip clip)
    {
        if (continuouslySource.isPlaying && continuouslySource.clip == clip) return;
        continuouslySource.clip = clip;
        continuouslySource.loop = true;
        continuouslySource.Play();
    }

    public void StopContinuosly()
    {
        continuouslySource.Stop();
    }

    // ── Utility ───────────────────────────────────────────────────────────────
    private IEnumerator FadeCoroutine(AudioSource source, float targetVolume, float duration)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        source.volume = targetVolume;

        // If faded to zero, stop the source to save resources
        if (targetVolume == 0f)
        {
            source.Stop();
        }
    }

    public void CrossFadeMusic(AudioClip newClip, float duration)
    {
        StartCoroutine(CrossFadeCoroutine(newClip, duration));
    }

    private IEnumerator CrossFadeCoroutine(AudioClip newClip, float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        // Fade out current clip
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        // Swap clip
        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.Play();

        // Fade in new clip
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, startVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = startVolume;
    }
}
