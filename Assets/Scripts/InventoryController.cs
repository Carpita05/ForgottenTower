using System;
using System.Collections.Generic;
using UnityEngine;

// Este es el Mánager Principal del Inventario (Singleton). 
// Se encarga de guardar objetos, apilarlos, contarlos y cargar la mochila al abrir el juego.
public class InventoryController : MonoBehaviour
{
    public static InventoryController Instance { get; private set; }

    [Header("Configuración Visual")]
    public GameObject inventoryPanel; // La ventana gráfica del inventario
    public GameObject slotPrefab;     // El molde visual de un hueco vacío
    public int slotCount;             // Cuántos huecos máximos tiene la mochila
    public GameObject[] itemPrefabs;  // Catálogo visual de objetos

    private ItemDictionary itemDictionary;

    // Una libreta mental (Diccionario) donde apuntamos rápidamente cuántos objetos tenemos de cada tipo.
    private Dictionary<int, int> itemsCountCache = new();

    // Una "alarma" global que avisa a otros menús (como las Quests) cuando hemos recogido o gastado algo.
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        if (inventoryPanel.transform.childCount == 0)
        {
            for (int i = 0; i < slotCount; i++)
            {
                Instantiate(slotPrefab, inventoryPanel.transform);
            }
        }

        itemDictionary = FindObjectOfType<ItemDictionary>();
        RebuildItemCounts();
    }

    // --- AÑADIR OBJETOS ---
    public bool AddItem(GameObject itemPrefab)
    {
        Item itemComponent = itemPrefab.GetComponent<Item>();
        if (itemComponent == null || itemComponent.isPickedUp) return false;
        itemComponent.isPickedUp = true;

        // 1. Primero intentamos buscar si ya tenemos ese mismo objeto para APILARLO (Stack)
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item existingItem = slot.currentItem.GetComponent<Item>();
                if (existingItem != null && existingItem.ID == itemComponent.ID && existingItem.quantity < existingItem.maxStackSize)
                {
                    existingItem.AddToStack(1);
                    RebuildItemCounts(); // Actualizamos la caché de datos
                    return true;
                }
            }
        }

        // 2. Si no se puede apilar, buscamos un hueco totalmente vacío
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem == null)
            {
                GameObject itemObj = Instantiate(itemPrefab, slot.transform);

                RectTransform rect = itemObj.GetComponent<RectTransform>();
                rect.anchoredPosition = Vector2.zero;
                rect.localScale = Vector3.one; // Forzamos escala (1,1,1) para evitar que herede tamaños del mundo físico

                slot.currentItem = itemObj; // Le asignamos el objeto al hueco de forma lógica

                RebuildItemCounts();
                return true;
            }
        }
        return false; // Inventario lleno
    }

    // --- RECONSTRUIR EL RECUENTO (EL CEREBRO DEL INVENTARIO) ---
    // Esta función limpia la libreta y vuelve a contar ranura por ranura qué tenemos. 
    // Es vital llamarla siempre que ocurra un cambio físico o de posición.
    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    if (itemsCountCache.ContainsKey(item.ID))
                    {
                        itemsCountCache[item.ID] += item.quantity;
                    }
                    else
                    {
                        itemsCountCache[item.ID] = item.quantity;
                    }
                }
            }
        }

        // Disparamos la alarma para avisar a sistemas como QuestController de que el inventario cambió
        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts()
    {
        return new Dictionary<int, int>(itemsCountCache);
    }

    // Devuelve una lista empaquetada con los datos puros para el script de guardado JSON
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> saveDataList = new List<InventorySaveData>();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    saveDataList.Add(new InventorySaveData { itemID = item.ID, quantity = item.quantity });
                    continue;
                }
            }
            saveDataList.Add(new InventorySaveData { itemID = -1, quantity = 0 }); // Ranura vacía
        }
        return saveDataList;
    }

    // Reconstruye físicamente la mochila al cargar una partida guardada
    public void SetInventoryItem(List<InventorySaveData> savedData)
    {
        int index = 0;
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null) continue;

            if (slot.currentItem != null) Destroy(slot.currentItem);
            slot.currentItem = null;

            if (savedData != null && index < savedData.Count)
            {
                InventorySaveData data = savedData[index++];
                if (data.itemID != -1 && itemDictionary != null)
                {
                    GameObject prefab = itemDictionary.GetItemPrefab(data.itemID);
                    if (prefab != null)
                    {
                        GameObject item = Instantiate(prefab, slot.transform);

                        RectTransform rect = item.GetComponent<RectTransform>();
                        rect.anchoredPosition = Vector2.zero;
                        rect.localScale = Vector3.one; // Seguridad extra contra reescalados al cargar

                        Item itemComponent = item.GetComponent<Item>();
                        if (itemComponent != null)
                        {
                            itemComponent.quantity = data.quantity;
                            itemComponent.UpdateQuantityDisplay();
                        }
                        slot.currentItem = item;
                    }
                }
            }
        }
        RebuildItemCounts();
    }

    // Quita objetos de la mochila (Ej: Al entregar una Quest)
    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            if (amountToRemove <= 0) break;

            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot?.currentItem != null && slot.currentItem.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;

                if (item.quantity <= 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null; // Vaciamos la variable del slot para evitar duplicados
                }
            }
        }
        RebuildItemCounts(); // Forzamos el recuento tras eliminar
    }
}