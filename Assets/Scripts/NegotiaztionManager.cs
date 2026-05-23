using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NegotiationManager : MonoBehaviour
{
    public static NegotiationManager Instance;

    [Header("UI de Negociación")]
    public GameObject negotiationPanel;
    public TextMeshProUGUI dialogueText;
    public GameObject buttonsContainer;
    public Button spareButton;
    public Button killButton;

    [Header("Recompensas y Efectos")]
    public GameObject redPotionPrefab;
    public GameObject strengthPotionPrefab;
    public GameObject disappearParticles;

    // --- NUEVO: Control de velocidad desde el Inspector ---
    [Header("Ajustes de Texto")]
    public float typingSpeed = 0.015f; // 0.015 es el doble de rápido que lo que tenías (0.03)

    private EnemyHealth currentEnemy;
    private string loreIntro = "<i><color=#A854F7>Gracias al poder de Nicky, puedes entender lo que dice el monstruo...</color></i>\n\n";

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
        negotiationPanel.SetActive(false);

        spareButton.onClick.AddListener(SpareEnemy);
        killButton.onClick.AddListener(KillEnemy);
    }

    public void StartNegotiation(EnemyHealth enemy)
    {
        currentEnemy = enemy;

        PauseController.SetPause(true);

        negotiationPanel.SetActive(true);
        buttonsContainer.SetActive(false);

        string randomPhrase = monsterPhrases[Random.Range(0, monsterPhrases.Length)];
        StartCoroutine(TypewriterEffect(loreIntro + "\"" + randomPhrase + "\""));
    }

    // --- NUEVO: Máquina de escribir mejorada para TextMeshPro ---
    private IEnumerator TypewriterEffect(string fullText)
    {
        // 1. Le damos todo el texto de golpe (así TMPro lee y oculta el HTML al instante)
        dialogueText.text = fullText;

        // 2. Le decimos que no muestre nada todavía
        dialogueText.maxVisibleCharacters = 0;

        // 3. Forzamos a que calcule dónde va cada letra y color
        dialogueText.ForceMeshUpdate();

        // 4. Averiguamos cuántas letras reales hay (ignorando las etiquetas <color> y <i>)
        int totalCharacters = dialogueText.textInfo.characterCount;

        // 5. Vamos revelando las letras reales una a una a la velocidad marcada
        for (int i = 0; i <= totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        buttonsContainer.SetActive(true);
    }

    private void SpareEnemy()
    {
        PauseController.SetPause(false);
        negotiationPanel.SetActive(false);

        if (currentEnemy != null)
        {
            if (disappearParticles != null)
                Instantiate(disappearParticles, currentEnemy.transform.position, Quaternion.identity);

            float roll = Random.Range(0f, 100f);
            GameObject potionToDrop = (roll <= 70f) ? redPotionPrefab : strengthPotionPrefab;

            if (potionToDrop != null)
                Instantiate(potionToDrop, currentEnemy.transform.position, Quaternion.identity);

            if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.RemoveEnemyFromCombat(false);

            Destroy(currentEnemy.gameObject);
        }
    }

    private void KillEnemy()
    {
        PauseController.SetPause(false);
        negotiationPanel.SetActive(false);

        if (currentEnemy != null)
        {
            currentEnemy.EnrageForTriplePoints();
        }
    }
}