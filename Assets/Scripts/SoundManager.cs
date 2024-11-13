using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
public class SoundManager : MonoBehaviour
{
    float m_soundVolume = 1;
    public float SoundVolume
    {
        get
        {
            return m_soundVolume;
        }
        set
        {
            foreach(KeyValuePair<GameObject, SoundInfo> soundInfoForObject in activeSounds)
            {
                if(soundInfoForObject.Value.VolumeMode == SoundInfo.SoundMode.Standard)
                {
                    soundInfoForObject.Key.GetComponent<AudioSource>().volume = soundInfoForObject.Value.Volume.Remap(0,1,0,value);
                }
            }
            m_soundVolume = value;
        }
    }
    float m_musicVolume = 1;
    public float MusicVolume
    {
        get
        {
            return m_musicVolume;
        }
        set
        {
            foreach(KeyValuePair<GameObject, SoundInfo> soundInfoForObject in activeSounds)
            {
                if(soundInfoForObject.Value.VolumeMode == SoundInfo.SoundMode.Music)
                {
                    soundInfoForObject.Key.GetComponent<AudioSource>().volume = soundInfoForObject.Value.Volume.Remap(0,1,0,value);
                }
            }
            m_musicVolume = value;
        }
    }
    void OnSceneLoaded(Scene oldScene, Scene newScene)
    {
        soundPool.Clear();
    }
    [SerializeField] List<SoundInfo> m_sounds = new List<SoundInfo>();
    public List<SoundInfo> Sounds {  get { return m_sounds; } }
    public static SoundManager instance;
    public ObjectPool<GameObject> soundPool;
    [SerializeField] Dictionary<GameObject, SoundInfo> activeSounds;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            soundPool = new ObjectPool<GameObject>(OnCreateSound, OnGetFromPool, OnReturnToPool, OnDestroySound);
            SceneManager.activeSceneChanged += OnSceneLoaded;
        }
        if(instance != this)
        {
            Destroy(this);
        }
    }
#nullable enable
    public SoundInfo? FindSoundInfoByName(string name)
    {
        return m_sounds.Find(sound => sound.Name == name);
    }
