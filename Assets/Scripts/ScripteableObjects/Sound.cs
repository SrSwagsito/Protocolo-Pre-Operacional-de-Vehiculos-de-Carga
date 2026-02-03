using System;
using UnityEngine;

[Serializable]
public class Sound
{
    public string id; 
    public AudioClip clip; 
    [Range (0,1)] public float volume;
}
