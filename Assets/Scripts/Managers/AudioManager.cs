using System.Collections.Generic;
using UnityEngine;

public enum SoundEffectType
{
    BoxDrop, BoxGrab, BoxOpen, Checkout, FurniturePlace, FurniturePickup, ItemPickup, ItemPlace, Jump, ItemThrow, Trash
}

[System.Serializable]
public class SoundEffect
{
    public AudioSource effectAudio;
    public SoundEffectType effectType;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] AudioSource titleMusic;
    [SerializeField] List<AudioSource> bgm = new();

    [SerializeField] List<SoundEffect> sfx;

    private bool bgmPlaying;
    private int currentTrack;

    void Awake()
    {
        instance = this;

         foreach(AudioSource track in bgm)
            track.volume = 0.1f;
    }

    void Start()
    {
        StopMusic();

        bgmPlaying = true;

        currentTrack = Random.Range(0, bgm.Count);

        bgm[currentTrack].Play();
    }

    void Update()
    {
        if(!bgmPlaying)
            return;

        if(bgm[currentTrack].isPlaying == false)
        {
            currentTrack++;

            if(currentTrack >= bgm.Count)
                currentTrack = 0;

            bgm[currentTrack].Play();
        }
    }

    private void StopMusic()
    {
        titleMusic.Stop();

        foreach(AudioSource track in bgm)
            track.Stop();
    }

    public void PlaySFX(SoundEffectType sfxToPlay)
    {
        AudioSource effectAudio = sfx.Find(x => x.effectType == sfxToPlay).effectAudio;

        effectAudio.Stop();
        effectAudio.Play();
    }
}
