using System.Collections.Generic;
using UnityEngine;

public class SoundEffectLibrary : MonoBehaviour
{
    [SerializeField] private SoundEffectGroup[] soundEffectGroups;

    // Cambiamos el diccionario para guardar el grupo completo, no solo la lista
    private Dictionary<string, SoundEffectGroup> soundDictionary;

    public void Awake()
    {
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        soundDictionary = new Dictionary<string, SoundEffectGroup>();
        foreach (SoundEffectGroup soundEffectGroup in soundEffectGroups)
        {
            soundDictionary[soundEffectGroup.name] = soundEffectGroup;
        }
    }

    // Añadimos "out float volume" para que devuelva también el volumen configurado
    public AudioClip GetRandomClip(string name, out float volume)
    {
        volume = 1f; // Volumen por defecto

        if (soundDictionary.ContainsKey(name))
        {
            SoundEffectGroup group = soundDictionary[name];
            volume = group.volume; // Leemos el volumen de este grupo específico

            if (group.audioClips.Count > 0)
            {
                return group.audioClips[Random.Range(0, group.audioClips.Count)];
            }
        }
        return null;
    }
}

[System.Serializable]
public struct SoundEffectGroup
{
    public string name;

    // ¡NUEVO! Esto creará una barra de volumen de 0 a 1 en tu Inspector
    [Range(0f, 1f)] public float volume;

    public List<AudioClip> audioClips;
}