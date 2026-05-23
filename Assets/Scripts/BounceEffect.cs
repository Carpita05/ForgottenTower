using System.Collections;
using UnityEngine;

public class BounceEffect : MonoBehaviour
{
    public float bounceHeight = 0.3f; // La altura del rebote
    public float bounceDuration = 0.4f; // La duración del rebote
    public int bounceCount = 2; // El número de rebotes

    public void StartBounce()
    {
        StartCoroutine(BounceHandler());

    }

    private IEnumerator BounceHandler()
    {
        Vector3 startPosition = transform.position;
        float localHeight = bounceHeight;
        float localDuration = bounceDuration;

        for (int i = 0; i < bounceCount; i ++ )
        {
            yield return Bounce(startPosition, localHeight, localDuration / 2);
            localHeight *=0.5f; // Reducir la altura del rebote en cada iteración
            localDuration *= 0.5f; // Reducir la duración del rebote en cada iteración
        }
        transform.position = startPosition; // Asegurar que el objeto vuelva a su posición original

    }
    private IEnumerator Bounce(Vector3 start, float height, float duration)
    {
        Vector3 peak = start + Vector3.up * height; // Calcular el punto más alto del rebote
        float elapsed = 0f;

        // Subir
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(start, peak, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        // Bajar
        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(peak, start, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

}
