using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    private static AudioPlayer _instance = null;

    public static AudioPlayer Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<AudioPlayer>();
            }

            return _instance;
        }
    }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> audioClips;

    public void PlaySfx(string clipName)
    {
        AudioClip sfx = audioClips.Find(a => a.name == clipName);

        if (sfx == null) return;
        
        audioSource.PlayOneShot(sfx);
    }
}
