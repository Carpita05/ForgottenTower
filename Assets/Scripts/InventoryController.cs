using System;
using System.Collections.Generic;
using UnityEngine;

// Este es el Mánager Principal del Inventario (Singleton). 
// Se encarga de guardar objetos, apilarlos, contarlos y cargar la mochila al abrir el juego.
public class InventoryController : MonoBehaviour
{
    // Hacemos que este script sea accesible desde cualquier parte del juego
    public static InventoryController Instance { get; private set; }

    [Header("Configuración Visual")]
    public GameObject inventoryPanel; // La ventana gráfica del inventario
    public GameObject slotPrefab;     // El molde visual de un hueco vacío
    public int slotCount;             // Cuántos huecos máximos tiene la mochila
    public GameObject[] itemPrefabs;  // Catálogo visual de objetos

    private ItemDictionary itemDictionary; // El diccionario con todos los objetos del juego

    // Una libreta mental (Diccionario) donde apuntamos rápidamente cuántos objetos tenemos de cada tipo.
    Dictionary<int, int> itemsCountCache = new();

    // Una "alarma" global que avisa a otros menús cuando hemos recogido o gastado algo.
    public event Action OnInventoryChanged;

    private void Awake()
    {
        // Aseguramos que solo exista un inventario en todo el juego (Patrón Singleton)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();
        // Al empezar, hacemos recuento general de lo que hay en la mochila
        RebuildItemCounts();
    }

    // Esta función hace "inventario": repasa hueco por hueco y cuenta todo lo que tenemos.
    public void RebuildItemCounts()
    {
        itemsCountCache.Clear(); // Borramos la pizarra para empezar a contar de cero

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            // Si el hueco no está vacío...
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    // Sumamos la cantidad de ese objeto a nuestra libreta mental
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }
        // Hacemos sonar la alarma para que la interfaz actualice sus números
        OnInventoryChanged?.Invoke();
    }

    // Permite a otros scripts preguntar cuántas cosas tenemos
    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    // --- RECOGER OBJETOS ---

    // Intenta meter un objeto nuevo en la mochila. Devuelve 'true' si cabe, o 'false' si está llena.
    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false;

        // 1er Intento: Buscar si ya tenemos un objeto igual para apilarlo encima
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    // ¡Encontramos uno igual! Lo sumamos al montón.
                    slotItem.AddToStack();
                    RebuildItemCounts();
                    return true;
                }
            }
        }

        // 2do Intento: Si no teníamos ninguno igual, buscamos un hueco totalmente vacío
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                // ¡Hay hueco! Creamos el dibujo del objeto, lo centramos y lo guardamos ahí.
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;

                RebuildItemCounts();
                return true;
            }
        }

        // Si llegamos aquí, es que no había ni montones iguales ni huecos vacíos.
        Debug.Log("Inventario lleno, no se puede agregar el item.");
        return false;
    }

    // --- SISTEMA DE GUARDADO ---

    // Empaqueta todo lo que hay en la mochila para guardarlo en el disco duro.
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();

                // Guardamos qué objeto es (ID), en qué casilla estaba y cuántos había en el montón.
                invData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }
        }
        return invData;
    }

    // Desempaqueta los datos del disco duro y reconstruye la mochila al cargar la partida.
    public void SetInventoryItem(List<InventorySaveData> inventorySaveData)
    {
        // 1. Tiramos a la basura cualquier objeto visual que hubiera en la mochila actual.
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // 2. Recreamos todos los huecos vacíos desde cero.
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        // 3. Colocamos los objetos guardados exactamente donde los dejamos.
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

                if (itemPrefab != null)
                {
                    // Creamos el objeto y lo centramos en su hueco
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    // Le asignamos la cantidad guardada (si es que había más de uno)
                    Item itemComponent = item.GetComponent<Item>();
                    if (itemComponent != null && data.quantity > 1)
                    {
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = item;
                }
            }
        }
        // Hacemos el recuento final para que todo cuadre
        RebuildItemCounts();
    }

    // --- ELIMINAR OBJETOS (Ej: Al usarlos o venderlos) ---

    // Busca un objeto específico en la mochila y destruye la cantidad que le pidamos.
    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            // Si ya hemos quitado todos los que queríamos, paramos de buscar.
            if (amountToRemove <= 0) break;

            Slot slot = slotTransform.GetComponent<Slot>();

            // Si encontramos el objeto que estamos buscando...
            if (slot?.currentItem != null && slot.currentItem.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                // Calculamos cuántos podemos quitar de este montón
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;

                // Si el montón se ha quedado a cero, destruimos el objeto por completo
                if (item.quantity <= 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }
    }
}