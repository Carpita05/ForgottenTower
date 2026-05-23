using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Este script es el "Director de Escena" al empezar un nivel nuevo.
// Se encarga de hacer una transición suave desde una pantalla negra, 
// mostrar en qué piso estamos y, finalmente, darle el control al jugador.
public class SceneStartManager : MonoBehaviour
{
    [Header("Configuración UI")]
    public Image fadeImage;             // La imagen negra que tapa toda la pantalla
    public TextMeshProUGUI floorText;   // El texto del título (Ej: "Piso 1")
    public string floorName = "Piso 1";

    [Header("Tiempos de Transición")]
    public float textFadeDuration = 1.0f;  // Lo que tarda el texto en aparecer de la nada
    public float waitTime = 2.0f;          // El tiempo que dejamos el texto en pantalla para leerlo
    public float imageFadeDuration = 1.5f; // Lo que tarda la pantalla negra en volverse transparente

    void Start()
    {
        // Nada más arrancar la escena, iniciamos la película de introducción
        StartCoroutine(StartSceneRoutine());
    }

    private IEnumerator StartSceneRoutine()
    {
        // 1. Avisamos al sistema global de que estamos en transición para bloquear menús
        FadeTrigger.isTransitioning = true;

        // 2. Buscamos al jugador y bloqueamos sus controles.
        // Así evitamos que empiece a caminar a ciegas mientras la pantalla sigue negra.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement playerMovement = null;

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.canMove = false;
        }

        // 3. Preparamos el escenario: Pantalla 100% negra y Texto 100% invisible
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f);
        floorText.color = new Color(floorText.color.r, floorText.color.g, floorText.color.b, 0f);
        floorText.text = floorName;

        // 4. Hacemos aparecer el texto suavemente ("Piso 1")
        yield return StartCoroutine(FadeTextAlpha(0f, 1f, textFadeDuration));

        // 5. Dejamos pasar unos segundos de silencio para crear atmósfera
        yield return new WaitForSeconds(waitTime);

        // 6. El texto se desvanece
        yield return StartCoroutine(FadeTextAlpha(1f, 0f, textFadeDuration));

        // 7. La pantalla negra desaparece como si se abriera el telón, mostrando el juego
        yield return StartCoroutine(FadeImageAlpha(1f, 0f, imageFadeDuration));

        // 8. ¡Acción! Le devolvemos el control al jugador
        if (playerMovement != null) playerMovement.canMove = true;
        FadeTrigger.isTransitioning = false;
    }

    // --- FUNCIONES MATEMÁTICAS AUXILIARES ---

    // Transición suave de transparencia para la pantalla negra
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
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, end);
    }

    // Transición suave de transparencia para las letras
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
        floorText.color = new Color(floorText.color.r, floorText.color.g, floorText.color.b, end);
    }
}