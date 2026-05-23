using UnityEngine;

// Este script es pequeñito pero vital. Representa un único hueco (casilla) dentro de la mochila.
// Su única función en la vida es recordar qué objeto tiene guardado en este momento.
public class Slot : MonoBehaviour
{
    // El objeto físico que está ocupando este hueco. Si no hay nada, esto estará vacío (null).
    public GameObject currentItem;
}