using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SceneStartManager : MonoBehaviour
{
    [Header("Configuración UI")]
    public Image fadeImage;
    public TextMeshProUGUI floorText;
    public string floorName = "Piso 1";

    [Header("Tiempos")]
    public float textFadeDuration = 1.0f; // Lo que tarda el texto en aparecer
    public float waitTime = 2.0f;         // Lo que está el texto en pantalla
    public float imageFadeDuration = 1.5f;// Lo que tarda la pantalla negra en desaparecer

    void Start()
    {
        // Al empezar la escena, iniciamos la rutina directamente
        StartCoroutine(StartSceneRoutine());
    }

    private IEnumerator StartSceneRoutine()
    {
        FadeTrigger.isTransitioning = true;

        // 1. Buscar al jugador y bloquear su movimiento
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement playerMovement = null;

        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null) playerMovement.canMove = false;
        }

        // 2. Nos aseguramos de que la pantalla esté negra y el texto invisible al inicio
        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 1f);
        floorText.color = new Color(floorText.color.r, floorText.color.g, floorText.color.b, 0f);
        floorText.text = floorName;

        // 3. Aparece el texto "Piso 1"
        yield return StartCoroutine(FadeTextAlpha(0f, 1f, textFadeDuration));

        // 4. Tiempo para que el jugador lo lea tranquilamente
        yield return new WaitForSeconds(waitTime);

        // 5. Desaparece el texto
        yield return StartCoroutine(FadeTextAlpha(1f, 0f, textFadeDuration));

        // 6. La pantalla negra se desvanece dejando ver el nivel
        yield return StartCoroutine(FadeImageAlpha(1f, 0f, imageFadeDuration));

        // 7. Le devolvemos el control al jugador para que empiece a jugar
        if (playerMovement != null) playerMovement.canMove = true;

        FadeTrigger.isTransitioning = false;
    }

    // Funciones auxiliares para la transición suave
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