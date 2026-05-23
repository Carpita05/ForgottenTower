using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Este script crea un efecto de "parpadeo largo" (fundido a negro) cuando el jugador 
// cambia de zona. Aprovecha ese fundido para mostrar el nombre del nuevo piso.
public class FadeTrigger : MonoBehaviour
{
    // Una señal global para todo el juego. Si está en 'true', le dice a otros sistemas 
    // (como el menú de pausa) que no se abran porque estamos a mitad de una transición.
    public static bool isTransitioning = false;

    [Header("Configuración del Fundido (Fade)")]
    public Image fadeImage;             // La imagen negra que tapará la pantalla
    public float fadeOutDuration = 1.5f;// Cuánto tarda en oscurecerse
    public float waitTime = 2.0f;       // Tiempo que la pantalla se queda totalmente negra
    public float fadeInDuration = 1.5f; // Cuánto tarda en volver a verse el juego

    [Header("Configuración del Texto de Piso")]
    public TextMeshProUGUI floorText;   // El texto de la pantalla donde escribiremos el piso
    public string floorName = "Piso 2"; // El nombre que queremos que aparezca
    public float textFadeDuration = 0.8f; // Cuánto tardan las letras en aparecer y desaparecer

    private bool isFading = false;

    // Cuando el jugador pisa esta zona invisible...
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isFading)
        {
            isFading = true;
            // Arrancamos la secuencia de la película paso a paso
            StartCoroutine(FadeSequenceRoutine(collision.gameObject));
        }
    }

    // Esta es la secuencia cronológica de lo que ocurre durante la transición
    private IEnumerator FadeSequenceRoutine(GameObject player)
    {
        // 1. Levantamos la barrera: avisamos de que estamos en transición para bloquear menús
        isTransitioning = true;

        // 2. Le quitamos los controles al jugador para que no caiga en trampas caminando a ciegas
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.canMove = false;

        // 3. FASE OSCURIDAD: La pantalla se va tiñendo de negro poco a poco
        yield return StartCoroutine(FadeImageAlpha(0, 1, fadeOutDuration));

        // 4. FASE LECTURA: Aparece el texto flotante con el nombre de la zona (ej: "Piso 2")
        if (floorText != null)
        {
            floorText.text = floorName;
            yield return StartCoroutine(FadeTextAlpha(0, 1, textFadeDuration));
        }

        // 5. Dejamos que el jugador lea el texto en la oscuridad durante un par de segundos
        yield return new WaitForSeconds(waitTime);

        // 6. Ocultamos el texto suavemente
        if (floorText != null)
        {
            yield return StartCoroutine(FadeTextAlpha(1, 0, textFadeDuration));
        }

        // 7. FASE LUZ: La pantalla negra se vuelve transparente y volvemos a ver el juego
        yield return StartCoroutine(FadeImageAlpha(1, 0, fadeInDuration));

        // 8. Le devolvemos los controles al jugador y bajamos la barrera del menú
        if (playerMovement != null) playerMovement.canMove = true;
        isTransitioning = false;
        isFading = false;
    }

    // --- Funciones Matemáticas Auxiliares ---

    // Cambia la transparencia (Alpha) de la imagen negra poco a poco usando Interpolación (Lerp)
    private IEnumerator FadeImageAlpha(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsed / duration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return null; // Esperamos al siguiente frame de Unity
        }
    }

    // Hace exactamente lo mismo, pero aplicado a las letras del texto
    private IEnumerator FadeTextAlpha(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, elapsed / duration);
            floorText.color = new Color(floorText.color.r, floorText.color.g, floorText.color.b, alpha);
            yield return null;
        }
    }
}