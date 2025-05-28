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

    [Header("Boss Music Settings")]
    public AudioClip bossMusicClip;
    private AudioSource bossMusicSource;

    [Header("Input Settings")]
    public string skipButton = "JoystickButton9"; // This is your "R3"
    public float doubleClickTime = 0.3f;

    private AudioSource secondaryMusicSource;
    private List<AudioClip> playlist;
    private int currentTrackIndex = 0;
    private Coroutine musicRoutine;
    private bool isSkipping = false;

    // Pause States:
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

        if (playlist.Count > 0 && playlist[0] != null) // Ensure playlist is valid
        {
            musicRoutine = StartCoroutine(MusicPlaybackRoutine());
        }
    }

    private IEnumerator MusicPlaybackRoutine()
    {
        while (true)
        {
            if (playlist.Count == 0 || playlist[currentTrackIndex] == null)
            {
                // Debug.LogWarning("Playlist empty or current track null, stopping music routine.");
                yield break;
            }

            musicSource.clip = playlist[currentTrackIndex];
            musicSource.volume = 0f;

            while (ShouldMainMusicBePaused())
            {
                yield return null;
            }
            if (musicSource.clip != null) musicSource.Play(); else yield break;


            float timer = 0f;
            while (timer < fadeInDuration)
            {
                while (ShouldMainMusicBePaused()) yield return null;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, timer / fadeInDuration);
                timer += Time.unscaledDeltaTime; // Use unscaledDeltaTime for UI/audio fades independent of Time.timeScale
                yield return null;
            }
            musicSource.volume = musicVolume;

            float remainingTime = (musicSource.clip != null ? musicSource.clip.length : 0) - fadeInDuration;
            if (remainingTime < 0) remainingTime = 0;
            float elapsed = 0f;

            while (elapsed < remainingTime && !isSkipping)
            {
                while (ShouldMainMusicBePaused() && !isSkipping) yield return null;
                
                if (!ShouldMainMusicBePaused())
                {
                    elapsed += Time.unscaledDeltaTime;
                }
                yield return null;
            }

            if (!isSkipping) // Natural end of track
            {
                float startVolume = musicSource.volume;
                timer = 0f;
                while (timer < skipFadeDuration)
                {
                    while (ShouldMainMusicBePaused()) yield return null;
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
                    while (ShouldMainMusicBePaused()) yield return null;
                    delayTimer += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            // If isSkipping, CrossFadeSkipRoutine handles transition and restarts this routine.
        }
    }

    private bool ShouldMainMusicBePaused()
    {
        return isGameGloballyPaused || isMainMusicPausedForBoss || isMainMusicUserPaused;
    }
    
    private void Update()
    {
        if (Input.GetButtonDown(skipButton) || Input.GetKeyDown(KeyCode.E)) // R3 or E key
        {
            clickCount++;
            if (clickResetCoroutine != null) StopCoroutine(clickResetCoroutine); // Stop previous reset if rapid clicks
            clickResetCoroutine = StartCoroutine(ResetClickCount());

            if (clickCount == 1)
            {
                lastClickTime = Time.unscaledTime; // Use unscaledTime for UI interactions
            }
            else if (clickCount >= 2 && Time.unscaledTime - lastClickTime <= doubleClickTime)
            {
                if (clickResetCoroutine != null) StopCoroutine(clickResetCoroutine);
                SkipTrack(); // Double press skips main playlist track
                clickCount = 0;
            }
        }
    }

    private IEnumerator ResetClickCount()
    {
        // Wait for the double-click window to expire
        yield return new WaitForSecondsRealtime(doubleClickTime); // Use WaitForSecondsRealtime for UI timing

        if (clickCount == 1) // If only one click occurred
        {
            ToggleMainMusicUserPause(); // Single press toggles pause for main playlist
        }
        clickCount = 0; // Reset for next interaction
    }

    private void ToggleMainMusicUserPause()
    {
        isMainMusicUserPaused = !isMainMusicUserPaused;
        if (isMainMusicUserPaused)
        {
            if (musicSource.isPlaying) musicSource.Pause();
            if (secondaryMusicSource.isPlaying) secondaryMusicSource.Pause();
            // Debug.Log("Main music user-paused (R3).");
        }
        else
        {
            // Only unpause if no other pause conditions are met
            if (!isGameGloballyPaused && !isMainMusicPausedForBoss)
            {
                if (!musicSource.isPlaying && musicSource.time > 0) musicSource.UnPause();
                if (secondaryMusicSource.clip != null && !secondaryMusicSource.isPlaying && secondaryMusicSource.time > 0) secondaryMusicSource.UnPause();
            }
            // Debug.Log("Main music user-resumed (R3).");
        }
    }

    public void SkipTrack()
    {
        // Can only skip main playlist tracks, and only if it's not paused for boss or by user in a way that implies "don't touch"
        if (isSkipping || playlist.Count == 0 || isMainMusicPausedForBoss || isMainMusicUserPaused) return;
        StartCoroutine(CrossFadeSkipRoutine());
    }

    private IEnumerator CrossFadeSkipRoutine()
    {
        isSkipping = true;

        AdvanceTrack();
        if (playlist.Count == 0 || playlist[currentTrackIndex] == null) { isSkipping = false; yield break; }

        secondaryMusicSource.clip = playlist[currentTrackIndex];
        secondaryMusicSource.volume = 0f;
        if (secondaryMusicSource.clip != null) secondaryMusicSource.Play(); else {isSkipping = false; yield break;}


        float timer = 0f;
        float startVolume = musicSource.volume;

        while (timer < skipFadeDuration)
        {
            // Crossfade should still happen even if game is globally paused, uses unscaledDeltaTime
            // However, if the main music is supposed to be fully silent (e.g. for boss), this needs care.
            // For now, let's assume crossfade always tries to complete visually.
            // If isGameGloballyPaused, sources might be paused externally by PauseGameAudio.
            float t = timer / skipFadeDuration;
            if(!isGameGloballyPaused) musicSource.volume = Mathf.Lerp(startVolume, 0f, t); // Only change if not globally paused
            if(!isGameGloballyPaused) secondaryMusicSource.volume = Mathf.Lerp(0f, musicVolume, t);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if(!isGameGloballyPaused) musicSource.Stop();
        
        AudioSource tempSource = musicSource;
        musicSource = secondaryMusicSource;
        secondaryMusicSource = tempSource;
        secondaryMusicSource.Stop(); // Ensure old secondary (now primary) is stopped.

        isSkipping = false;

        if (musicRoutine != null) StopCoroutine(musicRoutine);
        // Restart main music routine only if not paused for boss or user
        if (!isMainMusicPausedForBoss && !isMainMusicUserPaused)
        {
            musicRoutine = StartCoroutine(MusicPlaybackRoutine());
        }
    }

    private void AdvanceTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= playlist.Count)
        {
            StartCoroutine(ShuffleNextFrame()); // Shuffle for next cycle
            currentTrackIndex = 0;
        }
    }

    private IEnumerator ShuffleNextFrame()
    {
        yield return null; 
        ShufflePlaylist();
    }

    // Called by GameManager when game is actually paused (e.g. Escape menu)
    public void PauseGameAudio()
    {
        isGameGloballyPaused = true;
        if (musicSource.isPlaying) musicSource.Pause();
        if (secondaryMusicSource.isPlaying) secondaryMusicSource.Pause();
        if (bossMusicSource != null && bossMusicSource.isPlaying) bossMusicSource.Pause(); // Boss music IS paused by global game pause
    }

    // Called by GameManager when game is actually resumed
    public void ResumeGameAudio()
    {
        isGameGloballyPaused = false;

        // Main music only resumes if NOT paused for boss AND NOT user-paused by R3
        if (!isMainMusicPausedForBoss && !isMainMusicUserPaused)
        {
            if (!musicSource.isPlaying && musicSource.time > 0) musicSource.UnPause();
            if (secondaryMusicSource.clip != null && !secondaryMusicSource.isPlaying && secondaryMusicSource.time > 0) secondaryMusicSource.UnPause();
        }
        
        // Boss music resumes if it was playing (it's not affected by R3 user pause or boss-specific pause for main track)
        if (bossMusicSource != null && !bossMusicSource.isPlaying && bossMusicSource.time > 0 && bossMusicSource.clip != null) bossMusicSource.UnPause();
    }

    // --- Boss Music Specific Methods ---
    public void PlayBossMusic()
    {
        PauseMainTrackForBoss(); // Pause the main music track(s)

        if (bossMusicClip != null && bossMusicSource != null)
        {
            bossMusicSource.clip = bossMusicClip;
            bossMusicSource.volume = musicVolume;
            if (!isGameGloballyPaused) // Only play if game is not globally paused
            {
                bossMusicSource.Play();
            }
            else
            {
                 bossMusicSource.time = 0; // Ensure it starts from beginning when unpaused by ResumeGameAudio
            }
        }
    }

    public void StopBossMusic()
    {
        if (bossMusicSource != null && bossMusicSource.isPlaying)
        {
            bossMusicSource.Stop();
        }
    }

    public void PauseMainTrackForBoss()
    {
        isMainMusicPausedForBoss = true;
        if (musicSource.isPlaying) musicSource.Pause();
        if (secondaryMusicSource.isPlaying) secondaryMusicSource.Pause();
    }

    public void ResumeMainTrackAfterBoss()
    {
        isMainMusicPausedForBoss = false;
        // Only unpause main music if not globally paused AND not user-paused by R3
        if (!isGameGloballyPaused && !isMainMusicUserPaused)
        {
            if (!musicSource.isPlaying && musicSource.time > 0 && musicSource.clip != null) 
            {
                 musicSource.UnPause();
            } 
            // If music was stopped and routine needs restart
            else if ((!musicSource.isPlaying || musicSource.clip == null) && musicRoutine == null && playlist.Count > 0)
            {
                 StartMusic();
            }
        }
    }

    // --- General Music Control Methods (used by other systems if needed) ---
    public void PlayMainPlaylistMusic() // Call this on scene load or when explicitly starting main music
    {
        if (isMainMusicPausedForBoss || isMainMusicUserPaused) return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);
        ShufflePlaylist();
        currentTrackIndex = 0;
        StartMusic();
    }

    public void StopAllMusicImmediately() // For critical stops
    {
        if (musicRoutine != null) StopCoroutine(musicRoutine);
        if (musicSource.isPlaying) musicSource.Stop();
        if (secondaryMusicSource.isPlaying) secondaryMusicSource.Stop();
        if (bossMusicSource != null && bossMusicSource.isPlaying) bossMusicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (!ShouldMainMusicBePaused() && musicSource.isPlaying)
        {
            musicSource.volume = musicVolume;
        }
        if (bossMusicSource != null && bossMusicSource.isPlaying && !isGameGloballyPaused) // Boss music volume only affected by global pause
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
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Assuming "GameScene" is where gameplay and this AudioManager are primarily active.
        // Adjust scene name as necessary.
        if (scene.name == "GameScene") 
        {
            // Reset states that shouldn't persist across main game scene loads
            isMainMusicPausedForBoss = false; 
            isMainMusicUserPaused = false;
            // isGameGloballyPaused is managed by GameManager, typically false on new scene load unless designed otherwise

            PlayMainPlaylistMusic(); // Start the main playlist
        }
        else
        {
            // If loading a menu or other non-gameplay scene, you might want to stop all music
            // or play a specific menu track (which would be new logic).
            // StopAllMusicImmediately(); // Example: Stop music if not the game scene
        }
    }
}