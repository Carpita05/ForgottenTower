using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Este script controla la salud del jugador. 
// Es una de las piezas más complejas porque gestiona los corazones de la pantalla, 
// el tiempo de invulnerabilidad al recibir daño y el efecto visual de "tensión" al estar a punto de morir.
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Interfaz de Corazones")]
    public Image[] hearts;       // Array con las imágenes de los corazones en pantalla
    public Sprite fullHeart;     // Dibujo de corazón lleno
    public Sprite emptyHeart;    // Dibujo de corazón vacío

    [Header("Efecto Tensión (Alerta Visual)")]
    // Cuando al jugador le queda 1 punto de vida, los bordes de la pantalla parpadean en rojo.
    public GameObject redTensionBorder;
    public float tensionBlinkSpeed = 2f;  // Velocidad del latido/parpadeo
    public float maxTensionAlpha = 0.15f; // Lo fuerte (opaco) que se ve el color rojo
    public float tensionDuration = 20f;   // Cuántos segundos dura la alerta antes de desaparecer sola

    private Image tensionImage;
    private float currentTensionTimer = 0f;
    private bool canStartTension = true;

    [Header("Invulnerabilidad Temporal")]
    // Periodo de gracia para que no te maten de dos golpes instantáneos.
    public float iFramesDuration = 1.5f;
    private bool isInvulnerable = false;

    [Header("Muerte")]
    public GameObject deathParticlesPrefab; // Efecto visual al morir
    public GameManager gameManager;         // Referencia al director del juego para abrir la pantalla de fin

    [Header("Sonidos")]
    public string hurtSoundName = "PlayerHurt";
    public string deathSoundName = "PlayerDeath";

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Preparamos el borde rojo de tensión, pero lo dejamos oculto.
        if (redTensionBorder != null)
        {
            tensionImage = redTensionBorder.GetComponent<Image>();
            redTensionBorder.SetActive(false);
        }

        UpdateHealthUI();
    }

    void Update()
    {
        // --- CONTROL DEL EFECTO DE TENSIÓN ---
        // Si el temporizador de alerta está activo, hacemos latir el borde rojo
        if (currentTensionTimer > 0 && tensionImage != null)
        {
            currentTensionTimer -= Time.deltaTime;

            // Hacemos que la transparencia (Alpha) suba y baje usando matemáticas (PingPong)
            Color color = tensionImage.color;
            color.a = Mathf.Lerp(0f, maxTensionAlpha, Mathf.PingPong(Time.time * tensionBlinkSpeed, 1f));
            tensionImage.color = color;

            // Cuando se acaba el tiempo, lo volvemos totalmente transparente
            if (currentTensionTimer <= 0)
            {
                color.a = 0f;
                tensionImage.color = color;
            }
        }
    }

    // Función que reciben los enemigos u otros peligros para herir al jugador
    public void TakeDamage(int damage)
    {
        // Si estamos en nuestro periodo de gracia parpadeante, ignoramos el daño
        if (isInvulnerable) return;

        currentHealth -= damage;
        UpdateHealthUI(); // Actualizamos los corazones visuales

        // Si tenemos un sistema de vibración de cámara, le damos una sacudida
        if (ScreenShakeController.Instance != null)
        {
            ScreenShakeController.Instance.Shake(0.5f);
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
            // Activamos nuestra protección temporal
            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    // Revisa cuánta vida tenemos y dibuja los corazones correspondientes en la esquina de la pantalla
    private void UpdateHealthUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            // Si el corazón actual es menor a mi vida, lo pinto lleno. Si no, vacío.
            if (i < currentHealth) hearts[i].sprite = fullHeart;
            else hearts[i].sprite = emptyHeart;

            // Ocultamos corazones extra si la barra máxima se reduce por algún motivo
            hearts[i].enabled = i < maxHealth;
        }

        // --- ACTIVACIÓN DEL EFECTO DE TENSIÓN ---
        if (redTensionBorder != null)
        {
            if (currentHealth == 1) // Si estamos a un toque de morir...
            {
                redTensionBorder.SetActive(true);

                // Iniciamos la cuenta atrás de alerta si no lo habíamos hecho ya
                if (canStartTension)
                {
                    currentTensionTimer = tensionDuration;
                    canStartTension = false;
                }
            }
            else if (currentHealth > 1) // Si nos hemos curado...
            {
                // Apagamos la alarma y reseteamos el sistema
                redTensionBorder.SetActive(false);
                currentTensionTimer = 0f;
                canStartTension = true;

                if (tensionImage != null)
                {
                    Color resetColor = tensionImage.color;
                    resetColor.a = 0f;
                    tensionImage.color = resetColor;
                }
            }
        }
    }

    // Secuencia que hace parpadear al personaje en rojo y blanco tras recibir un golpe
    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;
        float timer = 0;

        while (timer < iFramesDuration)
        {
            // Nos pintamos de rojo semitransparente
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.8f);
            yield return new WaitForSeconds(0.1f);

            // Volvemos a nuestro color normal
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);

            timer += 0.2f;
        }

        // Nos aseguramos de quedar opacos y vulnerables de nuevo al terminar
        spriteRenderer.color = Color.white;
        isInvulnerable = false;
    }

    private void Die()
    {
        if (!string.IsNullOrEmpty(deathSoundName)) SoundEffectManager.Play(deathSoundName);

        // Hacemos que la pantalla tiemble mucho más fuerte
        if (ScreenShakeController.Instance != null) ScreenShakeController.Instance.Shake(2f);

        // Hacemos aparecer el efecto visual de "explosión" o humo
        if (deathParticlesPrefab != null) Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

        // Hacemos invisible al jugador y le quitamos la caja de físicas
        spriteRenderer.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        // Le quitamos los controles para que no siga pegando siendo un fantasma
        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerCombat>().enabled = false;

        // Llamamos a la pantalla negra de Game Over
        if (gameManager != null) gameManager.ShowGameOver();
    }

    // Función usada por las pociones (HealthPotionItem) para curarnos
    public void Heal(int healAmount)
    {
        currentHealth += healAmount;

        // Ponemos un tope para no tener más vida de la máxima permitida
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        UpdateHealthUI();
    }
}