using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicPlaylistManager : MonoBehaviour
{
    [Header("Playlist")]
    public List<AudioClip> songs = new List<AudioClip>();

    [Header("Playback Settings")]
    [Tooltip("Start playing automatically when the scene loads.")]
    public bool playOnStart = true;

    [Tooltip("Loop the entire playlist when it ends.")]
    public bool loopPlaylist = true;

    [Tooltip("Shuffle the playlist order on start.")]
    public bool shuffle = false;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Crossfade (optional)")]
    [Tooltip("Duration in seconds to fade between songs. Set to 0 to disable.")]
    public float crossfadeDuration = 1f;

    // ── Private state ──────────────────────────────────────────────────
    private AudioSource _audioSource;
    private List<AudioClip> _queue = new List<AudioClip>();
    private int _currentIndex = 0;
    private bool _isStopped = false;
    private Coroutine _playbackCoroutine;

    // ── Public read-only info ──────────────────────────────────────────
    public int  CurrentIndex  => _currentIndex;
    public bool IsPlaying     => _audioSource != null && _audioSource.isPlaying;
    public AudioClip CurrentSong => (_queue.Count > 0) ? _queue[_currentIndex] : null;

    // ──────────────────────────────────────────────────────────────────
    #region Unity Messages

    private void Awake()
    {
        _audioSource        = GetComponent<AudioSource>();
        _audioSource.loop   = false;   // We handle looping ourselves
        _audioSource.volume = volume;
    }

    private void Start()
    {
        StartCoroutine(BackToTitle());

        if (songs == null || songs.Count == 0)
        {
            Debug.LogWarning("[MusicPlaylistManager] No songs assigned!");
            return;
        }

        BuildQueue();

        if (playOnStart)
            StartCoroutine(PlaylistRoutine());
    }

    private void OnValidate()
    {
        // Keep volume in sync while tweaking in Play Mode
        if (_audioSource != null)
            _audioSource.volume = volume;
    }

    #endregion

    // ──────────────────────────────────────────────────────────────────
    #region Public Controls

    /// <summary>Start / resume playback from the current index.</summary>
    public void Play()
    {
        _isStopped = false;
        if (_playbackCoroutine != null) StopCoroutine(_playbackCoroutine);
        _playbackCoroutine = StartCoroutine(PlaylistRoutine());
    }

    /// <summary>Stop playback entirely.</summary>
    public void Stop()
    {
        _isStopped = true;
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }
        _audioSource.Stop();
    }

    /// <summary>Skip to the next song.</summary>
    public void Next()
    {
        AdvanceIndex();
        Play();
    }

    /// <summary>Go back to the previous song.</summary>
    public void Previous()
    {
        _currentIndex = (_currentIndex - 1 + _queue.Count) % _queue.Count;
        Play();
    }

    /// <summary>Jump directly to a specific index in the (current) queue.</summary>
    public void PlayAt(int index)
    {
        if (index < 0 || index >= _queue.Count)
        {
            Debug.LogWarning($"[MusicPlaylistManager] Index {index} is out of range.");
            return;
        }
        _currentIndex = index;
        Play();
    }

    /// <summary>Rebuild and re-shuffle (or restore) the queue without restarting.</summary>
    public void RebuildQueue(bool startPlayback = false)
    {
        BuildQueue();
        if (startPlayback) Play();
    }

    #endregion

    // ──────────────────────────────────────────────────────────────────
    #region Private Helpers

    private void BuildQueue()
    {
        _queue = new List<AudioClip>(songs);

        if (shuffle)
            ShuffleQueue();

        _currentIndex = 0;
    }

    private void ShuffleQueue()
    {
        for (int i = _queue.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_queue[i], _queue[j]) = (_queue[j], _queue[i]);
        }
        Debug.Log("[MusicPlaylistManager] Playlist shuffled.");
    }

    private void AdvanceIndex()
    {
        _currentIndex++;
        if (_currentIndex >= _queue.Count)
        {
            if (loopPlaylist)
            {
                _currentIndex = 0;
                if (shuffle) ShuffleQueue(); // Re-shuffle each loop if desired
            }
            else
            {
                _isStopped = true;
            }
        }
    }

    private IEnumerator PlaylistRoutine()
    {
        while (!_isStopped && _queue.Count > 0)
        {
            AudioClip clip = _queue[_currentIndex];

            if (clip == null)
            {
                Debug.LogWarning($"[MusicPlaylistManager] Song at index {_currentIndex} is null — skipping.");
                AdvanceIndex();
                continue;
            }

            Debug.Log($"[MusicPlaylistManager] Now playing [{_currentIndex + 1}/{_queue.Count}]: {clip.name}");

            // ── Crossfade in ──────────────────────────────────────────
            if (crossfadeDuration > 0f)
            {
                _audioSource.clip   = clip;
                _audioSource.volume = 0f;
                _audioSource.Play();

                float t = 0f;
                while (t < crossfadeDuration)
                {
                    t                   += Time.deltaTime;
                    _audioSource.volume  = Mathf.Lerp(0f, volume, t / crossfadeDuration);
                    yield return null;
                }
                _audioSource.volume = volume;
            }
            else
            {
                _audioSource.clip   = clip;
                _audioSource.volume = volume;
                _audioSource.Play();
            }

            // ── Wait until near the end (leave room for crossfade out) ─
            float waitTime = clip.length - (crossfadeDuration > 0f ? crossfadeDuration : 0f);
            yield return new WaitForSeconds(Mathf.Max(0f, waitTime));

            // ── Crossfade out ─────────────────────────────────────────
            if (crossfadeDuration > 0f && _audioSource.isPlaying)
            {
                float t = 0f;
                float startVol = _audioSource.volume;
                while (t < crossfadeDuration)
                {
                    t                   += Time.deltaTime;
                    _audioSource.volume  = Mathf.Lerp(startVol, 0f, t / crossfadeDuration);
                    yield return null;
                }
                _audioSource.Stop();
            }

            AdvanceIndex();
        }

        if (!loopPlaylist)
            Debug.Log("[MusicPlaylistManager] Playlist finished.");
    }

    IEnumerator BackToTitle()
    {
        yield return new WaitForSeconds(205f);

        // Stop playlist so it doesn't interfere with the fade
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
        }

        // Fade out over 5 seconds
        float fadeDuration = 5f;
        float startVolume  = _audioSource.volume;
        float t            = 0f;

        while (t < fadeDuration)
        {
            t                   += Time.deltaTime;
            _audioSource.volume  = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        _audioSource.Stop();
        SceneManager.LoadScene(0);
    }

    #endregion
}
