using UnityEngine;

// Esta etiqueta es la clave: le dice a Unity que esta "caja" de datos 
// se puede empaquetar, traducir a texto y guardar en el disco duro del PC.
[System.Serializable]
public class InventorySaveData
{
    // No guardamos el objeto 3D o el dibujo entero (que pesaría mucho), 
    // solo guardamos su DNI (ID) para buscarlo en el catálogo al cargar la partida.
    public int itemID;

    // El número del hueco exacto de la mochila donde dejamos el objeto.
    public int slotIndex;

    // Cuántos objetos iguales había apilados en ese mismo hueco.
    public int quantity = 1;
}