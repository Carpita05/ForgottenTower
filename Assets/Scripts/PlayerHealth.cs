using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración de Vida")]
    public int maxHealth = 3;
    public int currentHealth;

    [Header("Interfaz de Corazones")]
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Efecto Tensión")]
    public GameObject redTensionBorder;
    public float tensionBlinkSpeed = 2f;
    public float maxTensionAlpha = 0.15f;
    public float tensionDuration = 20f;

    private Image tensionImage;
    private float currentTensionTimer = 0f;
    private bool canStartTension = true;

    [Header("Invulnerabilidad (Cooldown)")]
    public float iFramesDuration = 1.5f;
    private bool isInvulnerable = false;

    [Header("Muerte")]
    public GameObject deathParticlesPrefab;
    public GameManager gameManager;

    [Header("Sonidos")]
    public string hurtSoundName = "PlayerHurt";
    public string deathSoundName = "PlayerDeath";

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (redTensionBorder != null)
        {
            tensionImage = redTensionBorder.GetComponent<Image>();
            redTensionBorder.SetActive(false);
        }

        UpdateHealthUI();
    }

    void Update()
    {
        if (currentTensionTimer > 0 && tensionImage != null)
        {
            currentTensionTimer -= Time.deltaTime;

            Color color = tensionImage.color;
            color.a = Mathf.Lerp(0f, maxTensionAlpha, Mathf.PingPong(Time.time * tensionBlinkSpeed, 1f));
            tensionImage.color = color;

            if (currentTensionTimer <= 0)
            {
                color.a = 0f;
                tensionImage.color = color;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        UpdateHealthUI();

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

            StartCoroutine(InvulnerabilityCoroutine());
        }
    }

    private void UpdateHealthUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHealth) hearts[i].sprite = fullHeart;
            else hearts[i].sprite = emptyHeart;

            hearts[i].enabled = i < maxHealth;
        }

        if (redTensionBorder != null)
        {
            if (currentHealth == 1)
            {
                redTensionBorder.SetActive(true);

                if (canStartTension)
                {
                    currentTensionTimer = tensionDuration;
                    canStartTension = false;
                }
            }
            else if (currentHealth > 1)
            {
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

    private IEnumerator InvulnerabilityCoroutine()
    {
        isInvulnerable = true;

        float timer = 0;
        while (timer < iFramesDuration)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0.5f, 0.8f);
            yield return new WaitForSeconds(0.1f);

            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);

            timer += 0.2f;
        }

        spriteRenderer.color = Color.white;
        isInvulnerable = false;
    }

    private void Die()
    {
        if (!string.IsNullOrEmpty(deathSoundName))
        {
            SoundEffectManager.Play(deathSoundName);
        }

        if (ScreenShakeController.Instance != null)
        {
            ScreenShakeController.Instance.Shake(2f);
        }

        if (deathParticlesPrefab != null) Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

        spriteRenderer.enabled = false;
        GetComponent<Collider2D>().enabled = false;

        GetComponent<PlayerMovement>().enabled = false;
        GetComponent<PlayerCombat>().enabled = false;

        if (gameManager != null) gameManager.ShowGameOver();
    }

    public void Heal(int healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        UpdateHealthUI();
    }
}