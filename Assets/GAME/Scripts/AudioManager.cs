using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum AudioChannel
{
    Master,
    Music,
    SFX,
    Voice
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Clips")]
    public List<AudioClip> clips;

    [Header("Volumes")]
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float voiceVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public SoundInstance PlayOneShot2D(string clipName)
    {
        AudioClip clip = clips.Find(c => c.name == clipName);
        return new SoundInstance(clip, false);
    }
    
    public SoundInstance PlayOneShot2D(AudioClip clip)
    {
        return new SoundInstance(clip, false);
    }

    public SoundInstance PlayOneShot3D(string clipName, Vector3 position)
    {
        AudioClip clip = clips.Find(c => c.name == clipName);
        return new SoundInstance(clip, true, position);
    }

}

public class SoundInstance
{
    private AudioClip clip;
    private bool is3D;
    private Vector3 position;
    private float volume = 1f;
    private float pitchVariation = 0f;
    private float maxDistance = 100f;
    private AudioMixerGroup audioMixerGroup;

    public SoundInstance(AudioClip clip, bool is3D, Vector3 position = default)
    {
        this.clip = clip;
        this.is3D = is3D;
        this.position = position;
    }

    public SoundInstance Volume(float v)
    {
        volume = Mathf.Clamp01(v);
        return this;
    }

    public SoundInstance PitchVariation(float variation)
    {
        pitchVariation = variation;
        return this;
    }

    public SoundInstance MaxDistance(float maxDistance)
    {
        this.maxDistance = maxDistance;
        return this;
    }

    public SoundInstance AudioMixerGroup(AudioMixerGroup group)
    {
        this.audioMixerGroup = group;
        return this;
    }

    public void Play()
    {
        if (clip == null) return;

        GameObject go = new GameObject("Audio_" + clip.name);
        AudioSource source = go.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.spatialBlend = is3D ? 1f : 0f;
        source.maxDistance = maxDistance;
        source.outputAudioMixerGroup = audioMixerGroup;
        
        if (is3D)
            go.transform.position = position;

        source.Play();
        Object.Destroy(go, clip.length / source.pitch);
    }
}
