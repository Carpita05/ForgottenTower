using UnityEngine;
using System.Collections;

// Este script es el "DJ" del juego. Controla la música de fondo y hace transiciones 
// suaves (Crossfade) entre la música de exploración, la de combate normal y la de jefe.
public class LevelMusicManager : MonoBehaviour
{
    public static LevelMusicManager Instance;

    [Header("Reproductores de Música")]
    // Truco profesional: En vez de apagar una canción y encender otra (que suena brusco),
    // tenemos 3 reproductores sonando a la vez, y solo jugamos con subir y bajar sus volúmenes.
    public AudioSource normalMusic;
    public AudioSource combatMusic;
    public AudioSource bossMusic;

    [Header("Configuración de Transiciones")]
    [Range(0f, 1f)] public float baseVolume = 0.4f;
    public float fadeSpeed = 1f; // Cómo de rápido se funde una canción con otra

    // Los "segundos de gracia" que te da el juego cuando matas al último enemigo 
    // antes de volver a poner la música tranquila.
    public float outOfCombatDelay = 2f;

    // Contadores para saber cuánta gente nos está atacando
    private int normalEnemiesCount = 0;
    private int bossesCount = 0;
    private float outOfCombatTimer = 0f;

    // Los 3 "modos" o estados musicales posibles
    private enum MusicState { Normal, Combat, Boss }
    private MusicState currentState = MusicState.Normal;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Al empezar el nivel, silenciamos los 3 reproductores por si acaso
        if (normalMusic != null) normalMusic.volume = 0f;
        if (combatMusic != null) combatMusic.volume = 0f;
        if (bossMusic != null) bossMusic.volume = 0f;

        // Le damos al "Play" a todas las canciones a la vez en bucle (pero en completo silencio)
        if (normalMusic != null) normalMusic.Play();
        if (combatMusic != null) combatMusic.Play();
        if (bossMusic != null) bossMusic.Play();
    }

    void Update()
    {
        UpdateCombatState(); // Decide qué estado toca (Tranquilo, Combate o Jefe)
        HandleMusicFading(); // Ajusta las ruedecitas de volumen según el estado
    }

    // --- SENSORES DE COMBATE ---

    // Los enemigos llaman a esta función cuando te ven
    public void AddEnemyToCombat(bool isBoss)
    {
        if (isBoss) bossesCount++;
        else normalEnemiesCount++;
    }

    // Los enemigos llaman a esta función cuando mueren o huyes muy lejos
    public void RemoveEnemyFromCombat(bool isBoss)
    {
        // Restamos 1 al contador, pero nos aseguramos de no bajar nunca de 0 (Mathf.Max)
        if (isBoss) bossesCount = Mathf.Max(0, bossesCount - 1);
        else normalEnemiesCount = Mathf.Max(0, normalEnemiesCount - 1);
    }

    // El cerebro del DJ: Decide qué disco debe sonar más alto
    private void UpdateCombatState()
    {
        // Prioridad 1: Si hay un jefe vivo, suena la música épica sí o sí
        if (bossesCount > 0)
        {
            currentState = MusicState.Boss;
            outOfCombatTimer = 0f; // Reseteamos el reloj de calma
        }
        // Prioridad 2: Si no hay jefe pero sí enemigos normales, suena la música de acción
        else if (normalEnemiesCount > 0)
        {
            currentState = MusicState.Combat;
            outOfCombatTimer = 0f;
        }
        // Prioridad 3: Si estamos solos y a salvo...
        else
        {
            // Si la música aún seguía tensa, empezamos una cuenta atrás de 2 segundos.
            // Así evitamos que la música cambie bruscamente si matas a un enemigo 
            // pero aparece otro de golpe justo un segundo después.
            if (currentState != MusicState.Normal)
            {
                outOfCombatTimer += Time.deltaTime;
                if (outOfCombatTimer >= outOfCombatDelay)
                {
                    currentState = MusicState.Normal; // Ya podemos relajarnos
                }
            }
        }
    }

    // Ejecuta físicamente la subida y bajada de los volúmenes (Fundido / Crossfade)
    private void HandleMusicFading()
    {
        // Calculamos el volumen máximo leyendo la barra de opciones del Menú Principal
        float maxVol = baseVolume * SoundEffectManager.MusicVolumeMultiplier;

        // Decidimos cuál es la meta de volumen para cada canción (o Todo o Nada)
        float targetNormal = (currentState == MusicState.Normal) ? maxVol : 0f;
        float targetCombat = (currentState == MusicState.Combat) ? maxVol : 0f;
        float targetBoss = (currentState == MusicState.Boss) ? maxVol : 0f;

        // Movemos las "ruedecitas" de volumen suavemente hacia su meta en cada fotograma
        if (normalMusic != null) normalMusic.volume = Mathf.MoveTowards(normalMusic.volume, targetNormal, fadeSpeed * Time.deltaTime);
        if (combatMusic != null) combatMusic.volume = Mathf.MoveTowards(combatMusic.volume, targetCombat, fadeSpeed * Time.deltaTime);
        if (bossMusic != null) bossMusic.volume = Mathf.MoveTowards(bossMusic.volume, targetBoss, fadeSpeed * Time.deltaTime);
    }
}