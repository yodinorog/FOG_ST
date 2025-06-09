using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    private float interactionRange = 3;       // Радиус взаимодействия
    public Text interactionText;              // UI-текст для отображения "Использовать"
    public LayerMask interactableLayer;       // Слой объектов для взаимодействия
    public GameObject shopUI;                 // UI-окно магазина
    public GameObject forgeUI;                 // UI-окно кузницы
    public GameObject mageUI;
    public GameObject storageUI;              // UI-окно склада
    public GameObject towerUI;                // UI-окно башни
    public GameObject towerPrefab1;         // Префаб первой башни
    public GameObject towerPrefab2;         // Префаб второй башни
    public GameObject towerPrefab3;         // Префаб третьей башни
    public GameObject towerPrefab4;         // Префаб третьей башни

    public GameObject resourcePrefabBlue;
    public GameObject resourcePrefabRed;
    public bool isGatheringNow = false;

    private MovementInput player; // Ссылка на скрипт MovementInput

    public string merchantCutsceneName = "MerchantCutscene";

    public LayerMask towerSlotLayer;    // Слой для проверки взаимодействия с TowerSlot
    public bool buildMode = false;      // Флаг режима строительства

    private Transform currentTarget;          // Текущий объект для взаимодействия
    private bool isInteracting = false;       // Флаг для определения взаимодействия
    private Animator animator;                // Аниматор игрока

    public bool showtowers;

    public GameObject towerUpgradeUI;
    public Button[] starButtons;
    public Text towerLevelText;
    public Text towerUpgradeCostText;

    public Sprite filledStarSprite;
    public Sprite emptyStarSprite;

    public Image towerIconImage;  // UI Image для отображения иконки типа башни

    public Sprite turretSprite;
    public Sprite damageAuraSprite;
    public Sprite magicTowerSprite;
    public Sprite slowTowerSprite;

    public int upgradeCost;

    private MonoBehaviour selectedTowerScript; // текущая выбранная башня
    public Button upgradeButton;               // присвоить через инспектор

    public GameObject interactionButton; // UI-кнопка взаимодействия
    public Text objectNameText;           // UI-текст с именем объекта
    private Outline currentOutline;       // Контур текущего объекта

    public Transform keyIconContainer; // Под UI надписью объекта
    public GameObject keyIconPrefab;   // Префаб иконки ключа

    void Start()
    {
        player = GetComponent<MovementInput>();
        interactionText.gameObject.SetActive(false);
        animator = GetComponent<Animator>();

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeSelectedTower);
        else
            Debug.LogWarning("Upgrade button is not assigned in the inspector!");

        interactionButton.SetActive(false);
        objectNameText.text = "";

        if (interactionButton != null)
        {
            interactionButton.SetActive(false); // скрыта по умолчанию
            interactionButton.GetComponent<Button>().onClick.RemoveAllListeners();
            interactionButton.GetComponent<Button>().onClick.AddListener(StartInteraction);
        }
    }

    void Update()
    {
        CheckForInteractable();

        if (currentTarget != null)
        {
            interactionText.transform.position = Camera.main.WorldToScreenPoint(currentTarget.position + Vector3.up * 1.5f);
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartInteraction();
            }
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }

        if (buildMode)
        {
            HighlightNearbySlots();
        }
        else
        {
            ClearSlotHighlights();
        }

        // Прерываем взаимодействие, если игрок ушел слишком далеко
        if (isInteracting && Vector3.Distance(transform.position, currentTarget.position) > interactionRange+1)
        {
            EndInteraction();
        }
    }

    public void ToggleBuildMode()
    {
        buildMode = !buildMode;

        if (buildMode)
        {
            player.ShowMessage("Build mode");
            player.BuildModeOn();
        }
        else
        {
            player.ShowMessage("Fight mode");
            player.BuildModeOff();
        }
    }

    void HighlightNearbySlots()
    {
        // Найдём все слоты с тэгом TowerSlot в радиусе взаимодействия
        Collider[] slotsInRange = Physics.OverlapSphere(transform.position, interactionRange, towerSlotLayer);

        foreach (GameObject slot in GameObject.FindGameObjectsWithTag("TowerSlot"))
        {
            // Если слот в радиусе взаимодействия, включаем его визуализацию
            if (IsSlotInRange(slot.transform, slotsInRange))
            {
                ShowSlot(slot);
            }
            else
            {
                HideSlot(slot);
            }
        }
    }

    bool IsSlotInRange(Transform slotTransform, Collider[] slotsInRange)
    {
        foreach (Collider col in slotsInRange)
        {
            if (col.transform == slotTransform)
            {
                return true;
            }
        }
        return false;
    }

    void ClearSlotHighlights()
    {
        // Скрываем все слоты при выходе из режима строительства
        foreach (GameObject slot in GameObject.FindGameObjectsWithTag("TowerSlot"))
        {
            HideSlot(slot);
        }
    }

    void ShowSlot(GameObject slot)
    {
        Renderer slotRenderer = slot.GetComponent<Renderer>();
        if (slotRenderer != null)
        {
            slotRenderer.enabled = true; // Показываем объект
        }
    }

    void HideSlot(GameObject slot)
    {
        Renderer slotRenderer = slot.GetComponent<Renderer>();
        if (slotRenderer != null)
        {
            slotRenderer.enabled = false; // Прячем объект
        }
    }

    public void CheckForInteractable()
    {
        Collider[] interactables = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;
        ResourceCooldown cooldown;

        foreach (Collider interactable in interactables)
        {
            if (interactable.CompareTag("TowerSlot") && !buildMode)
                continue;

            float distance = Vector3.Distance(transform.position, interactable.transform.position);
            if (distance < closestDistance)
            {
                closestTarget = interactable.transform;
                closestDistance = distance;
            }
        }

        if (closestTarget != null)
        {
            // Получаем компонент ResourceCooldown
            cooldown = closestTarget.GetComponent<ResourceCooldown>();

            // Только если это ресурс (руда) — проверяем расстояние вручную
            if (cooldown != null)
            {
                float confirmedDistance = Vector3.Distance(transform.position, closestTarget.position);
                if (confirmedDistance > interactionRange)
                {
                    ClearInteraction();
                    return; // Слишком далеко от руды — не показываем кнопку
                }
            }

            if (currentTarget != closestTarget)
            {
                ClearInteraction();
                currentTarget = closestTarget;

                Renderer rend = currentTarget.GetComponentInChildren<Renderer>();
                if (rend != null)
                {
                    currentOutline = currentTarget.gameObject.AddComponent<Outline>();
                    currentOutline.OutlineMode = Outline.Mode.OutlineAll;
                    currentOutline.OutlineColor = Color.white;
                    currentOutline.OutlineWidth = 20f;
                }

                objectNameText.text = currentTarget.name;
                objectNameText.gameObject.SetActive(true);
                // По умолчанию считаем, что можно показывать кнопку
                bool canInteract = true;

                // Проверка на ResourceCooldown
                cooldown = currentTarget.GetComponent<ResourceCooldown>();
                if (cooldown != null)
                {
                    if (!cooldown.IsAvailable)
                    {
                        canInteract = false;
                    }

                    // ДОБАВЬ: Проверка дистанции до руды
                    float distanceToOre = Vector3.Distance(transform.position, currentTarget.position);
                    if (distanceToOre > interactionRange)
                    {
                        canInteract = false;
                    }
                }

                if (currentTarget.CompareTag("Tower") && !buildMode)
                {
                    canInteract = false;
                    objectNameText.gameObject.SetActive(false); // скрываем название башни
                }

                // Отображаем кнопку, если можно взаимодействовать
                interactionButton.SetActive(canInteract);

                // Проверка на дверь
                Door door = currentTarget.GetComponent<Door>();
                if (door != null)
                {
                    ClearKeyIcons();

                    // Узнаем, сколько ключей у игрока
                    int playerKeys = player.inventory.GetItemCount(door.keyItem.id); // предполагается, что keyItem — это Item

                    for (int i = 0; i < door.keysRequired; i++)
                    {
                        GameObject icon = Instantiate(keyIconPrefab, keyIconContainer);

                        Image iconImage = icon.GetComponent<Image>();
                        if (iconImage != null)
                        {
                            // Покрасим в черный, если ключа не хватает
                            iconImage.color = (i < playerKeys) ? Color.white : Color.black;
                        }
                    }
                }
                else
                {
                    ClearKeyIcons();
                }
            }
            else
            {
                // even if target не изменился — обнови доступность взаимодействия (напр., cooldown прошёл или buildMode сменился)
                bool canInteract = true;

                cooldown = currentTarget.GetComponent<ResourceCooldown>();
                if (cooldown != null)
                {
                    if (!cooldown.IsAvailable || Vector3.Distance(transform.position, currentTarget.position) > interactionRange)
                    {
                        canInteract = false;
                    }
                }

                if (currentTarget.CompareTag("Tower") && !buildMode)
                {
                    canInteract = false;
                    objectNameText.gameObject.SetActive(false);
                }

                interactionButton.SetActive(canInteract);
            }

            // Обновление позиции текста и иконок
            Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTarget.position + Vector3.up * 3f);
            objectNameText.transform.position = screenPos;
            keyIconContainer.position = screenPos + new Vector3(0, -30f, 0);

            cooldown = currentTarget.GetComponent<ResourceCooldown>();
            if (cooldown != null && !cooldown.IsAvailable)
            {
                float secondsLeft = Mathf.Ceil(cooldown.TimeRemaining);
                int minutes = Mathf.FloorToInt(secondsLeft / 60);
                int seconds = Mathf.FloorToInt(secondsLeft % 60);
                objectNameText.text = string.Format("{0:D2}:{1:D2}", minutes, seconds);
            }
            else
            {
                interactionText.text = "";
            }

            interactionText.gameObject.SetActive(false);
        }
        else
        {
            ClearInteraction();
            ClearKeyIcons(); // <-- чтобы иконки исчезали, когда уходим от двери
        }
    }

    public void StartInteraction()
    {
        if (!isGatheringNow)
        {
            isInteracting = true;

            if (currentTarget.CompareTag("BlueOre"))
            {
                StartCoroutine(GatherResource(currentTarget, 1));
            }
            else if (currentTarget.CompareTag("RedOre"))
            {
                StartCoroutine(GatherResource(currentTarget, 2));
            }
            else if (currentTarget.CompareTag("Door"))
            {
                Door door = currentTarget?.GetComponent<Door>();
                if (door != null)
                {
                    Inventory inv = FindObjectOfType<Inventory>();
                    if (inv != null)
                    {
                        door.TryOpen(inv);
                        return;
                    }
                }
            }
            else if (currentTarget.CompareTag("Shop"))
            {
                FindObjectOfType<AudioManager>().Play("Menu");
                shopUI.SetActive(true);
            }
            else if (currentTarget.CompareTag("Forge"))
            {
                FindObjectOfType<AudioManager>().Play("Menu");
                forgeUI.SetActive(true);
            }
            else if (currentTarget.CompareTag("Mage"))
            {
                FindObjectOfType<AudioManager>().Play("Menu");
                mageUI.SetActive(true);
            }
            else if (currentTarget.CompareTag("Storage"))
            {
                FindObjectOfType<AudioManager>().Play("Menu");
                storageUI.SetActive(true);
            }
            else if (currentTarget.CompareTag("TowerSlot"))
            {
                PlaceTower();
            }
            else if (currentTarget.CompareTag("Tower") && buildMode)
            {
                FindObjectOfType<AudioManager>().Play("Menu");
                Transform towerTransform = currentTarget.parent;

                MonoBehaviour towerScript = towerTransform.GetComponent<Turret>() as MonoBehaviour ??
                                            towerTransform.GetComponent<DamageAuraTower>() as MonoBehaviour ??
                                            towerTransform.GetComponent<MagicTower>() as MonoBehaviour ??
                                            towerTransform.GetComponent<SlowTower>() as MonoBehaviour;

                if (towerScript != null)
                {
                    OpenTowerUpgradeUI(towerScript);
                }
                else
                {
                    Debug.LogWarning("Не найден скрипт башни на объекте: " + towerTransform.name);
                }
            }
        }
    }

    void PlaceTower()
    {
        FindObjectOfType<AudioManager>().Play("Menu");
        towerUI.SetActive(true); // Открываем UI выбора башен
    }

    public void BuildTower(int i, int cost)
    {
        switch (i)
        {
            case 0:
                BuildTower(towerPrefab1, cost, "Blue Ore");
                break;
            case 1:
                BuildTower(towerPrefab2, cost, "Red Ore");
                break;
            case 2:
                BuildTower(towerPrefab3, cost, "Blue Ore");
                break;
            case 3:
                BuildTower(towerPrefab4, cost, "Red Ore");
                break;
        }
    }

    void BuildTower(GameObject towerPrefab, int cost, string requiredOreName)
    {
        if (currentTarget != null && currentTarget.CompareTag("TowerSlot") && player.moneyBalance >= cost)
        {
            // Ищем нужную руду по имени
            InventorySlot oreSlot = player.inventory.slots.Find(slot =>
                slot.item.itemData.itemType == ItemType.Ore &&
                slot.item.itemData.itemName == requiredOreName);

            if (oreSlot != null && oreSlot.amount >= (cost / 50))
            {
                oreSlot.amount = oreSlot.amount - cost / 50;

                if (oreSlot.amount <= 0)
                {
                    player.inventory.RemoveItem(oreSlot.item);
                }

                player.SpendMoney(cost);
                Instantiate(towerPrefab, currentTarget.position, Quaternion.identity);
                Destroy(currentTarget.gameObject);
                towerUI.SetActive(false);
                player.inventory.onInventoryChanged.Invoke();

                Debug.Log($"Башня построена: {towerPrefab.name}, использован ресурс {requiredOreName}.");
                FindObjectOfType<AudioManager>().Play("Build");
            }
            else
            {
                player.ShowMessage("Not enough ore!");
            }
        }
        else if (player.moneyBalance < cost) player.ShowMessage("Not enough money!");
    }

    void EndInteraction()
    {
        isInteracting = false;

        if (currentTarget.CompareTag("Resource"))
        {
            StopCoroutine("GatherResource");
        }
        
        else if (currentTarget.CompareTag("TowerSlot"))
        {
            FindObjectOfType<AudioManager>().Play("Menu");
            towerUI.SetActive(false);
        }
        else if (currentTarget.CompareTag("Shop"))
        {
            FindObjectOfType<AudioManager>().Play("Menu");
            shopUI.SetActive(false);
        }
        else if (currentTarget.CompareTag("Forge"))
        {
            FindObjectOfType<AudioManager>().Play("Menu");
            forgeUI.SetActive(false);
        }
        else if (currentTarget.CompareTag("Mage"))
        {
            FindObjectOfType<AudioManager>().Play("Menu");
            mageUI.SetActive(false);
        }
        else if (currentTarget.CompareTag("Storage"))
        {
            FindObjectOfType<AudioManager>().Play("Menu");
            storageUI.SetActive(false);
        }

        interactionText.gameObject.SetActive(false);
    }

    IEnumerator GatherResource(Transform resource, int type)
    {
        Debug.Log("GatherResource запущена");

        if (FindObjectOfType<PlayerSkills>().isTurret) yield break;
        if (FindObjectOfType<PlayerSkills>().isTeleporting) yield break;
        if (isGatheringNow) yield break;
        isGatheringNow = true;

        ResourceCooldown cooldown = resource.GetComponent<ResourceCooldown>();

        if (cooldown != null && !cooldown.isOnCooldown)
        {
            player.RotateTowards(resource);
            yield return new WaitForSeconds(0.5f);

            animator.SetBool("IsGathering", true);
            isInteracting = true;
            player.digging = true;

            try
            {
                yield return new WaitForSeconds(5f); // время добычи

                if (Vector3.Distance(transform.position, resource.position) <= interactionRange-1)
                {
                    if (cooldown != null)
                        cooldown.StartCooldown();

                    Debug.Log("Ресурс добыт: " + resource.name);

                    Vector3 spawnPos = Vector3.Lerp(resource.position, transform.position, 0.5f) + Vector3.up * 0.5f;
                    if (type == 1 && resourcePrefabBlue != null)
                    {
                        Instantiate(resourcePrefabBlue, spawnPos, Quaternion.identity);
                    }
                    else if (type == 2 && resourcePrefabRed != null)
                    {
                        Instantiate(resourcePrefabRed, spawnPos, Quaternion.identity);
                    }
                }
            }
            finally
            {
                // Гарантированный сброс флагов
                animator.SetBool("IsGathering", false);
                player.digging = false;
                isGatheringNow = false;
                EndInteraction();
            }
        }
        else
        {
            isGatheringNow = false;
        }
    }

    IEnumerator ReactivateResource(GameObject resource, float delay)
    {
        yield return new WaitForSeconds(delay);
        resource.SetActive(true);
    }

    void OpenTowerUpgradeUI(MonoBehaviour towerScript)
    {
        towerUpgradeUI.SetActive(true);
        selectedTowerScript = towerScript;

        int towerLevel = 1;

        if (towerScript is Turret turret)
        {
            towerLevel = turret.towerLevel;
            towerIconImage.sprite = turretSprite;
        }
        else if (towerScript is DamageAuraTower damageAura)
        {
            towerLevel = damageAura.towerLevel;
            towerIconImage.sprite = damageAuraSprite;
        }
        else if (towerScript is MagicTower magic)
        {
            towerLevel = magic.towerLevel;
            towerIconImage.sprite = magicTowerSprite;
        }
        else if (towerScript is SlowTower slow)
        {
            towerLevel = slow.towerLevel;
            towerIconImage.sprite = slowTowerSprite;
        }

        towerLevelText.text = "LVL: " + towerLevel;

        if (towerScript is Turret || towerScript is SlowTower)
            upgradeCost = 50;
        else if (towerScript is DamageAuraTower || towerScript is MagicTower)
            upgradeCost = 100;

        towerUpgradeCostText.text = upgradeCost.ToString();

        if (towerLevel < 3) upgradeButton.interactable = true;
        else
        {
            upgradeButton.interactable = false;
            upgradeCost = 0;
            towerUpgradeCostText.text = " - ";
        }

        
        towerUpgradeCostText.color = (player.moneyBalance >= upgradeCost) ? Color.white : Color.red;


        UpdateStarVisuals(towerLevel);
    }

    public void UpgradeSelectedTower()
    {
        if (selectedTowerScript == null) return;

        int currentLevel = 1;

        if (selectedTowerScript is Turret turret)
            currentLevel = turret.towerLevel;
        else if (selectedTowerScript is DamageAuraTower damageAura)
            currentLevel = damageAura.towerLevel;
        else if (selectedTowerScript is MagicTower magic)
            currentLevel = magic.towerLevel;
        else if (selectedTowerScript is SlowTower slow)
            currentLevel = slow.towerLevel;
        
        int nextLevel = currentLevel + 1;

        int cost = upgradeCost;

        if (player.moneyBalance >= cost)
        {
            player.SpendMoney(cost);

            if (selectedTowerScript is Turret)
                ((Turret)selectedTowerScript).towerLevel = nextLevel;
            else if (selectedTowerScript is DamageAuraTower)
                ((DamageAuraTower)selectedTowerScript).towerLevel = nextLevel;
            else if (selectedTowerScript is MagicTower)
                ((MagicTower)selectedTowerScript).towerLevel = nextLevel;
            else if (selectedTowerScript is SlowTower)
                ((SlowTower)selectedTowerScript).towerLevel = nextLevel;

            player.ShowMessage($"Tower upgraded to level {nextLevel}");
            UpdateStarVisuals(nextLevel);
            towerLevelText.text = "LVL: " + nextLevel;
        }
        else
        {
            player.ShowMessage("Not enough money!");
        }

        // Пересчитать стоимость на следующий уровень
        int nextUpgradeCost = upgradeCost;
        towerUpgradeCostText.text = nextUpgradeCost.ToString();


        if (nextLevel < 3) upgradeButton.interactable = true;
        else
        {
            upgradeButton.interactable = false;
            upgradeCost = 0;
            towerUpgradeCostText.text = " - ";
        }

        

        towerUpgradeCostText.color = (player.moneyBalance >= nextUpgradeCost) ? Color.white : Color.red;
    }

    void TryUpgradeTower(MonoBehaviour towerScript, int targetLevel)
    {
        int currentLevel = 1;

        if (towerScript is Turret turret)
            currentLevel = turret.towerLevel;
        else if (towerScript is DamageAuraTower damageAura)
            currentLevel = damageAura.towerLevel;
        else if (towerScript is MagicTower magic)
            currentLevel = magic.towerLevel;
        else if (towerScript is SlowTower slow)
            currentLevel = slow.towerLevel;

        if (targetLevel <= currentLevel) return;

        int levelsToAdd = targetLevel - currentLevel;
        int cost = levelsToAdd * upgradeCost;

        if (player.moneyBalance >= cost)
        {
            player.SpendMoney(cost);

            if (towerScript is Turret)
                ((Turret)towerScript).towerLevel = targetLevel;
            else if (towerScript is DamageAuraTower)
                ((DamageAuraTower)towerScript).towerLevel = targetLevel;
            else if (towerScript is MagicTower)
                ((MagicTower)towerScript).towerLevel = targetLevel;
            else if (towerScript is SlowTower)
                ((SlowTower)towerScript).towerLevel = targetLevel;

            player.ShowMessage($"Tower level now is {targetLevel}");
            UpdateStarVisuals(targetLevel);
            towerLevelText.text = "LVL: " + targetLevel;

            
        }
        else
        {
            player.ShowMessage("Not enough money!");
        }
        if (targetLevel < 3) upgradeButton.interactable = true;
        else
        {
            upgradeButton.interactable = false;
            upgradeCost = 0;
            towerUpgradeCostText.text = " - ";
        }
    }

    void UpdateStarVisuals(int level)
    {
        for (int i = 0; i < starButtons.Length; i++)
        {
            Image img = starButtons[i].GetComponent<Image>();
            if (img != null)
            {
                img.sprite = (i < level) ? filledStarSprite : emptyStarSprite;
            }
        }

        if (level < 3) upgradeButton.interactable = true;
        else
        {
            upgradeButton.interactable = false;
            upgradeCost = 0;
            towerUpgradeCostText.text = " - ";
        }
    }

    void ClearInteraction()
    {
        if (currentOutline != null)
        {
            Destroy(currentOutline);
            currentOutline = null;
        }

        objectNameText.gameObject.SetActive(false);
        interactionButton.SetActive(false);
        interactionText.gameObject.SetActive(false);
        currentTarget = null;
    }

    void ClearKeyIcons()
    {
        foreach (Transform child in keyIconContainer)
        {
            Destroy(child.gameObject);
        }
    }
}