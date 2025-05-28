using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Music Settings")]
    public List<AudioClip> musicTracks;
    public AudioSource musicSource; // Assign your primary music AudioSource in Inspector
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    public float trackSwitchDelay = 1f;
    public float skipFadeDuration = 0.5f;
    public float fadeInDuration = 0.5f;

    [Header("Boss Music Settings")]
    public AudioClip bossMusicClip; // Assign your boss music clip in the Inspector
    private AudioSource bossMusicSource;

    [Header("Input Settings")]
    public string skipButton = "JoystickButton9"; // This is your "R3" (or change to desired input)
    public float doubleClickTime = 0.3f;

    private AudioSource secondaryMusicSource; // Used for crossfading
    private List<AudioClip> playlist;
    private int currentTrackIndex = 0;
    private Coroutine musicRoutine;
    private bool isSkipping = false;

    // --- Pause States ---
    private bool isGameGloballyPaused = false;     // True if GameManager paused the game (e.g., Escape menu)
    private bool isMainMusicPausedForBoss = false; // True if main music is paused because a boss is active
    private bool isMainMusicUserPaused = false;    // True if user paused main music via R3 single press

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
        // Ensure musicSource is assigned in Inspector, if not, try to add it.
        if (musicSource == null)
        {
            Debug.LogWarning("AudioManager: MusicSource not assigned in Inspector. Adding one.");
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        musicSource.loop = false;
        musicSource.volume = 0f;
        musicSource.playOnAwake = false;

        secondaryMusicSource = gameObject.AddComponent<AudioSource>();
        secondaryMusicSource.loop = false;
        secondaryMusicSource.volume = 0f;
        secondaryMusicSource.playOnAwake = false;

        bossMusicSource = gameObject.AddComponent<AudioSource>();
        bossMusicSource.loop = true;
        bossMusicSource.playOnAwake = false;
        if (bossMusicClip != null)
        {
            bossMusicSource.clip = bossMusicClip;
        }
        bossMusicSource.volume = musicVolume;

        CreatePlaylist();
        StartMusic();
    }

    private void CreatePlaylist()
    {
        playlist = new List<AudioClip>(musicTracks);
        if (playlist.Count > 0)
        {
            ShufflePlaylist();
            currentTrackIndex = 0;
        }
        else
        {
            Debug.LogWarning("AudioManager: No music tracks assigned to the playlist.");
        }
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

        if (playlist.Count > 0 && currentTrackIndex < playlist.Count && playlist[currentTrackIndex] != null)
        {
            musicRoutine = StartCoroutine(MusicPlaybackRoutine());
        }
        else if (playlist.Count > 0)
        {
             Debug.LogWarning("AudioManager: Playlist has tracks but current track is invalid or null. Can't start music playback.");
        }
    }

    private IEnumerator MusicPlaybackRoutine()
    {
        while (true)
        {
            if (playlist.Count == 0 || currentTrackIndex >= playlist.Count || playlist[currentTrackIndex] == null)
            {
                yield break; // Stop if playlist is invalid
            }

            musicSource.clip = playlist[currentTrackIndex];
            musicSource.volume = 0f;

            // Wait for all pause conditions to clear before starting
            while (ShouldMainMusicBePaused())
            {
                yield return null;
            }
            
            if (musicSource.clip != null) 
            {
                musicSource.Play();
            }
            else 
            { 
                yield break; 
            }

            // Fade in
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                while (ShouldMainMusicBePaused()) 
                {
                    yield return null;
                }
                musicSource.volume = Mathf.Lerp(0f, musicVolume, timer / fadeInDuration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            musicSource.volume = musicVolume;

            // Play the main portion of the track
            float clipLength = musicSource.clip != null ? musicSource.clip.length : 0f;
            float remainingTime = clipLength - fadeInDuration;
            if (remainingTime < 0) remainingTime = 0;
            float elapsed = 0f;

            while (elapsed < remainingTime && !isSkipping)
            {
                while (ShouldMainMusicBePaused() && !isSkipping) 
                {
                    yield return null;
                }
                
                if (!ShouldMainMusicBePaused()) // Only increment if not paused
                {
                    elapsed += Time.unscaledDeltaTime;
                }
                yield return null;
            }

            if (!isSkipping) // Natural end of track
            {
                // Fade out
                float startVolume = musicSource.volume;
                timer = 0f;
                while (timer < skipFadeDuration)
                {
                    while (ShouldMainMusicBePaused()) 
                    {
                        yield return null;
                    }
                    musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / skipFadeDuration);
                    timer += Time.unscaledDeltaTime;
                    yield return null;
                }

                musicSource.Stop();
                musicSource.volume = 0f;
                AdvanceTrack();

                // Delay between tracks
                float delayTimer = 0f;
                while(delayTimer < trackSwitchDelay)
                {
                    while (ShouldMainMusicBePaused()) 
                    {
                        yield return null;
                    }
                    delayTimer += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            // If isSkipping, CrossFadeSkipRoutine handles the transition
        }
    }

    private bool ShouldMainMusicBePaused()
    {
        return isGameGloballyPaused || isMainMusicPausedForBoss || isMainMusicUserPaused;
    }
    
    private void Update()
    {
        // Only process music controls if game is not globally paused (Escape menu)
        // This prevents music controls from interfering with menu navigation
        if (!isGameGloballyPaused)
        {
            // R3 or E key for music control
            if (Input.GetButtonDown(skipButton) || Input.GetKeyDown(KeyCode.E))
            {
                clickCount++;
                if (clickResetCoroutine != null) StopCoroutine(clickResetCoroutine);
                clickResetCoroutine = StartCoroutine(ResetClickCount());

                if (clickCount == 1)
                {
                    lastClickTime = Time.unscaledTime;
                }
                else if (clickCount >= 2 && (Time.unscaledTime - lastClickTime <= doubleClickTime))
                {
                    if (clickResetCoroutine != null) StopCoroutine(clickResetCoroutine);
                    SkipTrack(); // Double press skips main playlist track
                    clickCount = 0;
                }
            }
        }
    }

    private IEnumerator ResetClickCount()
    {
        // Wait for the double-click window to expire
        yield return new WaitForSecondsRealtime(doubleClickTime);

        if (clickCount == 1) // If, after the delay, it's still just one click
        {
            ToggleMainMusicUserPause(); // Single press toggles pause for main playlist
        }
        clickCount = 0;
    }

    private void ToggleMainMusicUserPause()
    {
        isMainMusicUserPaused = !isMainMusicUserPaused;
        Debug.Log($"AudioManager: User pause toggled. isMainMusicUserPaused = {isMainMusicUserPaused}");
        
        if (isMainMusicUserPaused)
        {
            if (musicSource.isPlaying) musicSource.Pause();
            if (secondaryMusicSource.isPlaying) secondaryMusicSource.Pause();
        }
        else
        {
            // Only unpause if no other "master" pause conditions are met
            if (!isGameGloballyPaused && !isMainMusicPausedForBoss)
            {
                if (!musicSource.isPlaying && musicSource.time > 0 && musicSource.clip != null) 
                {
                    musicSource.UnPause();
                }
                if (secondaryMusicSource.clip != null && !secondaryMusicSource.isPlaying && secondaryMusicSource.time > 0) 
                {
                    secondaryMusicSource.UnPause();
                }
            }
        }
    }

    public void SkipTrack()
    {
        // Can only skip main playlist tracks.
        // Prevent skipping if music is paused for boss, or if user explicitly paused it
        if (isSkipping || playlist.Count <= 1 || isMainMusicPausedForBoss || isMainMusicUserPaused) 
        {
            Debug.Log($"AudioManager: Skip blocked. isSkipping={isSkipping}, playlist.Count={playlist.Count}, isMainMusicPausedForBoss={isMainMusicPausedForBoss}, isMainMusicUserPaused={isMainMusicUserPaused}");
            return;
        }
        
        Debug.Log("AudioManager: Skipping track");
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        StartCoroutine(CrossFadeSkipRoutine());
    }

    private IEnumerator CrossFadeSkipRoutine()
    {
        isSkipping = true;

        AdvanceTrack();
        if (playlist.Count == 0 || currentTrackIndex >= playlist.Count || playlist[currentTrackIndex] == null) 
        { 
            isSkipping = false; 
            StartMusic();
            yield break; 
        }

        secondaryMusicSource.clip = playlist[currentTrackIndex];
        secondaryMusicSource.volume = 0f;
        if (secondaryMusicSource.clip != null) 
        {
            secondaryMusicSource.Play();
        }
        else 
        { 
            isSkipping = false; 
            StartMusic(); 
            yield break;
        }

        float timer = 0f;
        float startVolumeMusicSource = musicSource.isPlaying ? musicSource.volume : 0f;

        while (timer < skipFadeDuration)
        {
            float t = timer / skipFadeDuration;
            // Only adjust volume if not globally paused
            if (!isGameGloballyPaused) 
            {
                if (musicSource.isPlaying) musicSource.volume = Mathf.Lerp(startVolumeMusicSource, 0f, t);
                secondaryMusicSource.volume = Mathf.Lerp(0f, musicVolume, t);
            }
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (musicSource.isPlaying) musicSource.Stop();
        musicSource.volume = 0f;
        
        // Swap the audio sources
        AudioSource tempSource = musicSource;
        musicSource = secondaryMusicSource;
        secondaryMusicSource = tempSource;
        secondaryMusicSource.Stop(); 
        secondaryMusicSource.volume = 0f;

        isSkipping = false;

        // Restart the main routine for the new track
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        if (!ShouldMainMusicBePaused())
        {
            currentTrackIndex = playlist.IndexOf(musicSource.clip);
            if (currentTrackIndex != -1) 
            {
                musicRoutine = StartCoroutine(MusicPlaybackRoutine_ResumeFromCrossfade());
            }
            else 
            {
                StartMusic();
            }
        }
    }
    
    private IEnumerator MusicPlaybackRoutine_ResumeFromCrossfade()
    {
        if (musicSource == null || musicSource.clip == null) yield break;

        while (ShouldMainMusicBePaused()) yield return null;

        float clipLength = musicSource.clip.length;
        float elapsed = musicSource.time;
        float remainingTime = clipLength - elapsed;

        while (elapsed < clipLength && !isSkipping)
        {
            while (ShouldMainMusicBePaused() && !isSkipping) 
            {
                yield return null;
            }
            
            if (!ShouldMainMusicBePaused())
            {
                elapsed += Time.unscaledDeltaTime; 
            }
            
            if (!musicSource.isPlaying && !ShouldMainMusicBePaused())
            {
                if (musicSource.time < clipLength - 0.1f) 
                {
                    musicSource.Play();
                }
                else 
                {
                    break;
                }
            }
            yield return null;
        }

        if (!isSkipping) // Natural end of track
        {
            float startVolume = musicSource.volume;
            float timer = 0f;
            while (timer < skipFadeDuration)
            {
                while (ShouldMainMusicBePaused()) 
                {
                    yield return null;
                }
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / skipFadeDuration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            musicSource.Stop();
            musicSource.volume = 0f;
            AdvanceTrack();

            float delayTimer = 0f;
            while(delayTimer < trackSwitchDelay)
            {
                while (ShouldMainMusicBePaused()) 
                {
                    yield return null;
                }
                delayTimer += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        if (!isSkipping) StartMusic();
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

    // Called by GameManager when game is globally paused (e.g. Escape menu)
    public void PauseGameAudio()
    {
        Debug.Log("AudioManager: PauseGameAudio called");
        isGameGloballyPaused = true;
        if (musicSource.isPlaying) musicSource.Pause();
        if (secondaryMusicSource.isPlaying) secondaryMusicSource.Pause();
        if (bossMusicSource != null && bossMusicSource.isPlaying) bossMusicSource.Pause();
    }

    // Called by GameManager when game is globally resumed
    public void ResumeGameAudio()
    {
        Debug.Log("AudioManager: ResumeGameAudio called");
        isGameGloballyPaused = false;

        if (!isMainMusicPausedForBoss && !isMainMusicUserPaused)
        {
            if (!musicSource.isPlaying && musicSource.time > 0 && musicSource.clip != null) 
            {
                musicSource.UnPause();
            }
            if (secondaryMusicSource.clip != null && !secondaryMusicSource.isPlaying && secondaryMusicSource.time > 0) 
            {
                secondaryMusicSource.UnPause();
            }
        }
        
        if (bossMusicSource != null && !bossMusicSource.isPlaying && bossMusicSource.time > 0 && bossMusicSource.clip != null) 
        {
            bossMusicSource.UnPause();
        }
    }

    // --- Boss Music Specific Methods ---
    public void PlayBossMusic()
    {
        Debug.Log("AudioManager: PlayBossMusic called");
        PauseMainTrackForBoss(); 

        if (bossMusicClip != null && bossMusicSource != null)
        {
            bossMusicSource.clip = bossMusicClip;
            bossMusicSource.volume = musicVolume;
            if (!isGameGloballyPaused) 
            {
                bossMusicSource.Play();
            }
            else
            {
                 bossMusicSource.time = 0;
            }
        }
    }

    public void StopBossMusic()
    {
        Debug.Log("AudioManager: StopBossMusic called");
        if (bossMusicSource != null && bossMusicSource.isPlaying)
        {
            bossMusicSource.Stop();
        }
    }

    public void PauseMainTrackForBoss()
    {
        Debug.Log("AudioManager: PauseMainTrackForBoss called");
        isMainMusicPausedForBoss = true;
        if (musicSource.isPlaying) musicSource.Pause();
        if (secondaryMusicSource.isPlaying) secondaryMusicSource.Pause();
    }

    public void ResumeMainTrackAfterBoss()
    {
        Debug.Log("AudioManager: ResumeMainTrackAfterBoss called");
        isMainMusicPausedForBoss = false;
        if (!isGameGloballyPaused && !isMainMusicUserPaused)
        {
            if (!musicSource.isPlaying && musicSource.time > 0 && musicSource.clip != null) 
            {
                 musicSource.UnPause();
            } 
            else if ((!musicSource.isPlaying || musicSource.clip == null) && playlist.Count > 0)
            {
                 if(musicRoutine != null) StopCoroutine(musicRoutine);
                 StartMusic();
            }
        }
    }

    // --- General Music Control ---
    public void PlayMainPlaylistMusic() 
    {
        if (isMainMusicPausedForBoss || isMainMusicUserPaused) return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);
        CreatePlaylist();
        StartMusic();
    }

    public void StopAllMusicImmediately() 
    {
        Debug.Log("AudioManager: StopAllMusicImmediately called");
        if (musicRoutine != null) { StopCoroutine(musicRoutine); musicRoutine = null; }
        if (musicSource != null && musicSource.isPlaying) musicSource.Stop();
        if (secondaryMusicSource != null && secondaryMusicSource.isPlaying) secondaryMusicSource.Stop();
        if (bossMusicSource != null && bossMusicSource.isPlaying) bossMusicSource.Stop();
        
        if (musicSource != null) musicSource.volume = 0f;
        if (secondaryMusicSource != null) secondaryMusicSource.volume = 0f;
        if (bossMusicSource != null) bossMusicSource.volume = 0f;
        
        // Reset all pause states when stopping all music
        isMainMusicUserPaused = false;
        isMainMusicPausedForBoss = false;
        // Don't reset isGameGloballyPaused as that should only be controlled by GameManager
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (!ShouldMainMusicBePaused() && musicSource.isPlaying)
        {
            musicSource.volume = musicVolume;
        }
        if (bossMusicSource != null && bossMusicSource.isPlaying && !isGameGloballyPaused)
        {
            bossMusicSource.volume = musicVolume;
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        if (clickResetCoroutine != null) StopCoroutine(clickResetCoroutine);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"AudioManager: Scene loaded - {scene.name}");
        if (scene.name == "GameScene")
        {
            isMainMusicPausedForBoss = false; 
            isMainMusicUserPaused = false;
            PlayMainPlaylistMusic(); 
        }
    }
}