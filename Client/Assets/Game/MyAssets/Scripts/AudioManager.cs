using System;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public AudioClip[] audioClips;
    public AudioSource soundAudioSource, musicAudioSource;
    public AudioSource[] otherSounds;
    bool musicToggle = true;
    bool soundToggle = true;
    public Sprite musicOn, musicOff;
    public Sprite soundOn, soundOff;
    public Image musicBtnImage;
    public Image soundBtnImage;
    public static AudioManager Instance;

    public void Play(string name)
    {
        AudioClip clip = Array.Find(audioClips, sound => sound.name == name);

        if(soundAudioSource.clip != clip)
        soundAudioSource.clip = clip;

        soundAudioSource.Play();
    }

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey("Music"))
            MusicToggle();

        if (PlayerPrefs.HasKey("Sound"))
            SoundToggle();
    }

    public void MusicToggle()
    {
        musicToggle = !musicToggle;
        if (musicToggle)
        {
            musicBtnImage.sprite = musicOn;
            musicAudioSource.mute = false;
            PlayerPrefs.DeleteKey("Music");
        }
        else
        {
            musicBtnImage.sprite = musicOff;
            musicAudioSource.mute = true;
            PlayerPrefs.SetString("Music", "");
        }
    }

    public void SoundToggle()
    {
        soundToggle = !soundToggle;
        if (soundToggle)
        {
            soundBtnImage.sprite = soundOn;
            soundAudioSource.mute = false;

            foreach(AudioSource audio in otherSounds)
            {
                audio.mute = false;
            }

            PlayerPrefs.DeleteKey("Sound");
        }
        else
        {
            soundBtnImage.sprite = soundOff;
            soundAudioSource.mute = true;

            foreach (AudioSource audio in otherSounds)
            {
                audio.mute = true;
            }

            PlayerPrefs.SetString("Sound", "");
        }
    }
}
