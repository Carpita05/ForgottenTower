using UnityEngine;
using UnityEngine.UI;

// Script muy limpio para gestionar menús con pestañas (Ej: Inventario / Misiones / Opciones).
// Se encarga de encender la pestaña seleccionada, apagar las demás 
// y cambiar el color de los botones para saber dónde estamos.
public class TabsController : MonoBehaviour
{
    [Header("Referencias")]
    public Image[] tabImages;    // Los botones de las pestañas
    public GameObject[] pages;   // Las ventanas o paneles de contenido

    void Start()
    {
        // Al abrir el menú por primera vez, mostramos siempre la primera pestaña (Índice 0)
        ActivateTab(0);
    }

    // Esta función se conecta a los botones de la interfaz
    public void ActivateTab(int tabNo)
    {
        // 1. Apagamos todas las ventanas y ponemos todos los botones en color rojo (inactivo)
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
            tabImages[i].color = Color.red;
        }

        // 2. Encendemos únicamente la ventana que hemos seleccionado y pintamos su botón de blanco
        pages[tabNo].SetActive(true);
        tabImages[tabNo].color = Color.white;
    }
}