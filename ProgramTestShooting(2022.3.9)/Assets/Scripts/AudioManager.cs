using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop = false;
    public AudioMixerGroup mixerGroup;

    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    public AudioMixerGroup sfxMixerGroup;
    public AudioMixerGroup musicMixerGroup;

    [Header("Volume Settings")]
    [Range(0.0001f, 1)]
    public float masterVolume = 0f;
    [Range(0.0001f, 1)]
    public float musicVolume = 0f;
    [Range(0.0001f, 1)]
    public float sfxVolume = 0f;

    [Header("Sounds")]
    public Sound[] sounds;

    private Dictionary<string, Sound> soundDictionary = new Dictionary<string, Sound>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeAudio();
    }

    void InitializeAudio()
    {
        foreach (Sound sound in sounds)
        {
            GameObject soundObject = new GameObject($"Sound_{sound.name}");
            soundObject.transform.SetParent(this.transform);

            sound.source = soundObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;

            sound.source.outputAudioMixerGroup = sound.mixerGroup;

            soundDictionary[sound.name] = sound;
        }
    }
    private void Start()
    {
        SetMasterVolume(masterVolume);
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    public void Play(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.Play();
        }
        else
        {
            Debug.Log($"Sound {name} not found!");
        }
    }

    public void Stop(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.Stop();
        }
    }

    public void StopAll()
    {
        foreach (var sound in soundDictionary.Values)
        {
            sound.source.Stop();
        }
    }

    public void PlayOneShot(string name)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            sound.source.PlayOneShot(sound.clip);
        }
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat(StringManager.MASTER_VOLUME_PARAMETER, Mathf.Log10(volume)*20);
    }

    public void SetMusicVolume(float volume)
    {
        audioMixer.SetFloat(StringManager.MUSIC_VOLUME_PARAMETER, Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat(StringManager.SFX_VOLUME_PARAMETER, Mathf.Log10(volume) * 20);
    }

    public void FadeOut(string name, float duration)
    {
        if (soundDictionary.TryGetValue(name, out Sound sound))
        {
            StartCoroutine(FadeOutCoroutine(sound.source, duration));
        }
    }

    IEnumerator FadeOutCoroutine(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

}
