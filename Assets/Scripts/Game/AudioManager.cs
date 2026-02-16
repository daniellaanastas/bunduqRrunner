using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    public Sound[] sounds;
    
    private Dictionary<string, Sound> soundLookup;

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

        soundLookup = new Dictionary<string, Sound>(sounds.Length);
        
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.loop = s.loop;
            s.source.pitch = s.pitch;
            s.source.playOnAwake = false;
            
            if (!soundLookup.ContainsKey(s.name))
                soundLookup.Add(s.name, s);
        }
        
        PlaySound("MainTheme");
    }

    public void PlaySound(string name)
    {
        if (soundLookup.TryGetValue(name, out Sound s))
        {
            if (!s.source.isPlaying || !s.loop)
                s.source.Play();
        }
    }
    
    public void StopSound(string name)
    {
        if (soundLookup.TryGetValue(name, out Sound s))
            s.source.Stop();
    }
}