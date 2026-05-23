using System.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

// Herramienta global de fundido de pantalla.
// A diferencia de otros scripts, aquí aplicamos el paradigma de Programación Asíncrona moderna (async/await) 
// nativa de C# (.NET) en lugar del sistema tradicional de Corrutinas de Unity, logrando un código más limpio.
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;

    [SerializeField] CanvasGroup canvasGroup; // Controlador maestro de opacidad de la UI
    [SerializeField] float fadeDuration = 0.5f;
    [SerializeField] CinemachineConfiner2D vcam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Tarea asíncrona base que calcula la matemática del fundido
    async Task Fade(float targetTransparency)
    {
        float start = canvasGroup.alpha, t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            // Interpolación lineal entre la transparencia actual y el objetivo
            canvasGroup.alpha = Mathf.Lerp(start, targetTransparency, t / fadeDuration);

            // Task.Yield() pausa la ejecución hasta el siguiente frame de Unity 
            // (El equivalente moderno de 'yield return null')
            await Task.Yield();
        }

        // Nos aseguramos de clavar el valor exacto al terminar
        canvasGroup.alpha = targetTransparency;
    }

    // Fundido a Negro
    public async Task FadeOut()
    {
        await Fade(1); // 1 = 100% Opaco
    }

    // Fundido a Transparente
    public async Task FadeIn()
    {
        await Fade(0); // 0 = 0% Opaco (Invisible)
    }
}