#nullable disable
    /// <summary>
    /// Plays a sound at a static location
    /// </summary>
    /// <param name="location">The location that the sound should play at</param>
    /// <param name="soundInfo">The info of the sound that should be played</param>
    /// <returns>The GameObject with the attached AudioSource</returns>
    public GameObject PlaySound(Vector3 location, SoundInfo soundInfo)
    {
        GameObject sound = soundPool.Get();
        sound.transform.position = location;
        AudioSource source = SetupSound(sound, soundInfo);

        source.Play();
        Debug.Log(source.isPlaying);
        return sound;
    }
    /// <summary>
    /// Plays the sound on a given transform
    /// </summary>
    /// <param name="transform">The transform the sound should be attached to</param>
    /// <param name="soundInfo">The info of the sound that should be played</param>
    /// <returns>The GameObject with the attached AudioSource</returns>
    public GameObject PlaySound(Transform transform, SoundInfo soundInfo)
    {
        GameObject sound = soundPool.Get();
        sound.transform.parent = transform;
        sound.transform.localPosition = Vector3.zero;
        AudioSource source = SetupSound(sound, soundInfo);

        source.Play();
        return sound;
    }
    AudioSource SetupSound(GameObject sound, SoundInfo soundInfo)
    {
        AudioSource source = sound.GetComponent<AudioSource>();
        source.clip = soundInfo.AudioClip;
        source.outputAudioMixerGroup = soundInfo.AudioMixerGroup;
        source.mute = soundInfo.Mute;
        source.bypassEffects = soundInfo.BypassEffects;
        source.bypassListenerEffects = soundInfo.BypassListenerEffects;
        source.playOnAwake = soundInfo.PlayOnAwake;
        source.loop = soundInfo.Loop;
        source.priority = soundInfo.Priority;
        source.volume = soundInfo.Volume.Remap(0, 1, 0, soundInfo.VolumeMode == SoundInfo.SoundMode.Standard? SoundVolume : MusicVolume);
        source.pitch = soundInfo.Pitch;
        source.panStereo = soundInfo.StereoPan;
        source.spatialBlend = soundInfo.SpatialBlend;
        source.reverbZoneMix = soundInfo.ReverbZoneMix;
        source.minDistance = soundInfo.MinDistance;
        source.maxDistance = soundInfo.MaxDistance;
        if (!soundInfo.Loop && soundInfo.AudioClip != null)
        {
            StartCoroutine(DestroySound(sound, soundInfo.AudioClip.length));
        }
        return source;
    }
    IEnumerator DestroySound(GameObject soundToDestroy, float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        if(soundToDestroy != null)
        {
            soundPool.Release(soundToDestroy);
        }
    }
    GameObject OnCreateSound()
    {
        return new GameObject("Sound", typeof(AudioSource), typeof(SphereCollider));
    }
    void OnGetFromPool(GameObject objectFromPool)
    {
        objectFromPool.SetActive(true);
    }
    void OnReturnToPool(GameObject objectFromPool)
    {
        objectFromPool.SetActive(false);
    }
    private void OnDestroySound(GameObject soundToDestroy)
    {
        Destroy(soundToDestroy.gameObject);
    }
    [System.Serializable]
    public class SoundInfo
    {
        public string Name;
        public SoundMode VolumeMode;
        public AudioClip AudioClip;
        public AudioMixerGroup AudioMixerGroup;
        public bool Mute;
        public bool BypassEffects;
        public bool BypassListenerEffects;
        public bool PlayOnAwake;
        public bool Loop;
        [SerializeField, Range(0, 256)]
        int _priority = 128;
        public int Priority
        {
            get { return _priority; }
            set { _priority = Mathf.Clamp(value, 0, 256); }
        }
        [SerializeField, Range(0, 1)]
        float _volume = 1;
        public float Volume
        {
            get { return _volume; }
            set { _volume = Mathf.Clamp(value, 0, 1); }
        }
        [SerializeField, Range(-3, 3)]
        float _pitch = 1;
        public float Pitch
        {
            get { return _pitch; }
            set { _pitch = Mathf.Clamp(value, -3, 3); }
        }
        [SerializeField, Range(-1, 1)]
        float _stereoPan = 0;
        public float StereoPan
        {
            get { return _stereoPan; }
            set { _stereoPan = Mathf.Clamp(value, -1, 1); }
        }
        [SerializeField, Range(0, 1)]
        float _spatialBlend = 0;
        public float SpatialBlend
        {
            get { return _spatialBlend; }
            set { _spatialBlend = Mathf.Clamp(value, 0, 1); }
        }
        [SerializeField, Range(0, 1.1f)]
        float _reverbZoneMix = 1;
        public float ReverbZoneMix
        {
            get { return _reverbZoneMix; }
            set { _reverbZoneMix = Mathf.Clamp(value, 0, 1.1f); }
        }
        [SerializeField]
        float _minDistance = 1;
        public float MinDistance
        {
            get { return _minDistance; }
            set { _minDistance = Mathf.Clamp(value, 0, MaxDistance); }
        }

        [SerializeField]
        float _maxDistance = 500;
        public float MaxDistance
        {
            get { return _maxDistance; }
            set { _maxDistance = Mathf.Clamp(value, MinDistance, float.MaxValue); }
        }
        public enum SoundMode
        {
            Standard,
            Music
        }
        public SoundInfo
            (
            string name,
            SoundMode volumeMode,
            AudioClip audioClip,
            AudioMixerGroup audioMixerGroup = null,
            bool mute = false,
            bool bypassEffects = false,
            bool bypassListenerEffects = false,
            bool playOnAwake = true,
            bool loop = false,
            int priority = 128,
            float volume = 1,
            float pitch = 1,
            float stereoPan = 0,
            float spatialBlend = 0,
            float reverbZoneMix = 1,
            float minDistance = 1,
            float maxDistance = 500
            )
        {
            Name = name;
            VolumeMode = volumeMode;
            AudioClip = audioClip;
            AudioMixerGroup = audioMixerGroup;
            Mute = mute;
            BypassEffects = bypassEffects;
            BypassListenerEffects = bypassListenerEffects;
            PlayOnAwake = playOnAwake;
            Loop = loop;
            Priority = priority;
            Volume = volume;
            Pitch = pitch;
            StereoPan = stereoPan;
            SpatialBlend = spatialBlend;
            ReverbZoneMix = reverbZoneMix;
            MinDistance = minDistance;
            MaxDistance = maxDistance;

        }
    }
}