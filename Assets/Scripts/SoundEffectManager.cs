using UnityEngine;
using UnityEngine.UI;

public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    private static AudioSource audioSource;
    private static SoundEffectLibrary soundEffectLibrary;
    private static AudioSource voiceAudioSource;

    // Quitamos los [SerializeField] porque los sliders se conectan por código desde el menú
    private Slider sfxSlider;
    private Slider musicSlider;

    private static AudioSource currentBackgroundMusic;
    private static float currentBaseMusicVolume = 1f;

    public static float MusicVolumeMultiplier = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Aseguramos que viaja bien entre escenas

            AudioSource[] audioSources = GetComponents<AudioSource>();
            audioSource = audioSources[0];
            if (audioSources.Length > 1) voiceAudioSource = audioSources[1];

            soundEffectLibrary = GetComponent<SoundEffectLibrary>();
            DontDestroyOnLoad(gameObject);

            LoadSavedVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Play(string soundName)
    {
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName, out float clipVolume);
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip, clipVolume);
        }
    }

    public static void PlayVoice(AudioClip audioClip, float pitch = 1f, float baseVolume = 1f)
    {
        if (voiceAudioSource == null) return;

        voiceAudioSource.pitch = pitch;
        voiceAudioSource.clip = audioClip;

        float globalSfxVolume = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : 1f;
        voiceAudioSource.volume = baseVolume * globalSfxVolume;

        voiceAudioSource.Play();
    }

    private void LoadSavedVolumes()
    {
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            float savedSFX = PlayerPrefs.GetFloat("SFXVolume");
            if (audioSource != null) audioSource.volume = savedSFX;
            if (voiceAudioSource != null) voiceAudioSource.volume = savedSFX;
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            MusicVolumeMultiplier = PlayerPrefs.GetFloat("MusicVolume");
        }
    }

    public void ConnectSliders(Slider sfx, Slider music)
    {
        sfxSlider = sfx;
        musicSlider = music;

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(delegate { OnSFXValueChanged(); });
        }

        if (musicSlider != null)
        {
            musicSlider.value = MusicVolumeMultiplier;
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(delegate { OnMusicValueChanged(); });
        }
    }

    public void OnSFXValueChanged()
    {
        if (sfxSlider == null || audioSource == null) return;

        float vol = sfxSlider.value;
        audioSource.volume = vol;
        if (voiceAudioSource != null) voiceAudioSource.volume = vol;
        PlayerPrefs.SetFloat("SFXVolume", vol);
    }

    public void OnMusicValueChanged()
    {
        if (musicSlider == null) return;

        MusicVolumeMultiplier = musicSlider.value;
        PlayerPrefs.SetFloat("MusicVolume", MusicVolumeMultiplier);

        if (currentBackgroundMusic != null)
        {
            currentBackgroundMusic.volume = currentBaseMusicVolume * MusicVolumeMultiplier;
        }
    }

    public static void RegisterBackgroundMusic(AudioSource musicSource, float baseVolume)
    {
        currentBackgroundMusic = musicSource;
        currentBaseMusicVolume = baseVolume;
        currentBackgroundMusic.volume = currentBaseMusicVolume * MusicVolumeMultiplier;
    }
}