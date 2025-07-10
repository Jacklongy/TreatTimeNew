using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using UnityEngine.SceneManagement;
public class SoundManagerScript : MonoBehaviour
{
    // Array of sounds we can add too. 

    public Sound[] sounds;
    public static SoundManagerScript instance;

    /// <summary>
    /// Make sure there is only one instance of the sound manager. So we dont play sounds twice. 
    /// </summary>
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
  
        DontDestroyOnLoad(gameObject);


        // Add an audio source for all sounds in the array. 
        foreach (Sound s in sounds)
        {
            // s = sound that we are currently lookking at. = the new audio source component
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;

            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    /// <summary>
    /// get the name of the sound then play it. 
    /// </summary>
    /// <param name="name"> The name of the sound. </param>
    public void Play (string name)
    {
       Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.Log("name of sound wrong");

            return;
        }

        s.source.Play();
    }


    /// <summary>
    /// Stop playing to control sounds that are looping. 
    /// </summary>
    /// <param name="name"> name of sound. </param>
    public void StopPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.Log("name of sound wrong");

            return;
        }

        s.source.Stop();
    }


}
