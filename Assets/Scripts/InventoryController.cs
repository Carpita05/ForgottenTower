using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs; 

    public static InventoryController Instance { get; private set; }
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged; // Evento para notificar cambios en el inventario


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
        itemDictionary = FindObjectOfType<ItemDictionary>();
        RebuildItemCounts();
        
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if(item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }
        OnInventoryChanged? .Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;

    public bool AddItem(GameObject itemPrefab)
    {
        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false; // El prefab no tiene un componente Item

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            //buscamos un slot vacio
            Slot slot = slotTransform.GetComponent<Slot>();
            
            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if (slotItem != null && slotItem.ID == itemToAdd.ID)
                {
                    slotItem.AddToStack();
                    RebuildItemCounts();
                    return true; // Item agregado al stack existente
                }
            }
        }


        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            //buscamos un slot vacio
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                RebuildItemCounts();
                return true; // Item agregado exitosamente
            }
        }
        Debug.Log("Inventario lleno, no se puede agregar el item.");
        return false; // No hay espacio en el inventario
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                // Guardamos ID y la posición (índice) del slot
                invData.Add(new InventorySaveData { itemID = item.ID, 
                slotIndex = slotTransform.GetSiblingIndex(),
                quantity = item.quantity});
            }
        }
        return invData;
    }

    public void SetInventoryItem(List<InventorySaveData> inventorySaveData)
    {
        // 1. Limpiamos el inventario actual visualmente
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // 2. Recreamos los huecos (Slots) vacíos
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        // 3. Colocamos los items guardados en sus slots correspondientes
        foreach (InventorySaveData data in inventorySaveData)
        {
            // Verificamos que el índice guardado no exceda la capacidad actual
            if (data.slotIndex < slotCount)
            {
                // Obtenemos el slot específico usando el índice guardado
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();

                // Buscamos el prefab del objeto en el diccionario
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);

                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item itemComponent = item.GetComponent<Item>();
                    if (itemComponent != null && data.quantity>1)
                    {
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }


                    slot.currentItem = item;
                }
            }
        }
        RebuildItemCounts();
    }
    public void ReomveItemsFromInventory(int itemID, int amountToRemove)
    {
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            if(amountToRemove <= 0) break;

            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot?.currentItem.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;

                if(item.quantity <= 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }
    }
}