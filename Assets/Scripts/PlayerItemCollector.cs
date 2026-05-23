using UnityEngine;

// Este script funciona como una "aspiradora". 
// Se coloca en el jugador y hace que, al pisar un objeto tirado en el suelo,
// lo recojamos automáticamente y nos lo guardemos en la mochila.
public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    void Start()
    {
        // Buscamos cuál es el cerebro principal de nuestra mochila
        inventoryController = FindObjectOfType<InventoryController>();
    }

    // Esta función física salta sola cuando nuestro cuerpo roza otra zona invisible
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Comprobamos si el objeto que acabamos de pisar tiene la etiqueta "Item" (Botín)
        if (collision.CompareTag("Item"))
        {
            Item item = collision.GetComponent<Item>();

            if (item != null)
            {
                // Intentamos meter el objeto en la mochila. 
                // Esto nos devolverá 'true' si había hueco, o 'false' si estamos llenos.
                bool itemAdded = inventoryController.AddItem(collision.gameObject);

                if (itemAdded)
                {
                    // Si ha cabido en la mochila, mostramos el cartelito de "Objeto recogido"
                    item.ShowPopUp();

                    // Destruimos el objeto del suelo 3D (porque ahora lo tenemos en la interfaz 2D)
                    Destroy(collision.gameObject);
                }
            }
        }
    }
}