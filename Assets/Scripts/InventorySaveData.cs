using UnityEngine;

[System.Serializable]

public class InventorySaveData
{
    public int itemID;
    public int slotIndex; // Índice de la ranura en el inventario
    public int quantity = 1; // Cantidad del ítem en esa ranura
}
