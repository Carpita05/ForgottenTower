using System.Collections;
using UnityEngine;

// Este script se encarga de darle un efecto de "saltito" o rebote a los objetos 
// cuando caen al suelo (por ejemplo, cuando sueltas un botín del inventario).
public class BounceEffect : MonoBehaviour
{
    [Header("Configuración del Rebote")]
    public float bounceHeight = 0.3f;   // Cuánto de alto llega el primer bote
    public float bounceDuration = 0.4f; // Cuánto tiempo tarda en dar el bote
    public int bounceCount = 2;         // Cuántos botes da antes de quedarse quieto

    // Esta función es la que llamamos desde otros scripts para que el objeto empiece a saltar.
    public void StartBounce()
    {
        StartCoroutine(BounceHandler());
    }

    // Este es el "director" de los rebotes. Controla cuántos saltos da y 
    // hace que cada salto sea más pequeño y rápido que el anterior (perdiendo fuerza).
    private IEnumerator BounceHandler()
    {
        // Guardamos el punto exacto donde el objeto tocó el suelo
        Vector3 startPosition = transform.position;

        float localHeight = bounceHeight;
        float localDuration = bounceDuration;

        for (int i = 0; i < bounceCount; i++)
        {
            // Hacemos que el objeto suba y baje
            yield return Bounce(startPosition, localHeight, localDuration / 2);

            // Para el siguiente bote, la altura y el tiempo serán la mitad (la gravedad hace su efecto)
            localHeight *= 0.5f;
            localDuration *= 0.5f;
        }

        // Al terminar todos los botes, nos aseguramos de que el objeto quede posado firmemente en el suelo
        transform.position = startPosition;
    }

    // Esta función hace el movimiento físico del salto (subir hasta el pico y volver a bajar)
    private IEnumerator Bounce(Vector3 start, float height, float duration)
    {
        // Calculamos cuál es el punto más alto en el aire
        Vector3 peak = start + Vector3.up * height;
        float elapsed = 0f;

        // 1. Fase de subida
        while (elapsed < duration)
        {
            // Movemos el objeto suavemente desde el suelo hasta el punto más alto
            transform.position = Vector3.Lerp(start, peak, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        // 2. Fase de bajada
        while (elapsed < duration)
        {
            // Movemos el objeto suavemente desde el punto más alto hasta el suelo
            transform.position = Vector3.Lerp(peak, start, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}