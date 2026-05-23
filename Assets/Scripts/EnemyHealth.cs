using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Knockback")]
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

    // --- NUEVO: Variables de Negociación ---
    private bool hasNegotiated = false;
    private bool givesTriplePoints = false;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        followScript = GetComponent<EnemyFollow>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform != null && followScript != null && currentHealth > 0)
        {
            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (!playerDetected && distance <= followScript.detectionRange)
            {
                playerDetected = true;
                if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.AddEnemyToCombat(false);
            }
            else if (playerDetected && distance > followScript.detectionRange + 2f)
            {
                playerDetected = false;
                if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.RemoveEnemyFromCombat(false);
            }
        }

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
        // Si la vida baja de la mitad (y sigue vivo), y no ha negociado antes:
        if (currentHealth > 0 && currentHealth <= (maxHealth / 2) && !hasNegotiated)
        {
            hasNegotiated = true; // Solo intentará negociar 1 vez por combate

            // 10% de probabilidad (Para hacer pruebas en Unity ponlo temporalmente a 100f)
            if (Random.Range(0f, 100f) <= 10f)
            {
                if (NegotiationManager.Instance != null)
                {
                    NegotiationManager.Instance.StartNegotiation(this);
                    SoundEffectManager.Play("NegociationSound");
                    return; // Detenemos el knockback para que el juego se pause limpiamente
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

    // --- NUEVO: Función para cuando rechazas la paz ---
    public void EnrageForTriplePoints()
    {
        givesTriplePoints = true;
        // Pinta al enemigo de rojo sutilmente para indicar que está furioso por rechazarle
        if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.6f, 0.6f);
    }

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

        if (playerDetected)
        {
            if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.RemoveEnemyFromCombat(false);
            playerDetected = false;
        }

        int finalPoints = basePoints;

        if (detectionTimer > 3f)
        {
            float penaltyTime = detectionTimer - 3f;
            finalPoints -= Mathf.RoundToInt(penaltyTime * pointsDecayPerSecond);
            finalPoints = Mathf.Max(finalPoints, minPoints);
        }

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