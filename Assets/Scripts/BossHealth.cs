using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Este script controla la vida y el comportamiento del Jefe (Boss).
// Se encarga de mostrar su barra de vida, cambiar la música, calcular los puntos 
// y de guardar la partida para que el jefe no vuelva a aparecer una vez muerto.
public class BossHealth : MonoBehaviour
{
    [Header("Identificación")]
    // Un nombre único para este jefe. Sirve para que el sistema de guardado 
    // recuerde si ya lo hemos matado en el pasado.
    public string bossID = "Boss_01";

    [Header("Estadísticas del Jefe")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Interfaz de Usuario")]
    // Referencias a la barra de vida roja que aparece en pantalla durante la pelea.
    public GameObject bossHealthCanvas;
    public Image healthFill;

    [Header("Eventos de Victoria")]
    // Puerta o barrera física que se abrirá automáticamente cuando derrotemos al jefe.
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

        // Nos aseguramos de que la barra de vida esté oculta al cargar el nivel. 
        // Solo se mostrará cuando empiece el combate oficialmente.
        if (bossHealthCanvas != null) bossHealthCanvas.SetActive(false);
    }

    void Update()
    {
        // Cronómetro oculto para medir cuánto tardas en matar al jefe. 
        // Cuanto más tardes, menos puntos te dará al final.
        if (combatStarted && currentHealth > 0)
        {
            combatTimer += Time.deltaTime;
        }
    }

    // Función que se llama (normalmente al pisar una zona invisible o cruzar una puerta) 
    // para despertar al jefe y empezar la pelea.
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

            // Le avisamos al DJ (LevelMusicManager) de que quite la música normal 
            // y ponga la música épica de jefe.
            if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.AddEnemyToCombat(true);
        }
    }

    public void TakeDamage(int damage, Transform attacker = null)
    {
        // Si el jefe ya está muerto, ignoramos los golpes extra.
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // Actualizamos la barra roja de la pantalla para reflejar el daño.
        if (healthFill != null)
        {
            healthFill.fillAmount = (float)currentHealth / maxHealth;
        }

        // Comprobamos si el golpe ha sido letal.
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Si sigue vivo, hacemos que suene el grito de dolor.
            if (!string.IsNullOrEmpty(hurtSoundName))
            {
                SoundEffectManager.Play(hurtSoundName);
            }

            // Disparamos la animación de recibir daño.
            if (anim != null) anim.SetTrigger("Hurt");

            if (attacker != null) StartCoroutine(KnockbackRoutine(attacker));
        }
    }

    private IEnumerator KnockbackRoutine(Transform attacker)
    {
        if (rb != null)
        {
            // Desactivamos temporalmente su guión de perseguir para que no luche 
            // contra la fuerza del empujón.
            if (followScript != null) followScript.enabled = false;

            // Calculamos la dirección contraria al golpe para empujarlo hacia atrás.
            Vector2 direction = (transform.position - attacker.position).normalized;
            rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

            // Esperamos una fracción de segundo mientras es empujado.
            yield return new WaitForSeconds(knockbackDuration);

            // Frenamos al jefe en seco para que no siga resbalando por el suelo como si fuera hielo.
            rb.linearVelocity = Vector2.zero;

            // Le volvemos a dar permiso para perseguirnos.
            if (followScript != null) followScript.enabled = true;
        }
    }

    void Die()
    {
        if (!string.IsNullOrEmpty(deathSoundName))
        {
            SoundEffectManager.Play(deathSoundName);
        }

        // Le avisamos al sistema de música que el jefe ha caído, 
        // para que vuelva la canción de exploración tranquila.
        if (combatStarted)
        {
            if (LevelMusicManager.Instance != null) LevelMusicManager.Instance.RemoveEnemyFromCombat(true);
            combatStarted = false;
        }

        // Calculamos los puntos: le restamos puntos al jugador por cada segundo 
        // extra que haya tardado en matarlo (dándole 3 segundos de gracia).
        int finalPoints = basePoints;

        if (combatTimer > 3f)
        {
            float penaltyTime = combatTimer - 3f;
            finalPoints -= Mathf.RoundToInt(penaltyTime * pointsDecayPerSecond);
            finalPoints = Mathf.Max(finalPoints, minPoints);
        }

        // Enviamos la puntuación final a nuestro marcador.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(finalPoints);
        }

        // Disparamos la animación de muerte.
        if (anim != null) anim.SetTrigger("Death");

        // Ocultamos la barra de vida de la pantalla.
        if (bossHealthCanvas != null) bossHealthCanvas.SetActive(false);

        // Desactivamos su cuerpo y sus ataques para que el cadáver no nos haga daño 
        // ni sea un obstáculo físico.
        GetComponent<Collider2D>().enabled = false;

        if (followScript != null) followScript.enabled = false;

        EnemyDamage damageScript = GetComponent<EnemyDamage>();
        if (damageScript != null) damageScript.enabled = false;

        // Abrimos la puerta de la sala del jefe para poder avanzar.
        if (doorToOpen != null) doorToOpen.SetActive(false);

        // Guardamos en el archivo del PC que este jefe específico ha sido derrotado.
        // Así, si el jugador sale y vuelve a entrar, el jefe no resucitará.
        if (SaveController.Instance != null)
        {
            SaveController.Instance.RegisterBossDefeated(bossID);
        }

        // Por último, destruimos el objeto de la escena después de 2 segundos, 
        // dándole tiempo suficiente para que termine su animación de muerte.
        Destroy(gameObject, 2f);
    }
}