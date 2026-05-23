using UnityEngine;
using System.Collections;

public class LevelMusicManager : MonoBehaviour
{
    public static LevelMusicManager Instance;

    [Header("Reproductores de Música")]
    public AudioSource normalMusic;
    public AudioSource combatMusic;
    public AudioSource bossMusic;

    [Header("Configuración de Crossfade")]
    [Range(0f, 1f)] public float baseVolume = 0.4f;
    public float fadeSpeed = 1f; // Cómo de rápido hace la transición de volumen
    public float outOfCombatDelay = 2f; // Los segundos de gracia antes de volver a la música normal

    private int normalEnemiesCount = 0;
    private int bossesCount = 0;
    private float outOfCombatTimer = 0f;

    private enum MusicState { Normal, Combat, Boss }
    private MusicState currentState = MusicState.Normal;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Empezamos con todo silenciado
        if (normalMusic != null) normalMusic.volume = 0f;
        if (combatMusic != null) combatMusic.volume = 0f;
        if (bossMusic != null) bossMusic.volume = 0f;

        // Reproducimos todas las canciones en bucle a la vez (pero en silencio)
        if (normalMusic != null) normalMusic.Play();
        if (combatMusic != null) combatMusic.Play();
        if (bossMusic != null) bossMusic.Play();
    }

    void Update()
    {
        UpdateCombatState();
        HandleMusicFading();
    }

    // Funciones que llamarán los enemigos al verte o morir
    public void AddEnemyToCombat(bool isBoss)
    {
        if (isBoss) bossesCount++;
        else normalEnemiesCount++;
    }

    public void RemoveEnemyFromCombat(bool isBoss)
    {
        if (isBoss) bossesCount = Mathf.Max(0, bossesCount - 1);
        else normalEnemiesCount = Mathf.Max(0, normalEnemiesCount - 1);
    }

    private void UpdateCombatState()
    {
        if (bossesCount > 0)
        {
            currentState = MusicState.Boss;
            outOfCombatTimer = 0f; // Reseteamos el reloj
        }
        else if (normalEnemiesCount > 0)
        {
            currentState = MusicState.Combat;
            outOfCombatTimer = 0f; // Reseteamos el reloj
        }
        else
        {
            // Si llegamos a 0 enemigos, empieza la cuenta atrás de 2 segundos
            if (currentState != MusicState.Normal)
            {
                outOfCombatTimer += Time.deltaTime;
                if (outOfCombatTimer >= outOfCombatDelay)
                {
                    currentState = MusicState.Normal;
                }
            }
        }
    }

    private void HandleMusicFading()
    {
        // Calculamos el volumen máximo leyendo el Slider global de tu SoundEffectManager
        float maxVol = baseVolume * SoundEffectManager.MusicVolumeMultiplier;

        // Decidimos a qué volumen debe ir cada canción según el estado actual
        float targetNormal = (currentState == MusicState.Normal) ? maxVol : 0f;
        float targetCombat = (currentState == MusicState.Combat) ? maxVol : 0f;
        float targetBoss = (currentState == MusicState.Boss) ? maxVol : 0f;

        // Movemos el volumen de las 3 pistas suavemente (Crossfade)
        if (normalMusic != null) normalMusic.volume = Mathf.MoveTowards(normalMusic.volume, targetNormal, fadeSpeed * Time.deltaTime);
        if (combatMusic != null) combatMusic.volume = Mathf.MoveTowards(combatMusic.volume, targetCombat, fadeSpeed * Time.deltaTime);
        if (bossMusic != null) bossMusic.volume = Mathf.MoveTowards(bossMusic.volume, targetBoss, fadeSpeed * Time.deltaTime);
    }
}