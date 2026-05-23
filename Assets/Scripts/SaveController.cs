using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    public static SaveController Instance { get; private set; }

    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;

    public List<string> defeatedBosses = new List<string>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        InitializeComponents();
        LoadGame();
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        hotbarController = FindObjectOfType<HotbarController>();
        chests = FindObjectsOfType<Chest>();
    }

    public void RegisterBossDefeated(string bossID)
    {
        if (!defeatedBosses.Contains(bossID))
        {
            defeatedBosses.Add(bossID);
        }
    }

    public void SaveGame()
    {
        PlayerCombat combat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombat>();
        int currentDamage = combat != null ? combat.attackDamage : 1;

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

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log("Partida Guardada en: " + saveLocation);
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();
        foreach (Chest chest in chests)
        {
            chestStates.Add(new ChestSaveData { chestID = chest.ChestID, isOpened = chest.IsOpened });
        }
        return chestStates;
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = saveData.playerPosition;

            PolygonCollider2D saveMapBoundary = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();
            if (saveMapBoundary != null)
            {
                FindObjectOfType<CinemachineConfiner2D>().BoundingShape2D = saveMapBoundary;
            }

            inventoryController.SetInventoryItem(saveData.inventorySaveData);
            hotbarController.SetHotbarItem(saveData.hotbarSaveData);

            LoadChestStates(saveData.chestSaveData);

            QuestController.Instance.LoadQuestProgress(saveData.questProgressData);
            QuestController.Instance.handInQuestsIDs = saveData.handInQuestsIDs ?? new List<string>();

            this.defeatedBosses = saveData.defeatedBossesIDs ?? new List<string>();
            CheckDefeatedBosses();

            PlayerCombat combat = player.GetComponent<PlayerCombat>();
            if (combat != null)
            {
                // Si el archivo JSON tiene daño (mayor a 0), lo cargamos. Si es una partida antigua o no hay daño guardado, cargamos el daño base.
                int damageToLoad = saveData.playerAttackDamage > 0 ? saveData.playerAttackDamage : combat.baseAttackDamage;
                combat.LoadDamage(damageToLoad);
            }
        }
        else
        {
            SaveGame();
            inventoryController.SetInventoryItem(new List<InventorySaveData>());
            hotbarController.SetHotbarItem(new List<InventorySaveData>());
        }
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestStates.FirstOrDefault(c => c.chestID == chest.ChestID);
            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpened);
            }
        }
    }

    private void CheckDefeatedBosses()
    {
        BossHealth[] bossesInScene = FindObjectsOfType<BossHealth>();
        foreach (BossHealth boss in bossesInScene)
        {
            if (defeatedBosses.Contains(boss.bossID))
            {
                if (boss.doorToOpen != null) boss.doorToOpen.SetActive(false);
                Destroy(boss.gameObject);
            }
        }
    }
}