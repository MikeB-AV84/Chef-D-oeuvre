using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Settings")]
    public List<AudioClip> musicTracks;
    public AudioSource musicSource;
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    public float trackSwitchDelay = 1f;
    public float skipFadeDuration = 0.5f;
    public float fadeInDuration = 0.5f;

    [Header("Input Settings")]
    public string skipButton = "JoystickButton9";
    public float doubleClickTime = 0.3f;

    private AudioSource secondaryMusicSource;
    private List<AudioClip> playlist;
    private int currentTrackIndex = 0;
    private Coroutine musicRoutine;
    private bool isSkipping = false;
    private bool isPaused = false;
    private float lastClickTime = 0f;
    private int clickCount = 0;
    private Coroutine clickResetCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        musicSource.loop = false;
        musicSource.volume = 0f;

        secondaryMusicSource = gameObject.AddComponent<AudioSource>();
        secondaryMusicSource.loop = false;
        secondaryMusicSource.volume = 0f;

        CreatePlaylist();
        StartMusic();
    }

    private void CreatePlaylist()
    {
        playlist = new List<AudioClip>(musicTracks);
        ShufflePlaylist();
        currentTrackIndex = 0;
    }

    private void ShufflePlaylist()
    {
        System.Random rng = new System.Random();
        int n = playlist.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            AudioClip temp = playlist[k];
            playlist[k] = playlist[n];
            playlist[n] = temp;
        }
    }

    private void StartMusic()
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
        }

        if (playlist.Count > 0)
        {
            musicRoutine = StartCoroutine(MusicPlaybackRoutine());
        }
    }

    private IEnumerator MusicPlaybackRoutine()
    {
        while (true)
        {
            if (playlist.Count == 0) yield break;

            musicSource.clip = playlist[currentTrackIndex];
            musicSource.volume = 0f;
            musicSource.Play();

            float timer = 0f;
            while (timer < fadeInDuration)
            {
                musicSource.volume = Mathf.Lerp(0f, musicVolume, timer / fadeInDuration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            musicSource.volume = musicVolume;

            float remainingTime = musicSource.clip.length - fadeInDuration;
            float elapsed = 0f;

            while (elapsed < remainingTime && !isSkipping)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!isSkipping)
            {
                float startVolume = musicSource.volume;
                timer = 0f;
                while (timer < skipFadeDuration)
                {
                    musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / skipFadeDuration);
                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }

                musicSource.Stop();
                musicSource.volume = 0f;
                AdvanceTrack();

                if (playlist.Count > 0)
                {
                    musicSource.clip = playlist[currentTrackIndex];
                }

                yield return new WaitForSecondsRealtime(trackSwitchDelay);
            }
        }
    }

    private void Update()
    {
        if (Input.GetButtonDown(skipButton) || Input.GetKeyDown(KeyCode.E))
        {
            clickCount++;

            if (clickCount == 1)
            {
                lastClickTime = Time.time;
                clickResetCoroutine = StartCoroutine(ResetClickCount());
            }
            else if (clickCount == 2 && Time.time - lastClickTime <= doubleClickTime)
            {
                if (clickResetCoroutine != null) StopCoroutine(clickResetCoroutine);
                SkipTrack();
                clickCount = 0;
            }
        }
    }

    private IEnumerator ResetClickCount()
    {
        yield return new WaitForSeconds(doubleClickTime);

        if (clickCount == 1)
        {
            TogglePause();
        }

        clickCount = 0;
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            ResumeMusic();
            isPaused = false;
        }
        else
        {
            PauseMusic();
            isPaused = true;
        }
    }

    public void SkipTrack()
    {
        if (isSkipping || playlist.Count == 0) return;

        StartCoroutine(CrossFadeSkipRoutine());
    }

    private IEnumerator CrossFadeSkipRoutine()
    {
        isSkipping = true;

        AdvanceTrack();
        secondaryMusicSource.clip = playlist[currentTrackIndex];
        secondaryMusicSource.volume = 0f;
        secondaryMusicSource.Play();

        float timer = 0f;
        float startVolume = musicSource.volume;

        while (timer < skipFadeDuration)
        {
            float t = timer / skipFadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
            secondaryMusicSource.volume = Mathf.Lerp(0f, musicVolume, t);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        musicSource.Stop();
        AudioSource tempSource = musicSource;
        musicSource = secondaryMusicSource;
        secondaryMusicSource = tempSource;

        isSkipping = false;

        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(MusicPlaybackRoutine());
    }

    private void AdvanceTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= playlist.Count)
        {
            StartCoroutine(ShuffleNextFrame());
            currentTrackIndex = 0;
        }
    }

    private IEnumerator ShuffleNextFrame()
    {
        yield return null;
        ShufflePlaylist();
    }

    public void PlayRandomMusic()
    {
        if (musicRoutine != null) StopCoroutine(musicRoutine);

        StartCoroutine(ShuffleNextFrame());
        currentTrackIndex = 0;
        musicRoutine = StartCoroutine(MusicPlaybackRoutine());
    }

    public void StopMusic()
    {
        if (musicRoutine != null) StopCoroutine(musicRoutine);

        StartCoroutine(FadeOutAndStop());
    }

    private IEnumerator FadeOutAndStop()
    {
        float startVolume = musicSource.volume;
        float timer = 0f;

        while (timer < skipFadeDuration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / skipFadeDuration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = 0f;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (!isSkipping && musicSource.isPlaying)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void PauseMusic()
    {
        if (musicSource.isPlaying)
        {
            musicSource.Pause();
            if (secondaryMusicSource.isPlaying)
            {
                secondaryMusicSource.Pause();
            }
        }
    }

    public void ResumeMusic()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.UnPause();
            if (secondaryMusicSource.clip != null)
            {
                secondaryMusicSource.UnPause();
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "GameScene") // Replace with your game scene name
    {
        PlayRandomMusic();
    }
    }
}