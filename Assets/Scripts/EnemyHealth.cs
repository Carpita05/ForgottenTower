using UnityEngine;
using System.Collections;

// Este script controla la vida de los enemigos normales.
// Además de gestionar el daño y los puntos, incluye la mecánica estrella del juego:
// ¡El sistema de negociación! Si le haces mucho daño, el enemigo puede pedirte piedad.
public class EnemyHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Knockback (Empujón al recibir daño)")]
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;

    [Header("Sonidos")]
    public string hurtSoundName = "EnemyHurt";
    public string deathSoundName = "EnemyDeath";

    [Header("Sistema de Puntos")]
    public int basePoints = 100;
    public int pointsDecayPerSecond = 15;
    public int minPoints = 20;

    private bool playerDetected = false;
    private float detectionTimer = 0f;
    private Transform playerTransform;

    private Animator anim;
    private Rigidbody2D rb;
    private EnemyFollow followScript;
    private SpriteRenderer spriteRenderer;

    // --- Variables del Sistema de Negociación ---
    // Un seguro para que el enemigo solo intente rendirse una sola vez por combate.
    private bool hasNegotiated = false;
    // Si rechazamos la paz, esta variable se activa para darnos el triple de puntos al matarlo.
    private bool givesTriplePoints = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        followScript = GetComponent<EnemyFollow>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Buscamos automáticamente al jugador al empezar.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        // Control de distancias y música dinámica.
        if (playerTransform != null && followScript != null && currentHealth > 0)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            // Si el jugador entra en nuestro rango visual, avisamos al sistema de música
            // para que cambie a la canción de combate.
            if (!playerDetected && distance <= followScript.detectionRange)
            {
                playerDetected = true;
                if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.AddEnemyToCombat(false);
            }
            // Si el jugador huye y se aleja lo suficiente, volvemos a la música tranquila.
            else if (playerDetected && distance > followScript.detectionRange + 2f)
            {
                playerDetected = false;
                if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.RemoveEnemyFromCombat(false);
            }
        }

        // Cronómetro para penalizar los puntos si tardamos mucho en matarlo.
        if (playerDetected && currentHealth > 0)
        {
            detectionTimer += Time.deltaTime;
        }
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // --- SISTEMA DE NEGOCIACIÓN ---
        // Si la vida del enemigo baja a la mitad o menos (pero sigue vivo),
        // y aún no ha intentado rendirse en este combate:
        if (currentHealth > 0 && currentHealth <= (maxHealth / 2) && !hasNegotiated)
        {
            // Marcamos que ya ha intentado negociar para que no lo haga con cada golpe.
            hasNegotiated = true;

            // Tiramos un dado invisible del 0 al 100.
            // Hay un 10% de probabilidades de que el enemigo pida piedad (Poder de Nicky).
            if (Random.Range(0f, 100f) <= 10f)
            {
                if (NegotiationManager.Instance != null)
                {
                    // Congelamos el combate y abrimos la ventana de diálogo para decidir su destino.
                    NegotiationManager.Instance.StartNegotiation(this);
                    SoundEffectManager.Play("NegociationSound");
                    return; // Cancelamos el empujón hacia atrás para que el juego se pause limpiamente y en el sitio.
                }
            }
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (!string.IsNullOrEmpty(hurtSoundName)) SoundEffectManager.Play(hurtSoundName);
            if (anim != null) anim.SetTrigger("Hurt");
            if (attacker != null) StartCoroutine(KnockbackRoutine(attacker));
        }
    }

    // --- Función para cuando rechazamos la paz ---
    // Si decidimos no perdonarle la vida, el enemigo se enfada.
    public void EnrageForTriplePoints()
    {
        givesTriplePoints = true;
        // Pintamos el dibujo del enemigo de un tono rojizo para que el jugador vea que está furioso.
        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.6f, 0.6f);
    }

    // Función que empuja al enemigo hacia atrás al recibir daño.
    private IEnumerator KnockbackRoutine(Transform attacker)
    {
        if (rb != null)
        {
            if (followScript != null) followScript.enabled = false;
            Vector2 direction = (transform.position - attacker.position).normalized;
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
            yield return new WaitForSeconds(knockbackDuration);
            rb.linearVelocity = Vector2.zero;
            if (followScript != null) followScript.enabled = true;
        }
    }

    void Die()
    {
        if (!string.IsNullOrEmpty(deathSoundName)) SoundEffectManager.Play(deathSoundName);

        // Si morimos, le decimos a la música que hay un enemigo menos persiguiéndonos.
        if (playerDetected)
        {
            if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.RemoveEnemyFromCombat(false);
            playerDetected = false;
        }

        int finalPoints = basePoints;

        // Restamos puntos si el combate duró más de 3 segundos de tiempo efectivo.
        if (detectionTimer > 3f)
        {
            float penaltyTime = detectionTimer - 3f;
            finalPoints -= Mathf.RoundToInt(penaltyTime * pointsDecayPerSecond);
            finalPoints = Mathf.Max(finalPoints, minPoints);
        }

        // ¡PREMIO POR SER CRUEL! Si lo matamos después de rechazar su rendición, la puntuación obtenida se multiplica por 3.
        if (givesTriplePoints)
        {
            finalPoints *= 3;
            Debug.Log("¡Puntos x3 aplicados por ejecución!");
        }

        if (ScoreManager.Instance != null) ScoreManager.Instance.AddPoints(finalPoints);

        if (anim != null) anim.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        if (followScript != null) followScript.enabled = false;
        if (GetComponent<EnemyDamage>() != null) GetComponent<EnemyDamage>().enabled = false;

        Destroy(gameObject, 1f);
    }
}