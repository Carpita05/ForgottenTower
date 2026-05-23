using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

// Este script es el cerebro de la mecánica estrella: ¡La Negociación!
// Congela el juego, muestra una conversación estilo máquina de escribir
// y te permite decidir el destino del enemigo (Perdonar o Eliminar).
public class NegotiationManager : MonoBehaviour
{
    public static NegotiationManager Instance;

    [Header("UI de Negociación")]
    public GameObject negotiationPanel;  // El cuadro oscuro que aparece al negociar
    public TextMeshProUGUI dialogueText; // El texto de la conversación
    public GameObject buttonsContainer;  // La caja que agrupa los botones
    public Button spareButton;           // El botón de "Perdonar"
    public Button killButton;            // El botón de "Eliminar"

    [Header("Recompensas y Efectos")]
    public GameObject redPotionPrefab;       // Premio 1: Poción de Vida
    public GameObject strengthPotionPrefab;  // Premio 2: Poción de Fuerza
    public GameObject disappearParticles;    // Efecto visual cuando el enemigo huye

    [Header("Ajustes de Texto")]
    // Cuánto tiempo pasa entre letra y letra (hace que parezca que alguien está escribiendo)
    public float typingSpeed = 0.015f;

    private EnemyHealth currentEnemy; // Recordamos con qué enemigo estamos hablando

    // Texto fijo que aparece siempre al principio, coloreado usando etiquetas HTML
    private string loreIntro = "<i><color=#A854F7>Gracias al poder de Nicky, puedes entender lo que dice el monstruo...</color></i>\n\n";

    // Lista de frases aleatorias que el monstruo puede decirnos para pedir piedad
    private string[] monsterPhrases = {
        "Humano, me aferro a tu piedad. Perdóname, te dejaré en paz y a cambio te daré algo útil.",
        "¡Espera, por favor! No quiero morir. Te daré mis pertenencias si me dejas ir.",
        "Me rindo... Eres demasiado fuerte. Toma esto y déjame vivir, te lo ruego."
    };

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Escondemos la ventana de negociación al principio
        negotiationPanel.SetActive(false);

        // Conectamos los botones a sus funciones correspondientes
        spareButton.onClick.AddListener(SpareEnemy);
        killButton.onClick.AddListener(KillEnemy);
    }

    // Esta función se llama desde el script EnemyHealth cuando la vida del monstruo baja del 50%
    public void StartNegotiation(EnemyHealth enemy)
    {
        currentEnemy = enemy;

        // Congelamos el tiempo del juego para que no nos ataquen mientras leemos
        PauseController.SetPause(true);

        // Mostramos la ventana y escondemos los botones hasta que el texto termine de escribirse
        negotiationPanel.SetActive(true);
        buttonsContainer.SetActive(false);

        // Elegimos una frase al azar y arrancamos el efecto de máquina de escribir
        string randomPhrase = monsterPhrases[Random.Range(0, monsterPhrases.Length)];
        StartCoroutine(TypewriterEffect(loreIntro + "\"" + randomPhrase + "\""));
    }

    // Efecto visual que revela el texto letra por letra
    private IEnumerator TypewriterEffect(string fullText)
    {
        // 1. Cargamos todo el texto de golpe (así Unity oculta el código de colores HTML)
        dialogueText.text = fullText;

        // 2. Le decimos que esconda todas las letras al principio
        dialogueText.maxVisibleCharacters = 0;

        // 3. Forzamos al programa a colocar invisiblemente cada letra en su sitio
        dialogueText.ForceMeshUpdate();

        // 4. Averiguamos cuántas letras reales hay para saber hasta dónde contar
        int totalCharacters = dialogueText.textInfo.characterCount;

        // 5. Vamos mostrando una letra más en cada ciclo y esperamos una fracción de segundo
        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;

            // Usamos Realtime porque el tiempo normal del juego (Time.deltaTime) está congelado
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        // 6. Al terminar de escribir, mostramos los botones para que el jugador elija
        buttonsContainer.SetActive(true);
    }

    // DECISIÓN: PERDONAR (SPARE)
    private void SpareEnemy()
    {
        PauseController.SetPause(false); // Volvemos a poner el juego en marcha
        negotiationPanel.SetActive(false); // Cerramos el panel

        if (currentEnemy != null)
        {
            // 1. Efecto visual de desvanecimiento
            if (disappearParticles != null)
                Instantiate(disappearParticles, currentEnemy.transform.position, Quaternion.identity);

            // 2. Tiramos los dados: 70% de probabilidad de Poción Roja, 30% de Poción de Fuerza
            float roll = Random.Range(0f, 100f);
            GameObject potionToDrop = (roll <= 70f) ? redPotionPrefab : strengthPotionPrefab;

            // 3. Hacemos aparecer el premio donde estaba el enemigo
            if (potionToDrop != null)
                Instantiate(potionToDrop, currentEnemy.transform.position, Quaternion.identity);

            // 4. Avisamos al gestor de música de que la pelea ha terminado
            if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.RemoveEnemyFromCombat(false);

            // 5. Borramos al enemigo para siempre (sin darle puntos al jugador por haber sido pacífico)
            Destroy(currentEnemy.gameObject);
        }
    }

    // DECISIÓN: ELIMINAR (KILL)
    private void KillEnemy()
    {
        PauseController.SetPause(false); // Descongelamos el juego
        negotiationPanel.SetActive(false); // Cerramos el panel

        if (currentEnemy != null)
        {
            // Al rechazar la paz, el enemigo entra en estado de furia, se pinta de rojo y la pelea sigue
            currentEnemy.EnrageForTriplePoints();
        }
    }
}