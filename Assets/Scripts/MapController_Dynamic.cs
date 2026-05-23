using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Este script crea un mapa automáticamente leyendo las habitaciones del nivel.
// Dibuja un plano en pantalla, señala de verde dónde estamos 
// y mueve un icono del jugador para indicar nuestra posición.
public class MapController_Dynamic : MonoBehaviour
{
    public static MapController_Dynamic Instance { get; set; }

    [Header("Referencias de Interfaz (UI)")]
    public RectTransform mapParent; // El panel vacío donde se dibujará el mapa
    public GameObject areaPrefab;   // El molde (un cuadrado blanco) para dibujar cada habitación
    public RectTransform playerIcon;// El icono de la cabeza del jugador en el mapa

    [Header("Colores")]
    public Color defaultColor = Color.gray;     // Color de las salas sin explorar / vacías
    public Color currentAreaColor = Color.green;// Color de la sala donde estamos ahora

    [Header("Configuración del Mapa")]
    public GameObject mapBounds;              // Objeto invisible que contiene los límites de las salas
    public PolygonCollider2D initialArea;     // La sala donde empezamos
    public float mapScale = 10f;              // Escala: a mayor número, más grande se dibuja el mapa

    // Array con las formas físicas de todas las habitaciones
    private PolygonCollider2D[] mapAreas;

    // Una libreta para recordar qué dibujo de la interfaz corresponde a qué habitación real
    private Dictionary<string, RectTransform> uiAreas = new Dictionary<string, RectTransform>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Busca todas las zonas invisibles (Colliders) que definen las habitaciones del nivel
        mapAreas = mapBounds.GetComponentsInChildren<PolygonCollider2D>();
    }

    // Borra el mapa viejo y dibuja uno completamente nuevo
    public void GenerateMap(PolygonCollider2D newCurrentArea = null)
    {
        PolygonCollider2D currentArea = newCurrentArea != null ? newCurrentArea : initialArea;

        ClearMap();

        // Por cada habitación física que encuentra en el mundo, pinta un cuadrado en la pantalla
        foreach (PolygonCollider2D area in mapAreas)
        {
            CreateAreaUI(area, area == currentArea);
        }

        MovePlayerIcon(currentArea.name);
    }

    // Borra los dibujos anteriores del mapa
    private void ClearMap()
    {
        foreach (Transform child in mapParent)
        {
            Destroy(child.gameObject);
        }
        uiAreas.Clear();
    }

    // Transforma una zona física del mundo 2D en un rectángulo dibujado en el menú
    private void CreateAreaUI(PolygonCollider2D area, bool isCurrent)
    {
        // 1. Creamos el cuadrado visual
        GameObject areaImage = Instantiate(areaPrefab, mapParent);
        RectTransform rectTransform = areaImage.GetComponent<RectTransform>();

        // 2. Medimos cuánto ocupa la habitación real en el mundo
        Bounds bounds = area.bounds;

        // 3. Calculamos su tamaño en pantalla multiplicándolo por la Escala (mapScale)
        rectTransform.sizeDelta = new Vector2(bounds.size.x * mapScale, bounds.size.y * mapScale);
        rectTransform.anchoredPosition = bounds.center * mapScale;

        // 4. Si es la sala en la que estamos, la pintamos verde. Si no, gris.
        areaImage.GetComponent<Image>().color = isCurrent ? currentAreaColor : defaultColor;

        // 5. Apuntamos el dibujo en nuestra libreta para poder modificarlo luego
        uiAreas[area.name] = rectTransform;
    }

    // Actualiza los colores cuando cambiamos de habitación
    public void UpdateCurrentArea(string newCurrentArea)
    {
        // Revisa todas las salas dibujadas y pinta de verde solo en la que acabamos de entrar
        foreach (KeyValuePair<string, RectTransform> area in uiAreas)
        {
            area.Value.GetComponent<Image>().color = area.Key == newCurrentArea ? currentAreaColor : defaultColor;
        }

        // Mueve la "ficha" de nuestro personaje a la sala nueva
        MovePlayerIcon(newCurrentArea);
    }

    // Mueve el icono del jugador al centro matemático de la habitación
    private void MovePlayerIcon(string newCurrentArea)
    {
        if (uiAreas.TryGetValue(newCurrentArea, out RectTransform areaUI))
        {
            playerIcon.anchoredPosition = areaUI.anchoredPosition;
        }
    }
}