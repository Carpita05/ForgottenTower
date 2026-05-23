using UnityEngine;
using UnityEngine.UI;

// Este script (Singleton) es el Mánager Principal de Sonido del juego.
// Controla el volumen general, reproduce los efectos, gestiona las voces de los NPCs 
// y recuerda la configuración del jugador guardándola en el disco duro.
public class SoundEffectManager : MonoBehaviour
{
    public static SoundEffectManager Instance;

    // Reproductores físicos de Unity
    private static AudioSource audioSource;       // Para efectos generales (golpes, menús)
    private static AudioSource voiceAudioSource;  // Exclusivo para las "voces" de los NPCs
    private static SoundEffectLibrary soundEffectLibrary; // Nuestra librería de sonidos

    // Barras de volumen de la interfaz (UI)
    private Slider sfxSlider;
    private Slider musicSlider;

    // Control de la música de fondo
    private static AudioSource currentBackgroundMusic;
    private static float currentBaseMusicVolume = 1f;
    public static float MusicVolumeMultiplier = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Lo soltamos de cualquier padre para que viaje libre entre niveles

            // Buscamos los reproductores de audio que le hayamos puesto a este objeto
            AudioSource[] audioSources = GetComponents<AudioSource>();
            audioSource = audioSources[0];

            // Si le pusimos un segundo reproductor, lo usamos para las voces
            if (audioSources.Length > 1) voiceAudioSource = audioSources[1];

            soundEffectLibrary = GetComponent<SoundEffectLibrary>();

            // Hacemos que este objeto sea inmortal (no se destruye al cambiar de escena)
            DontDestroyOnLoad(gameObject);

            // Cargamos la configuración de volumen que el jugador eligió la última vez
            LoadSavedVolumes();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Reproduce un efecto de sonido general buscando su nombre en la librería
    public static void Play(string soundName)
    {
        AudioClip audioClip = soundEffectLibrary.GetRandomClip(soundName, out float clipVolume);
        if (audioClip != null)
        {
            audioSource.PlayOneShot(audioClip, clipVolume);
        }
    }

    // Reproduce el "blablabla" de los personajes al hablar con la velocidad (pitch) ajustada
    public static void PlayVoice(AudioClip audioClip, float pitch = 1f, float baseVolume = 1f)
    {
        if (voiceAudioSource == null) return;

        voiceAudioSource.pitch = pitch;
        voiceAudioSource.clip = audioClip;

        // Calculamos el volumen final mezclando el volumen del personaje con el del menú de opciones
        float globalSfxVolume = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : 1f;
        voiceAudioSource.volume = baseVolume * globalSfxVolume;

        voiceAudioSource.Play();
    }

    // --- GUARDADO Y CARGA DE OPCIONES ---

    private void LoadSavedVolumes()
    {
        // Si ya habíamos guardado el volumen de efectos antes...
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            float savedSFX = PlayerPrefs.GetFloat("SFXVolume");
            if (audioSource != null) audioSource.volume = savedSFX;
            if (voiceAudioSource != null) voiceAudioSource.volume = savedSFX;
        }

        // Si ya habíamos guardado el volumen de la música...
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            MusicVolumeMultiplier = PlayerPrefs.GetFloat("MusicVolume");
        }
    }

    // Esta función la llama el Menú de Pausa al abrirse para conectar sus barras de volumen a este mánager
    public void ConnectSliders(Slider sfx, Slider music)
    {
        sfxSlider = sfx;
        musicSlider = music;

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.RemoveAllListeners(); // Limpiamos eventos viejos por seguridad
            sfxSlider.onValueChanged.AddListener(delegate { OnSFXValueChanged(); });
        }

        if (musicSlider != null)
        {
            musicSlider.value = MusicVolumeMultiplier;
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(delegate { OnMusicValueChanged(); });
        }
    }

    // Se dispara cada vez que movemos la barra de Efectos (SFX)
    public void OnSFXValueChanged()
    {
        if (sfxSlider == null || audioSource == null) return;

        float vol = sfxSlider.value;
        audioSource.volume = vol;
        if (voiceAudioSource != null) voiceAudioSource.volume = vol;

        // Guardamos el nuevo valor en el ordenador
        PlayerPrefs.SetFloat("SFXVolume", vol);
    }

    // Se dispara cada vez que movemos la barra de Música
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

    // Registra la música del nivel actual para poder aplicarle las subidas y bajadas de volumen
    public static void RegisterBackgroundMusic(AudioSource musicSource, float baseVolume)
    {
        currentBackgroundMusic = musicSource;
        currentBaseMusicVolume = baseVolume;
        currentBackgroundMusic.volume = currentBaseMusicVolume * MusicVolumeMultiplier;
    }
}