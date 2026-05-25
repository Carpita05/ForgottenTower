using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Este es el "Molde Maestro" para cualquier cosa que se pueda recoger en el juego.
public class Item : MonoBehaviour
{
    [Header("Datos del Objeto")]
    public int ID;               // El código de barras único de este objeto
    public string itemName;      // El nombre real que leerá el jugador
    public int quantity = 1;     // Cuántos objetos hay en este montón

    [Header("Configuración de Inventario")]
    public int maxStackSize = 99; 
    public GameObject itemPrefab;

    [HideInInspector] public bool isPickedUp = false;

    private TMP_Text quantityText; // El pequeño texto en la esquina del objeto que muestra la cantidad

    private void Awake()
    {
        // Al nacer, el objeto busca automáticamente su propio texto para saber dónde escribir
        quantityText = GetComponentInChildren<TMP_Text>();
        UpdateQuantityDisplay();
    }

    // Actualiza el numerito visual de la mochila.
    public void UpdateQuantityDisplay()
    {
        if (quantityText != null)
        {
            // Si solo tenemos 1 objeto, borramos el texto para que la imagen quede más limpia.
            // Si tenemos más de 1, mostramos el número total.
            quantityText.text = quantity > 1 ? quantity.ToString() : "";
        }
    }

    // --- SISTEMA DE APILADO (STACKING) ---

    // Añade más objetos a este mismo montón
    public void AddToStack(int amount = 1)
    {
        quantity += amount;
        UpdateQuantityDisplay();
    }

    // Quita objetos del montón
    public void RemoveFromStack(int amount = 1)
    {
        quantity -= amount;
        UpdateQuantityDisplay();
    }

    // Herramienta que clona este objeto visualmente (útil para dividir objetos 
    // en dos huecos distintos del inventario pulsando el clic derecho)
    public GameObject CloneItem(int newQuantity)
    {
        GameObject clone = Instantiate(gameObject);
        Item cloneItem = clone.GetComponent<Item>();

        cloneItem.quantity = newQuantity;
        cloneItem.UpdateQuantityDisplay();

        return clone;
    }

    // --- ACCIONES DEL OBJETO ---

    // La palabra clave 'virtual' significa que esta es la acción por defecto, 
    // PERO permite que otros scripts "hijos" (como la poción) reescriban esta regla 
    // para hacer su propia magia al ser usados.
    public virtual void UseItem()
    {
        // Si el objeto es "basura" o un material que no se puede usar directamente, da este aviso:
        Debug.Log($"Este objeto ({itemName}) no funciona así.");
    }

    // Muestra un cartelito en la pantalla avisando al jugador de lo que acaba de recoger
    public virtual void ShowPopUp()
    {
        // Copiamos la imagen del objeto
        Sprite itemIcon = GetComponent<Image>().sprite;

        // Hacemos que suene el efecto de recoger botín
        SoundEffectManager.Play("ItemPickUp");

        // Le enviamos la imagen y el nombre al sistema de notificaciones de la pantalla
        if (ItemPickUpUIController.Instance != null)
        {
            ItemPickUpUIController.Instance.ShowItemPickup(itemName, itemIcon);
        }

        Debug.Log($"El jugador ha recogido: {itemName} (ID: {ID})");
    }
}