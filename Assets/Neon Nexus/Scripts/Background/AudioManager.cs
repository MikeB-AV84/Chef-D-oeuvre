using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    // Add second audio source for smooth transitions
    private AudioSource secondaryMusicSource;
    private List<AudioClip> playlist;
    private int currentTrackIndex = 0;
    private Coroutine musicRoutine;
    private bool isSkipping = false;

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
        // Setup primary music source
        musicSource.loop = false;
        musicSource.volume = 0f; // Start at zero for fade-in
        
        // Create secondary audio source for crossfading
        secondaryMusicSource = gameObject.AddComponent<AudioSource>();
        secondaryMusicSource.loop = false;
        secondaryMusicSource.volume = 0f;
        
        // Pre-shuffle playlist at initialization
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
        // Use Fisher-Yates shuffle algorithm
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
            musicRoutine = null;
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
            if (playlist.Count == 0) 
            {
                yield break;
            }

            // Setup track
            musicSource.clip = playlist[currentTrackIndex];
            musicSource.volume = 0f;
            musicSource.Play();
            
            // Fade in
            float timer = 0f;
            while (timer < fadeInDuration)
            {
                musicSource.volume = Mathf.Lerp(0f, musicVolume, timer / fadeInDuration);
                timer += Time.unscaledDeltaTime;
                yield return null;
            }
            musicSource.volume = musicVolume;
            
            // Wait for track to complete or skip
            float remainingTime = musicSource.clip.length - fadeInDuration;
            float elapsed = 0f;
            
            while (elapsed < remainingTime && !isSkipping)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            // If track ended naturally (not skipped)
            if (!isSkipping)
            {
                // Fade out at end
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
                
                // Delay between tracks - but prepare next track during this time
                AdvanceTrack();
                
                // Pre-load next track during the delay
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
            SkipTrack();
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
        
        // Prepare the next track in the secondary source
        AdvanceTrack();
        secondaryMusicSource.clip = playlist[currentTrackIndex];
        secondaryMusicSource.volume = 0f;
        secondaryMusicSource.Play();
        
        // Cross-fade between the two sources
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
        
        // Complete the transition
        musicSource.Stop();
        
        // Swap the audio sources
        AudioSource tempSource = musicSource;
        musicSource = secondaryMusicSource;
        secondaryMusicSource = tempSource;
        
        // Reset skip flag
        isSkipping = false;
        
        // If there was an active music routine, stop it
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
        }
        
        // Start new playback routine
        musicRoutine = StartCoroutine(MusicPlaybackRoutine());
    }

    private void AdvanceTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= playlist.Count)
        {
            // Instead of shuffling immediately, queue up a shuffle for next frame
            StartCoroutine(ShuffleNextFrame());
            currentTrackIndex = 0;
        }
    }
    
    private IEnumerator ShuffleNextFrame()
    {
        // Delay the shuffle to the next frame to prevent lag
        yield return null;
        ShufflePlaylist();
    }

    public void PlayRandomMusic()
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }
        
        StartCoroutine(ShuffleNextFrame());
        currentTrackIndex = 0;
        musicRoutine = StartCoroutine(MusicPlaybackRoutine());
    }

    public void StopMusic()
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }
        
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
        
        // Only adjust current playback if not during a transition
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
        }
    }
    
    public void ResumeMusic()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.UnPause();
        }
    }
}