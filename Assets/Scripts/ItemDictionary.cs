using System.Collections.Generic;
using UnityEngine;

// Este script funciona como el "Gran Catálogo" de todos los objetos del juego.
// Al arrancar, le pone una etiqueta con un número (ID) a cada objeto para que 
// el sistema de guardado y el inventario puedan buscarlos rápidamente sin confundirse.
public class ItemDictionary : MonoBehaviour
{
    // Lista donde arrastraremos todos los objetos posibles desde el Inspector de Unity
    public List<Item> itemPrefabs;

    // Una "agenda" interna (Diccionario) para buscar objetos a la velocidad de la luz usando su número de ID
    public Dictionary<int, GameObject> itemDictionary = new Dictionary<int, GameObject>();

    private void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();

        // 1. Recorremos la lista de objetos y les asignamos un número de serie (ID).
        // Empezamos en 1, 2, 3... según su orden en la lista.
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i] != null)
            {
                itemPrefabs[i].ID = i + 1;
            }
        }

        // 2. Guardamos todos los objetos en nuestra "agenda" asociándolos a su ID recién creado.
        foreach (Item item in itemPrefabs)
        {
            itemDictionary[item.ID] = item.gameObject;
        }
    }

    // Cualquier otro script puede llamar a esta función dándole un número (ej: "Dame el objeto 5") 
    // y el diccionario le devolverá el objeto correcto al instante.
    public GameObject GetItemPrefab(int itemID)
    {
        itemDictionary.TryGetValue(itemID, out GameObject prefab);

        if (prefab == null)
        {
            Debug.LogError($"Error: El objeto con ID {itemID} no existe en el catálogo.");
        }

        return prefab;
    }
}