using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    [Header("Identificación")]
    public string bossID = "Boss_01";

    [Header("Estadísticas del Jefe")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Interfaz de Usuario")]
    public GameObject bossHealthCanvas;
    public Image healthFill;

    [Header("Eventos de Victoria")]
    public GameObject doorToOpen;

    [Header("Knockback")]
    public float knockbackForce = 2f;
    public float knockbackDuration = 0.15f;

    [Header("Sonidos")]
    public string hurtSoundName = "BossHurt";
    public string deathSoundName = "BossDeath";

    [Header("Sistema de Puntos")]
    public int basePoints = 500;
    public int pointsDecayPerSecond = 25;
    public int minPoints = 100;

    private bool combatStarted = false;
    private float combatTimer = 0f;

    private Animator anim;
    private Rigidbody2D rb;
    private EnemyFollow followScript;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        followScript = GetComponent<EnemyFollow>();

        if (bossHealthCanvas != null) bossHealthCanvas.SetActive(false);
    }

    void Update()
    {
        if (combatStarted && currentHealth > 0)
        {
            combatTimer += Time.deltaTime;
        }
    }

    public void ActivationBoss()
    {
        if (bossHealthCanvas != null && !combatStarted)
        {
            currentHealth = maxHealth;
            if (healthFill != null)
            {
                healthFill.fillAmount = 1f;
            }

            bossHealthCanvas.SetActive(true);
            combatStarted = true;
            Debug.Log("¡Combate de jefe iniciado! El tiempo corre para la puntuación.");

            // --- AVISO A LA MÚSICA ÉPICA ---
            if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.AddEnemyToCombat(true);
        }
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (healthFill != null)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (!string.IsNullOrEmpty(hurtSoundName))
            {
                SoundEffectManager.Play(hurtSoundName);
            }

            if (anim != null) anim.SetTrigger("Hurt");

            if (attacker != null) StartCoroutine(KnockbackRoutine(attacker));
        }
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
        if (!string.IsNullOrEmpty(deathSoundName))
        {
            SoundEffectManager.Play(deathSoundName);
        }

        if (combatStarted)
        {
            if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.RemoveEnemyFromCombat(true);
            combatStarted = false;
        }

        int finalPoints = basePoints;

        if (combatTimer > 3f)
        {
            float penaltyTime = combatTimer - 3f;
            finalPoints -= Mathf.RoundToInt(penaltyTime * pointsDecayPerSecond);
            finalPoints = Mathf.Max(finalPoints, minPoints);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(finalPoints);
        }

        if (anim != null) anim.SetTrigger("Death");

        if (bossHealthCanvas != null) bossHealthCanvas.SetActive(false);
        GetComponent<Collider2D>().enabled = false;

        if (followScript != null) followScript.enabled = false;

        EnemyDamage damageScript = GetComponent<EnemyDamage>();
        if (damageScript != null) damageScript.enabled = false;

        if (doorToOpen != null) doorToOpen.SetActive(false);

        if (SaveController.Instance != null)
        {
            SaveController.Instance.RegisterBossDefeated(bossID);
        }

        Destroy(gameObject, 2f);
    }
}