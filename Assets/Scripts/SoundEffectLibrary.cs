using System.Collections.Generic;
using UnityEngine;

// Esta es la "Discoteca" o "Biblioteca" de efectos de sonido del juego.
// Guarda todos los ruidos (espadazos, pasos, menús) organizados por categorías.
public class SoundEffectLibrary : MonoBehaviour
{
    // Lista visible en el Inspector donde agrupamos los sonidos
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;

    // Una agenda interna para buscar grupos de sonido rapidísimo por su nombre
    private Dictionary<string, SoundEffectGroup> soundDictionary;

    public void Awake()
    {
        InitializeDictionary();
    }

    // Al arrancar, cogemos la lista del Inspector y la organizamos en nuestra agenda interna
    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, SoundEffectGroup>();
        foreach (SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            // Guardamos el grupo entero asociado a su nombre (Ej: "SwordSwing")
            soundDictionary[soundEffectGroup.name] = soundEffectGroup;
        }
    }

    // Esta función la usan otros scripts cuando quieren hacer ruido. 
    // Piden un sonido por su nombre y la función les devuelve el archivo de audio y el volumen al que debe sonar.
    public AudioClip GetRandomClip(string name, out float volume)
    {
        volume = 1f; // Volumen estándar por si acaso

        // Si tenemos ese sonido en nuestra agenda...
        if (soundDictionary.ContainsKey(name))
        {
            SoundEffectGroup group = soundDictionary[name];
            volume = group.volume; // Leemos si el programador le bajó el volumen en el Inspector

            // TRUCO PROFESIONAL: Si hay varios sonidos parecidos (ej: 3 quejidos distintos),
            // elegimos uno al azar para que no suene repetitivo y artificial.
            if (group.audioClips.Count > 0)
            {
                return group.audioClips[Random.Range(0, group.audioClips.Count)];
            }
        }
        return null; // Si no encuentra el sonido, no devuelve nada
    }
}

// Esta estructura es como una "carpeta" personalizada para ordenar los sonidos en Unity
[System.Serializable]
public struct SoundEffectGroup
{
    public string name; // Nombre del grupo (Ej: "PasosHierba")

    // Barra deslizante en el Inspector para ajustar el volumen de este sonido específico (de 0 a 1)
    [Range(0f, 1f)] public float volume;

    // Lista de archivos de audio que suenan parecido y podemos alternar
    public List<AudioClip> audioClips;
}