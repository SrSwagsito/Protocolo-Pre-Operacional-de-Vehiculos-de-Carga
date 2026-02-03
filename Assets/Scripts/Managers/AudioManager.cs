using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;


[DefaultExecutionOrder(-1001)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixerGroup musicMixer;
    [SerializeField] private AudioMixerGroup sfxMixer;

    [Header("SFX Pool Settings")]
    [SerializeField] private int minimalSFXSources = 10;
    [SerializeField] private int maxSFXSources = 25;

    private AudioSource musicSource;
    private List<AudioSource> sfxPool;

    private Dictionary<string, float> cooldownTracker = new Dictionary<string, float>();

    // ================================
    // SOUND DATA
    // ================================
    [Serializable]
    public class Sound
    {
        public string name;

        [Header("Audio Clips")]
        public AudioClip[] clips;

        [Header("Sound Properties")]
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        [Range(0f, 0.5f)] public float randomPitchVariation = 0.1f;

        public bool loop = false;
        public bool isMusic = false;

        [Header("Cooldown")]
        public float cooldown = 0f;

        [Header("Priority")]
        [Range(0, 10)] public int priority = 5; // Prioridad de sonidos
    }

    [Header("Sound List")]
    [SerializeField] private Sound[] sounds;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupMusicSource();
        SetupSFXPool();
    }

    private void SetupMusicSource()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicMixer;
        musicSource.loop = true;
    }

    private void SetupSFXPool()
    {
        sfxPool = new List<AudioSource>();
        for (int i = 0; i < minimalSFXSources; i++)
            CreateNewSFX();
    }

    private AudioSource CreateNewSFX()
    {
        AudioSource s = gameObject.AddComponent<AudioSource>();
        s.outputAudioMixerGroup = sfxMixer;

        s.spatialBlend = 0; // 0 = 2D por defecto
        sfxPool.Add(s);

        return s;
    }

    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource src in sfxPool)
            if (!src.isPlaying)
                return src;

        if (sfxPool.Count < maxSFXSources)
            return CreateNewSFX();

        // Si está lleno, buscamos uno de baja prioridad y lo sobreescribimos
        AudioSource lowest = sfxPool[0];
        return lowest;
    }

    // =====================================
    // HELPERS
    // =====================================

    private Sound GetSound(string name)
    {
        return Array.Find(sounds, s => s.name == name);
    }

    private bool CheckCooldown(Sound s)
    {
        if (s.cooldown <= 0) return true;

        if (!cooldownTracker.ContainsKey(s.name))
        {
            cooldownTracker[s.name] = Time.time;
            return true;
        }

        if (Time.time - cooldownTracker[s.name] >= s.cooldown)
        {
            cooldownTracker[s.name] = Time.time;
            return true;
        }

        return false;
    }


    // Play normal Sound

    public void Play(string name)
    {
        Sound sound = GetSound(name);
        if (sound == null)
        {
            Debug.LogWarning($"Sound '{name}' not found.");
            return;
        }

        if (!CheckCooldown(sound)) return;

        if (sound.isMusic)
        {
            PlayMusic(sound);
            return;
        }

        AudioSource src = GetAvailableSource();

        src.spatialBlend = 0f; // 2D
        src.volume = sound.volume;

        float pitch = sound.pitch;
        pitch += UnityEngine.Random.Range(-sound.randomPitchVariation, sound.randomPitchVariation);
        src.pitch = pitch;

        AudioClip clip = sound.clips.Length == 1
            ? sound.clips[0]
            : sound.clips[UnityEngine.Random.Range(0, sound.clips.Length)];

        src.PlayOneShot(clip, sound.volume);
    }

    // Play 3D Sound
    public void Play3D(string name, Vector3 position)
    {
        Sound sound = GetSound(name);
        if (sound == null)
        {
            Debug.LogWarning($"Sound '{name}' not found.");
            return;
        }

        if (!CheckCooldown(sound)) return;

        AudioSource src = GetAvailableSource();

        src.spatialBlend = 1f; // 1 = 3D
        src.transform.position = position;

        src.rolloffMode = AudioRolloffMode.Linear;
        src.minDistance = 3f;
        src.maxDistance = 25f;

        float pitch = sound.pitch;
        pitch += UnityEngine.Random.Range(-sound.randomPitchVariation, sound.randomPitchVariation);
        src.pitch = pitch;

        AudioClip clip = sound.clips.Length == 1
            ? sound.clips[0]
            : sound.clips[UnityEngine.Random.Range(0, sound.clips.Length)];

        src.PlayOneShot(clip, sound.volume);
    }

    // MUSIC

    public void PlayMusic(string name)
    {
        Sound sound = GetSound(name);
        if (sound == null || !sound.isMusic) return;

        PlayMusic(sound);
    }

    private void PlayMusic(Sound sound)
    {
        musicSource.Stop();
        musicSource.clip = sound.clips[0];
        musicSource.volume = sound.volume;
        musicSource.pitch = sound.pitch;
        musicSource.loop = sound.loop;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Music Fade
    public void FadeMusic(float targetVolume, float duration)
    {
        StartCoroutine(FadeMusicCoroutine(targetVolume, duration));
    }

    private IEnumerator FadeMusicCoroutine(float target, float duration)
    {
        float start = musicSource.volume;
        float t = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
    }
}