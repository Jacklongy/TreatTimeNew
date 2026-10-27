using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// The sound class contains all the componnents of the sound, E.g the volume. 
/// </summary>
[System.Serializable]
public class Sound 
{
    public string name; 

    public AudioClip clip;
    // Optional extra clips; one is picked at random (along with the main clip) each play.
    public AudioClip[] variations;
    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

    [HideInInspector]
    public int lastIndex = -1;
}
