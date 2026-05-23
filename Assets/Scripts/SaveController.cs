using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

// Este es el Gestor Maestro de Guardado (Singleton).
// Se encarga de recoger los datos de todos los demás sistemas (mochila, misiones, combate...),
// meterlos en la "maleta" (SaveData) y escribir el archivo JSON en el ordenador. 
// También hace el proceso inverso al cargar la partida.
public class SaveController : MonoBehaviour
{
    public static SaveController Instance { get; private set; }

    private string saveLocation; // La ruta exacta del disco duro donde se guardará el archivo JSON

    // Referencias a otros sistemas que necesitamos consultar
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;

    // Lista temporal en memoria para recordar qué jefes hemos matado mientras jugamos
    public List<string> defeatedBosses = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeComponents();
        LoadGame(); // Nada más arrancar el nivel, intentamos cargar la partida anterior
    }

    private void InitializeComponents()
    {
        // Guardamos el archivo en la carpeta "AppData" segura del usuario del ordenador
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");

        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();

        // Buscamos todos los cofres que existen en el mapa al arrancar
        chests = FindObjectsOfType<Chest>();
    }

    // --- GUARDADO ---

    // Esta función la llaman los Jefes (BossHealth) justo antes de morir
    public void RegisterBossDefeated(string bossID)
    {
        if (!defeatedBosses.Contains(bossID))
        {
            defeatedBosses.Add(bossID);
        }
    }

    // Recopila todo y escribe el archivo en el ordenador
    public void SaveGame()
    {
        // 1. Buscamos el daño actual del jugador
        PlayerCombat combat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombat>();
        int currentDamage = combat != null ? combat.attackDamage : 1;

        // 2. Preparamos la "maleta" (SaveData) y le metemos absolutamente todo
        SaveData saveData = new SaveData()
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindObjectOfType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,

            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestsState(),

            questProgressData = QuestController.Instance.activateQuests,
            handInQuestsIDs = QuestController.Instance.handInQuestsIDs,

            defeatedBossesIDs = this.defeatedBosses,
            playerAttackDamage = currentDamage
        };

        // 3. Traducimos la maleta a texto (JSON) y la guardamos físicamente en el disco duro
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log("Partida Guardada en: " + saveLocation);
    }

    // Lee el estado de todos los cofres para apuntar cuáles están ya abiertos
    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();
        foreach (Chest chest in chests)
        {
            chestStates.Add(new ChestSaveData { chestID = chest.ChestID, isOpened = chest.IsOpened });
        }
        return chestStates;
    }

    // --- CARGA ---

    // Lee el archivo del ordenador y reconstruye el mundo tal y como lo dejamos
    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            // 1. Leemos el texto del JSON y lo convertimos de vuelta en datos reales (SaveData)
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            // 2. Colocamos al jugador donde estaba
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = saveData.playerPosition;

            // 3. Ajustamos los límites de la cámara a la habitación correcta
            PolygonCollider2D saveMapBoundary = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();
            if (saveMapBoundary != null)
            {
                FindObjectOfType<CinemachineConfiner2D>().BoundingShape2D = saveMapBoundary;
            }

            // 4. Reconstruimos los objetos del inventario y la barra inferior
            inventoryController.SetInventoryItem(saveData.inventorySaveData);
            hotbarController.SetHotbarItem(saveData.hotbarSaveData);

            // 5. Dejamos abiertos los cofres que ya habíamos saqueado
            LoadChestStates(saveData.chestSaveData);

            // 6. Restauramos el diario de misiones
            QuestController.Instance.LoadQuestProgress(saveData.questProgressData);
            QuestController.Instance.handInQuestsIDs = saveData.handInQuestsIDs ?? new List<string>();

            // 7. Borramos del mapa a los jefes que ya habíamos derrotado para que no resuciten
            this.defeatedBosses = saveData.defeatedBossesIDs ?? new List<string>();
            CheckDefeatedBosses();

            // 8. Le devolvemos al jugador el daño extra que hubiera conseguido
            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                int damageToLoad = saveData.playerAttackDamage > 0 ? saveData.playerAttackDamage : combat.baseAttackDamage;
                combat.LoadDamage(damageToLoad);
            }
        }
        else
        {
            // Si el archivo NO existe (es la primera vez que jugamos), 
            // creamos un archivo nuevo por defecto y limpiamos los inventarios.
            SaveGame();
            inventoryController.SetInventoryItem(new List<InventorySaveData>());
            hotbarController.SetHotbarItem(new List<InventorySaveData>());
        }
    }

    // Sincroniza los cofres del mundo con los datos guardados
    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach (Chest chest in chests)
        {
            // Buscamos si tenemos información guardada sobre este cofre en particular
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);

            if (chestSaveData != null)
            {
                // Le aplicamos su estado correcto (abierto o cerrado)
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }

    // Busca por el mapa si hay jefes vivos que, según el archivo de guardado, deberían estar muertos
    private void CheckDefeatedBosses()
    {
        BossHealth[] bossesInScene = FindObjectsOfType<BossHealth>();

        foreach (BossHealth boss in bossesInScene)
        {
            if (defeatedBosses.Contains(boss.bossID))
            {
                // Si el jefe ya estaba muerto en nuestra partida guardada, 
                // abrimos la puerta de su sala y destruimos su modelo físico para que no moleste.
                if (boss.doorToOpen != null) boss.doorToOpen.SetActive(false);
                Destroy(boss.gameObject);
            }
        }
    }
}