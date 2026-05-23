using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarController : MonoBehaviour
{
    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 10;// Del 1-0 en el teclado

    private ItemDictionary itemDictionary;

    private Key[] hotbarKeys;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();

        hotbarKeys = new Key[slotCount];
        for (int i = 0; i < slotCount; i++)
        {
            // CAMBIO AQUÍ: Cambiamos el + 1 por + i
            hotbarKeys[i] = i < 9 ? (Key)((int)Key.Digit1 + i) : Key.Digit0;
        }
    }

    void Update()
    {
        for (int i = 0; i < slotCount; i++)
        {
            if (Keyboard.current[hotbarKeys[i]].wasPressedThisFrame)
            {
                UseItemSlot(i);
            }
        }
    }

    // CAMBIO AQUÍ: Sacamos la función fuera del Update
    void UseItemSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem();
        }
    }

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> hotbarData = new List<InventorySaveData>();
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                // Guardamos ID y la posición (índice) del slot
                hotbarData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
            }
        }
        return hotbarData;
    }

    public void SetHotbarItem(List<InventorySaveData> inventorySaveData)
    {
        // 1. Limpiamos la hotbar actual visualmente
        foreach (Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }

        // 2. Recreamos los huecos (Slots) vacíos
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

        // 3. Colocamos los items guardados en sus slots correspondientes
        foreach (InventorySaveData data in inventorySaveData)
        {
            // Verificamos que el índice guardado no exceda la capacidad actual
            if (data.slotIndex < slotCount)
            {
                // Obtenemos el slot específico usando el índice guardado
                Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();

                // Buscamos el prefab del objeto en el diccionario
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);

                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);

                    RectTransform itemRect = item.GetComponent<RectTransform>();

                    itemRect.localPosition = Vector3.zero;
                    itemRect.anchoredPosition = Vector2.zero;

                    itemRect.localRotation = Quaternion.identity;

                    itemRect.localScale = Vector3.one;

                    slot.currentItem = item;
                }
            }
        }
    }
}
