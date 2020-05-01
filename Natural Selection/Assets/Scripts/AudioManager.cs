using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {

        public string name;

        public AudioClip clip;

        [Range(0, 1f)]
        public float volume;
        [Range(0, 1)]
        public float pitch;

        public bool loop;

        [HideInInspector]
        public AudioSource source;
    }

    [Header("Sounds Configuration")]
    public Sound[] sounds;


    private static List<Sound> cache;

    void Start()
    {
        cache = new List<Sound>();

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.loop = s.loop;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            cache.Add(s);
        }
        Play("BGM");
    }

    public static void MuteMusic()
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

    public static void UnmuteMusic()
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

    public static void Play(string n)
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

    public static void Stop(string n)
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
}

