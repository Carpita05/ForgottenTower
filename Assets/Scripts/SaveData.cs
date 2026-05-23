using System.Collections.Generic;
using UnityEngine;

// --- ESTRUCTURAS DE DATOS PARA GUARDAR ---
// Esta etiqueta [System.Serializable] es fundamental. Le dice a Unity que esta clase 
// no es un objeto 3D ni un comportamiento, sino un paquete de datos puro que se puede 
// traducir a texto (JSON) y guardar físicamente en el disco duro del ordenador.

[System.Serializable]
public class SaveData
{
    [Header("Datos del Jugador")]
    public Vector3 playerPosition;   // Coordenadas exactas (X, Y) del jugador
    public int playerAttackDamage;   // Cuánto daño hace (por si tomó pociones de fuerza)

    [Header("Datos del Mundo")]
    public string mapBoundary;             // Nombre de la habitación en la que nos quedamos
    public List<string> defeatedBossesIDs; // Lista de DNI de los jefes que ya hemos matado

    [Header("Inventario y Objetos")]
    public List<InventorySaveData> inventorySaveData; // Lo que hay dentro de la mochila
    public List<InventorySaveData> hotbarSaveData;    // Lo que hay en la barra de acceso rápido
    public List<ChestSaveData> chestSaveData;         // El estado de todos los cofres del mundo

    [Header("Misiones (Quests)")]
    public List<QuestProgress> questProgressData; // Misiones a medias y su progreso
    public List<string> handInQuestsIDs;          // Misiones ya terminadas y cobradas
}

// Estructura pequeñita y auxiliar solo para recordar si un cofre específico ya fue saqueado
[System.Serializable]
public class ChestSaveData
{
    public string chestID; // El DNI único del cofre
    public bool isOpened;  // ¿Está abierto (true) o cerrado (false)?
}