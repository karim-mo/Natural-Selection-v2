using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviourPun
{
    //public static AudioManager instance;

    [System.Serializable]
    public class Sound
    {

        public string name;

        public AudioClip clip;

        [Range(0, 1f)]
        public float volume;
        [Range(0, 1)]
        public float pitch;
        [Range(0, 1)]
        public float spatialBlend;

        public float maxDistance;

        public bool loop;

        [HideInInspector]
        public AudioSource source;
    }

    [Header("Sounds Configuration")]
    public Sound[] sounds;


    private static List<Sound> cache;

    //private void Awake()
    //{
    //    if (instance != null) Destroy(instance);
    //    else
    //        instance = this;
    //}

    void Start()
    {
        if(GetComponent<PhotonView>()) 
            if (!photonView.IsMine) return;

        cache = new List<Sound>();

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.loop = s.loop;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.spatialBlend = s.spatialBlend;
            s.source.maxDistance = s.maxDistance;

            cache.Add(s);
        }
        Play("BGM");
    }

    public void MuteMusic()
    {
        foreach (Sound s in cache)
        {
            if (s.name == "BGM")
            {
                s.source.volume = 0;
                return;
            }
        }
    }

    public void UnmuteMusic()
    {
        foreach (Sound s in cache)
        {
            if (s.name == "BGM")
            {
                s.source.volume = 0.65f;
                return;
            }
        }
    }

    public void Play(string n)
    {
        foreach (Sound s in cache)
        {
            if (s.name == n)
            {
                s.source.Play();
                return;
            }
        }
    }
    public void PlayOne(string n)
    {
        foreach (Sound s in cache)
        {
            if (s.name == n)
            {
                s.source.PlayOneShot(s.clip);
                return;
            }
        }
    }

    public void Stop(string n)
    {
        foreach (Sound s in cache)
        {
            if (s.name == n)
            {
                s.source.Stop();
                return;
            }
        }
    }

    public AudioClip Find(string n)
    {
        foreach (Sound s in cache)
        {
            if (s.name == n)
            {
                 return s.clip;
            }
        }
        return null;
    }
}

