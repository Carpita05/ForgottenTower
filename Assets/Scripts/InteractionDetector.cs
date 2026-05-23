using UnityEngine;
using UnityEngine.InputSystem;

// Este script funciona como las "manos" y los "ojos" del jugador.
// Detecta cuándo nos acercamos a un objeto con el que podemos interactuar 
// (como un cofre, un NPC o una puerta) y nos muestra un icono de aviso.
public class InteractionDetector : MonoBehaviour
{
    // Guarda en la memoria cuál es el objeto interactivo que tenemos justo delante.
    private IInteractable interactableInRange = null;

    // El dibujo (bocadillo o tecla) que aparece sobre la cabeza del jugador para avisarle.
    public GameObject interactionIcon;

    void Start()
    {
        // Al empezar a jugar, escondemos el icono porque aún no estamos cerca de nada.
        interactionIcon.SetActive(false);
    }

    // Esta función se dispara automáticamente cuando el jugador pulsa la tecla de interactuar 
    // (configurada en el Input System, por ejemplo, la tecla 'E' o el botón 'Sur' del mando).
    public void OnInteract(InputAction.CallbackContext context)
    {
        // Comprobamos si la tecla se ha pulsado hasta el fondo (performed)
        if (context.performed)
        {
            // Si tenemos un objeto interactivo delante, le damos la orden de actuar (ej: abrir cofre)
            interactableInRange?.Interact();

            // Si después de interactuar, el objeto ya no se puede usar más (ej: un cofre ya abierto),
            // apagamos el icono de aviso.
            if (!interactableInRange.CanInteract())
            {
                interactionIcon.SetActive(false);
            }
        }
    }

    // --- ZONA DE SENSORES FÍSICOS ---

    // Cuando el jugador entra en el espacio vital de otro objeto...
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Le preguntamos al objeto: "¿Eres algo con lo que puedo interactuar?"
        // Si responde que sí, y además está disponible para usarse...
        if (collision.TryGetComponent(out IInteractable interactable) && interactable.CanInteract())
        {
            // Lo fijamos como nuestro objetivo actual y encendemos el icono de aviso.
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    // Cuando el jugador se aleja y sale del espacio vital del objeto...
    private void OnTriggerExit2D(Collider2D collision)
    {
        // Comprobamos si el objeto del que nos alejamos es el mismo que teníamos fijado.
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            // Lo borramos de nuestra memoria y apagamos el icono de aviso.
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}