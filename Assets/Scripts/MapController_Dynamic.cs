using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController_Dynamic : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform mapParent;
    public GameObject areaPrefab;
    public RectTransform playerIcon;

    [Header("Colours")]
    public Color defaultColor = Color.gray;
    public Color currentAreaColor = Color.green;

    [Header("Map Settings")]
    public GameObject mapBounds;
    public PolygonCollider2D initialArea;
    public float mapScale = 10f;

    private PolygonCollider2D[] mapAreas;
    private Dictionary<string, RectTransform> uiAreas = new Dictionary<string, RectTransform>();

    public static MapController_Dynamic Instance { get; set; }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        mapAreas = mapBounds.GetComponentsInChildren<PolygonCollider2D>();

    }
    //Generar el mapa
    public void GenerateMap(PolygonCollider2D newCurrentArea = null)
    {
        PolygonCollider2D currentArea = newCurrentArea != null ? newCurrentArea : initialArea;

        ClearMap();

        foreach (PolygonCollider2D area in mapAreas)
        {
           CreateAreaUI(area, area == currentArea);
        }
        MovePlayerIcon(currentArea.name);
    }

    //Limpiar el mapa
    private void ClearMap()
    {
        foreach (Transform child in mapParent)
        {
            Destroy(child.gameObject);
        }
        uiAreas.Clear();
    }

    private void CreateAreaUI(PolygonCollider2D area, bool isCurrent)
    {
        //Instanciar el prefab del área
        GameObject areaImage = Instantiate(areaPrefab, mapParent);
        RectTransform rectTransform = areaImage.GetComponent<RectTransform>();

        //Coger los bordes
        Bounds bounds = area.bounds;

        //Escalar y posicionar el área en el mapa
        rectTransform.sizeDelta = new Vector2(bounds.size.x * mapScale, bounds.size.y * mapScale);
        rectTransform.anchoredPosition = bounds.center * mapScale;

        //Colorear el área según si es la actual o no
        areaImage.GetComponent<Image>().color = isCurrent ? currentAreaColor : defaultColor;

        //Agregar el área al diccionario
        uiAreas[area.name] = rectTransform;
    }


    //Actualizar el mapa con la posición del jugador
    public void UpdateCurrentArea(string newCurrentArea)
    {
        foreach (KeyValuePair<string, RectTransform> area in uiAreas)
        {
            area.Value.GetComponent<Image>().color = area.Key == newCurrentArea ? currentAreaColor : defaultColor; // Restablecer el color de todas las áreas
        }

        MovePlayerIcon(newCurrentArea);
    }

    //Mover el icono del jugador al área actual
    private void MovePlayerIcon(string newCurrentArea)
    {

        if(uiAreas.TryGetValue(newCurrentArea, out RectTransform areaUI)){
            playerIcon.anchoredPosition = areaUI.anchoredPosition; // Mover el icono del jugador al centro del área actual
        }

    }

}
