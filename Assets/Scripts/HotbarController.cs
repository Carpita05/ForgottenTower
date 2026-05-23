using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Este script controla la Barra de Acceso Rápido (Hotbar).
// Permite al jugador usar objetos de su inventario pulsando los números del 1 al 0 en su teclado,
// y se encarga de guardar y cargar esos objetos cuando salimos del juego.
public class HotbarController : MonoBehaviour
{
    [Header("Configuración de la Barra")]
    public GameObject hotbarPanel;  // El panel visual que contiene los huecos en la pantalla
    public GameObject slotPrefab;   // El molde (Prefab) para crear huecos vacíos
    public int slotCount = 10;      // Cuántos huecos tiene la barra (teclas del 1 al 0)

    private ItemDictionary itemDictionary; // El "catálogo" de todos los objetos del juego
    private Key[] hotbarKeys;              // Las teclas asignadas a cada hueco

    private void Awake()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();

        // Configuramos los controles: le asignamos a cada hueco un número del teclado.
        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            // Asignamos del 1 al 9 secuencialmente, y al décimo hueco le asignamos el número 0.
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }
    }

    void Update()
    {
        // Vigila constantemente si el jugador pulsa alguna de las teclas de la barra.
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                UseItemSlot(i);
            }
        }
    }

    // Esta función busca qué objeto hay en el hueco que hemos pulsado y lo utiliza.
    void UseItemSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem(); // Por ejemplo, beber la poción o equipar una espada
        }
    }

    // --- SISTEMA DE GUARDADO ---

    // Función para recoger los datos de la barra y guardarlos en el disco duro.
    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();

        // Revisamos hueco por hueco
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                // Anotamos qué objeto es (su ID) y en qué posición exacta de la barra estaba
                hotbarData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
            }
        }
        return hotbarData;
    }

    // Función para reconstruir la barra exactamente como estaba cuando el jugador cargue la partida.
    public void SetHotbarItem(List<InventorySaveData> inventorySaveData)
    {
        // 1. Limpiamos la barra visualmente, destruyendo cualquier objeto residual
        foreach (Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // 2. Recreamos los 10 huecos vacíos desde cero
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

        // 3. Colocamos los objetos guardados en los huecos correspondientes
        foreach (InventorySaveData data in inventorySaveData)
        {
            // Verificamos por seguridad que el hueco guardado exista en la barra actual
            if (data.slotIndex < slotCount)
            {
                // Buscamos el hueco concreto
                Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();

                // Buscamos el objeto en el catálogo usando su ID
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

                if (itemPrefab != null)
                {
                    // Creamos el objeto dentro del hueco
                    GameObject item = Instantiate(itemPrefab, slot.transform);

                    // Ajustamos su tamaño y posición para que quede perfectamente centrado en la interfaz
                    RectTransform itemRect = item.GetComponent<RectTransform>();
                    itemRect.localPosition = Vector3.zero;
                    itemRect.anchoredPosition = Vector2.zero;
                    itemRect.localRotation = Quaternion.identity;
                    itemRect.localScale = Vector3.one;

                    // Le decimos al hueco que ahora contiene oficialmente este objeto
                    slot.currentItem = item;
                }
            }
        }
    }
}