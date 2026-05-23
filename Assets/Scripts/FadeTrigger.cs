using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FadeTrigger : MonoBehaviour
{
    // --- NUEVO: Variable global (static) para avisarle al menú de que estamos ocupados ---
    public static bool isTransitioning = false;

    [Header("Configuración del Fade")]
    public Image fadeImage;
    public float fadeOutDuration = 1.5f;
    public float waitTime = 2.0f; // He subido un poco el tiempo para que dé tiempo a leer
    public float fadeInDuration = 1.5f;

    [Header("Configuración del Texto de Piso")]
    public TextMeshProUGUI floorText; // Arrastra aquí el nuevo FloorText
    public string floorName = "Piso 2"; // Escribe aquí el nombre de la zona en el Inspector
    public float textFadeDuration = 0.8f; // Cuánto tarda el texto en aparecer/desaparecer

    private bool isFading = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isFading)
        {
            isFading = true;
            StartCoroutine(FadeSequenceRoutine(collision.gameObject));
        }
    }

    private IEnumerator FadeSequenceRoutine(GameObject player)
    {
        // --- NUEVO: Ponemos la señal en rojo (bloqueamos el menú) ---
        isTransitioning = true;

        // 1. BLOQUEAR AL JUGADOR
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.canMove = false;

        // --- 2. FADE OUT (Pantalla a Negro) ---
        yield return StartCoroutine(FadeImageAlpha(0, 1, fadeOutDuration));

        // --- 3. MOSTRAR TEXTO DEL PISO ---
        if (floorText != null)
        {
            floorText.text = floorName; // Asignamos el nombre
            yield return StartCoroutine(FadeTextAlpha(0, 1, textFadeDuration));
        }

        // --- 4. TIEMPO DE ESPERA EN NEGRO ---
        // Esperamos el tiempo configurado menos lo que tardó el texto en aparecer
        yield return new WaitForSeconds(waitTime);

        // --- 5. OCULTAR TEXTO DEL PISO ---
        if (floorText != null)
        {
            yield return StartCoroutine(FadeTextAlpha(1, 0, textFadeDuration));
        }

        // --- 6. FADE IN (Pantalla a Transparente) ---
        yield return StartCoroutine(FadeImageAlpha(1, 0, fadeInDuration));

        // 7. DEVOLVER EL MOVIMIENTO AL JUGADOR
        if (playerMovement != null) playerMovement.canMove = true;

        isTransitioning = false;

        isFading = false;
    }

    // Función auxiliar para fundir la IMAGEN
    private IEnumerator FadeImageAlpha(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(start, end, elapsed / duration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, a);
            yield return null;
        }
    }

    // Función auxiliar para fundir el TEXTO
    private IEnumerator FadeTextAlpha(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(start, end, elapsed / duration);
            floorText.color = new Color(floorText.color.r, floorText.color.g, floorText.color.b, a);
            yield return null;
        }
    }
}