using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CharacterCustomization : MonoBehaviour
{
    [System.Serializable]
    public class Item
    {
        public int id;                    // Уникальный ID предмета
        public int Cost = 100;
        public GameObject itemObject;     // Объект предмета для визуального отображения
        public Transform attachPoint;     // Точка, к которой будет крепиться предмет (если нужно)
    }

    public List<Item> items;  // Список всех доступных предметов
    private Dictionary<int, GameObject> equippedItems = new Dictionary<int, GameObject>(); // Словарь для хранения надетых предметов

    

    [Header("Character Color Materials")]
    //public List<Material> colorMaterials; // Предустановленные материалы для смены цвета
    //public Renderer targetRenderer;       // Рендерер игрока (например, SkinnedMeshRenderer)
    public Texture2D[] albedoList;
    Renderer[] characterMaterials;
    [ColorUsage(true, true)]
    public Color[] eyeColors;
    private int currentColorIndex = -1;

    private void Start()
    {
        characterMaterials = GetComponentsInChildren<Renderer>();

        LoadEquipment();

        int savedColorIndex = PlayerPrefs.GetInt("CharacterColorIndex", 0);
        SetCharacterColor(savedColorIndex);
    }

    public void RewardPlayer(int earned)
    {
        PlayerDataManager.AddCrystalls(earned);
    }

    public void EquipItem(int itemId)
    {
        // Перед тем как надеть новый предмет, снимаем все остальные
        UnequipAllItems();

        // Находим предмет с соответствующим ID
        Item itemToEquip = items.Find(i => i.id == itemId);

        if (itemToEquip != null)
        {
            // Активируем предмет или создаём дочерний объект, если точка крепления задана
            GameObject newItem;
            if (itemToEquip.attachPoint != null)
            {
                newItem = Instantiate(itemToEquip.itemObject, itemToEquip.attachPoint);
                newItem.transform.localPosition = Vector3.zero;
                newItem.transform.localRotation = Quaternion.identity;
            }
            else
            {
                newItem = itemToEquip.itemObject;
                newItem.SetActive(true);
            }

            // Сохраняем надетый предмет в словарь и записываем его ID в PlayerPrefs
            equippedItems[itemId] = newItem;
            PlayerPrefs.SetInt("EquippedItem_" + itemId, itemId);
            PlayerPrefs.Save();
        }
    }

    public void UnequipAllItems()
    {
        // Проходим по всем надетым предметам, удаляем их и очищаем данные
        foreach (var equippedItem in equippedItems.Values)
        {
            Destroy(equippedItem);
        }
        equippedItems.Clear(); // Очищаем словарь после снятия всех предметов
        PlayerPrefs.DeleteAll(); // Удаляем все сохраненные предметы из PlayerPrefs
    }

    private void LoadEquipment()
    {
        // Загружаем надетые предметы, используя сохранённые ID
        foreach (Item item in items)
        {
            if (PlayerPrefs.HasKey("EquippedItem_" + item.id))
            {
                EquipItem(item.id);
            }
        }
    }

    public void SetCharacterColor(int index)
    {
        // Применяем текстуру (albedo) ко всем материалам персонажа
        for (int i = 0; i < characterMaterials.Length; i++)
            if (characterMaterials[i].transform.CompareTag("PlayerEyes"))
                characterMaterials[i].material.SetColor("_EmissionColor", eyeColors[index]);
            else
                characterMaterials[i].material.SetTexture("_MainTex", albedoList[index]);

        // Сохраняем выбранный индекс цвета в PlayerPrefs
        PlayerPrefs.SetInt("CharacterColorIndex", index);
        PlayerPrefs.Save();
    }
}

public class PlayerDataManager : MonoBehaviour
{
    public static int Crystalls
    {
        get => PlayerPrefs.GetInt("Crystalls", 0);
        set
        {
            PlayerPrefs.SetInt("Crystalls", value);
            PlayerPrefs.Save();
        }
    }

    public static void AddCrystalls(int amount)
    {
        Crystalls += amount;
    }

    public static bool SpendCrystalls(int amount)
    {
        if (Crystalls >= amount)
        {
            Crystalls -= amount;
            return true;
        }
        return false;
    }

    
}