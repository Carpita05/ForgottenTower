using UnityEngine;

// Este script convierte cualquier objeto en un cofre del tesoro interactivo.
// Se encarga de comprobar si ya ha sido abierto, soltar el botín haciéndolo rebotar
// y cambiar su imagen para que el jugador vea que ya está vacío.
public class Chest : MonoBehaviour, IInteractable
{
    // Guarda si el cofre ya ha sido saqueado. Nadie más puede modificarlo desde fuera.
    public bool IsOpened { get; private set; }

    // Un "DNI" único para este cofre. Sirve para que el sistema de guardado 
    // recuerde si ya lo abrimos en una partida anterior.
    public string ChestID { get; private set; }

    [Header("Contenido y Aspecto")]
    public GameObject itemPrefab; // El objeto (botín) que saldrá del cofre
    public Sprite openedSprite;   // La imagen del cofre cuando está abierto y vacío

    // Al arrancar el nivel, le asignamos su identificador único al cofre
    // (si es que no tiene uno ya asignado previamente).
    void Start()
    {
        ChestID ??= GlobalHelper.GenerateUniqueID(gameObject);
    }

    // Comprobamos si el jugador tiene permiso para tocar el cofre.
    // Solo puede hacerlo si el cofre sigue cerrado.
    public bool CanInteract()
    {
        return !IsOpened;
    }

    // Esta es la acción que se dispara cuando el jugador se acerca y pulsa el botón de interactuar.
    public void Interact()
    {
        // Si el cofre ya estaba abierto, no hacemos nada y cancelamos la acción.
        if (!CanInteract()) return;

        OpenChest();
    }

    // El proceso físico y visual de abrir el cofre.
    private void OpenChest()
    {
        // 1. Lo marcamos como abierto para siempre
        SetOpened(true);

        // 2. Reproducimos el efecto de sonido (como el crujido de una bisagra)
        SoundEffectManager.Play("OpenChest");

        // 3. Si le pusimos un objeto de botín en el Inspector, lo creamos en el mundo
        if (itemPrefab)
        {
            // Lo hacemos aparecer un poquito más abajo de la posición del cofre
            GameObject droppedItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);

            // Le damos un pequeño empujón para que dé un saltito al salir (usando el script BounceEffect)
            droppedItem.GetComponent<BounceEffect>().StartBounce();
        }
    }

    // Esta función actualiza la "memoria" del cofre y su aspecto en pantalla.
    public void SetOpened(bool opened)
    {
        IsOpened = opened;

        // Si se acaba de abrir, cambiamos su dibujo (sprite) original por la imagen del cofre vacío.
        if (IsOpened)
        {
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}