using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Este script es el Gestor de Notificaciones (Singleton).
// Cada vez que recoges un objeto, hace aparecer un pequeño cartel en la esquina 
// de la pantalla con la foto y el nombre del objeto, y luego lo desvanece suavemente.
public class ItemPickUpUIController : MonoBehaviour
{
    // Hacemos que sea accesible desde cualquier parte del juego
    public static ItemPickUpUIController Instance { get; private set; }

    [Header("Configuración Visual")]
    public GameObject popupPrefab;  // El molde visual del cartelito de notificación
    public int maxPopups = 5;       // Límite máximo de carteles que pueden verse a la vez
    public float popupDuration = 2f;// Cuántos segundos se queda visible cada cartel

    // Una "cola" (Queue) funciona como la cola del supermercado: 
    // el primero que entra es el primero que sale. Nos sirve para borrar los carteles más viejos.
    private readonly Queue<GameObject> activePopups = new();

    private void Awake()
    {
        // Nos aseguramos de que solo haya un gestor de notificaciones en todo el juego
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Error: ¡Múltiples instancias de ItemPickUpUIController detectadas! Destruyendo el sobrante.");
            Destroy(gameObject);
        }
    }

    // Esta función la llaman los objetos (Item.cs) justo al ser recogidos del suelo
    public void ShowItemPickup(string itemName, Sprite itemIcon)
    {
        // 1. Creamos un nuevo cartelito en la pantalla
        GameObject newPopup = Instantiate(popupPrefab, transform);

        // 2. Le escribimos el nombre del objeto
        newPopup.GetComponentInChildren<TMP_Text>().text = itemName;

        // 3. Le ponemos la foto (icono) correspondiente
        Image itemImage = newPopup.transform.Find("ItemIcon")?.GetComponent<Image>();
        if (itemImage)
        {
            itemImage.sprite = itemIcon;
        }

        // 4. Lo metemos en nuestra "cola" de carteles activos
        activePopups.Enqueue(newPopup);

        // 5. Si la pantalla se está llenando de carteles (más del límite permitido),
        // cogemos el más antiguo de la cola y lo destruimos para dejar espacio.
        if (activePopups.Count > maxPopups)
        {
            Destroy(activePopups.Dequeue());
        }

        // 6. Arrancamos el temporizador para que este nuevo cartel desaparezca solo al rato
        StartCoroutine(FadeOutAndDestroy(newPopup));
    }

    // Esta función hace que el cartel se vuelva transparente poco a poco antes de borrarse
    private IEnumerator FadeOutAndDestroy(GameObject popup)
    {
        // Esperamos un par de segundos con el cartel totalmente visible para que el jugador lo lea
        yield return new WaitForSeconds(popupDuration);

        if (popup == null) yield break;

        // Herramienta para modificar la transparencia de todo el cartel a la vez
        CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();

        // Bucle que va bajando la visibilidad de 1 (opaco) a 0 (transparente) a lo largo de 1 segundo
        for (float timePassed = 0f; timePassed < 1f; timePassed += Time.deltaTime)
        {
            if (popup == null) yield break;

            canvasGroup.alpha = 1f - timePassed;
            yield return null; // Esperamos al siguiente fotograma
        }

        // Una vez es totalmente invisible, lo destruimos para liberar memoria del ordenador
        Destroy(popup);
    }
}