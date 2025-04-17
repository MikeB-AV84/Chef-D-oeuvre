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
    public string skipButton = "RB_Button";

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
        musicSource.loop = false;
        musicSource.volume = 0f; // Start at zero for fade-in
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
        for (int i = playlist.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            AudioClip temp = playlist[i];
            playlist[i] = playlist[randomIndex];
            playlist[randomIndex] = temp;
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
            timer = fadeInDuration;
            while (timer < musicSource.clip.length && !isSkipping)
            {
                timer += Time.unscaledDeltaTime;
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
                
                // Delay between tracks
                yield return new WaitForSecondsRealtime(trackSwitchDelay);
                
                // Advance to next track
                AdvanceTrack();
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
        
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }
        
        musicRoutine = StartCoroutine(SkipRoutine());
    }

    private IEnumerator SkipRoutine()
    {
        isSkipping = true;

        // Fade out
        float startVolume = musicSource.volume;
        float timer = 0f;
        while (timer < skipFadeDuration)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / skipFadeDuration);
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        musicSource.Stop();
        AdvanceTrack();

        // Reset skip flag before starting new playback
        isSkipping = false;
        
        // Start new playback directly
        musicRoutine = StartCoroutine(MusicPlaybackRoutine());
    }

    private void AdvanceTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= playlist.Count)
        {
            ShufflePlaylist();
            currentTrackIndex = 0;
        }
    }

    public void PlayRandomMusic()
    {
        if (musicRoutine != null)
        {
            StopCoroutine(musicRoutine);
            musicRoutine = null;
        }
        
        ShufflePlaylist();
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
        
        musicSource.Stop();
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