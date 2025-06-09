using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI; // Для работы с UI
using UnityEngine.SceneManagement;
using System.Linq;

[RequireComponent(typeof(CharacterController))]
public class MovementInput : MonoBehaviour
{
    [SerializeField] public Image weaponSlotIcon;  // Иконка слота для оружия
    [SerializeField] public GameObject weaponSlotUI;  // UI для слота оружия
    public Transform weaponSlot; // Слот для размещения оружия в руке персонажа
    public int currentWeaponUpgrade = 0;
    public int ActualWeaponID = 0;
    private int lastWeaponID = -1;
    public int lastCombatWeaponID = 0;
    public Sprite WeaponModeUI;
    public Sprite BuildModeUI;
    public Button BuildModeButton;
    public int CastleHP;
    public Text enemyDebugText; // ← сюда перетащи EnemyDebugText из Canvas через инспектор

    public int moneyBalance; // Баланс денег

    public Text moneyText; // UI для отображения денег

    public int remaining;

    public float Velocity;
    public GameObject Inventory;
    public Button InventoryButton;
    public Sprite openInventorySprite; // Спрайт для кнопки, когда инвентарь открыт
    public Sprite closeInventorySprite; // Спрайт для кнопки, когда инвентарь закрыт
    private bool isRolling = false;

    public bool buildMode = false;

    [Space]
    public float InputX;
    public float InputZ;
    public Vector3 desiredMoveDirection;
    public bool blockRotationPlayer;
    public float desiredRotationSpeed = 0.1f;
    public Animator anim;
    public float allowPlayerRotation = 0.1f;
    public Camera cam;
    public CharacterController controller;
    public bool isGrounded;
    public float Speed;

    public ParticleSystem attackEffectObject;
    public ParticleSystem attackEffectObject1;
    public ParticleSystem attackEffectObject2;// Игровой объект, представляющий эффект
    public ParticleSystem damageEffectObject; // Игровой объект, представляющий эффект

    public ParticleSystem effectObject; // Игровой объект, представляющий эффект
    public ParticleSystem HealeffectObject;

    public Inventory inventory;  // Ссылка на инвентарь игрока
    public Text messageText; // Назначь в инспекторе
    private Coroutine messageRoutine;

    private Coroutine effectCoroutine;

    private float detectionRadius = 4f;    // Радиус обнаружения врагов
    private float smallDetectionRadius = 2.5f; // Малый радиус для поворота к врагу
    private float attackRadius = 2.5f;
    public LayerMask enemyLayer;           // Слой врагов
    private NavMeshAgent agent;            // NavMeshAgent персонажа
    public bool isAttacking = false;      // Флаг атаки
    public bool isDead = false;

    [Header("Player Stats")]
    private int maxLevel = 25;              // Максимальный уровень
    public int currentLevel = 1;           // Текущий уровень
    public float currentXP = 0f;           // Текущий опыт
    public float xpToNextLevel = 100f;     // Опыт для повышения уровня
    public Slider sliderXP;                // UI-элемент Slider для опыта

    public int currentHealth;              // Текущее здоровье
    public int maxHealth;                  // Максимальное здоровье на уровне
    public Slider sliderHP;                // UI-элемент Slider для здоровья

    public float baseSpeed;
    private float speed;         // Текущая скорость
    public int strength;    // Сила персонажа

    [Header("UI Text Elements")]
    public Text xpText;                    // Текст для отображения опыта (сейчас/необходимо)
    public Text healthText;                // Текст для отображения здоровья (фактическое/максимальное)
    public Text levelText;                 // Текст для отображения уровня

    public Text statsText; // UI элемент для вывода характеристик

    [Header("Animation Smoothing")]
    [Range(0, 1f)] public float HorizontalAnimSmoothTime = 0.2f;
    [Range(0, 1f)] public float VerticalAnimTime = 0.2f;
    [Range(0, 1f)] public float StartAnimTime = 0.3f;
    [Range(0, 1f)] public float StopAnimTime = 0.15f;

    public float verticalVel;
    private Vector3 moveVector;

    public float interactionRange = 2.0f;  // Дальность взаимодействия
    public LayerMask items;                // Слой для предметов
    public Text description;               // UI элемент для описания предметов

    private Vector3 lastMoveDirection = Vector3.zero; // Сохраняем последнее направление движения
    private float currentSpeed = 0f; // Текущая скорость

    private GameObject activeUseText;
    private GameObject activeGifImage;
    public GameObject useTextPrefab;      // Префаб текста "Использовать"
    public GameObject gifImagePrefab;     // Префаб GIF-изображения для использования

    [Header("Dash Settings")]
    public float dashDistance = 1f;        // Максимальная длина рывка
    public float dashSpeed = 0.4f;        // Базовая скорость dasha
    public float stopDistance = 1f;       // Минимальное расстояние до врага
    public float knockbackForce = 0.5f; // Сила отталкивания врага
    public LayerMask obstacleLayer;  // Слой препятствий для проверки

    [Header("Combo System")]
    public int comboStage = 0;              // Текущий этап комбо (0 = нет, 1-3 этапы комбо)
    private float comboResetTimer;
    public float comboResetTime = 1.5f;
    [SerializeField] private float comboEndDelay = 0.5f; // Задержка после завершения комбо
    private bool isComboOnCooldown = false;            // Флаг задержки после комбо
    public ComboBarController comboBar;

    private PlayerInteraction playerint;

    public Slider castleSlider;
    public Image castleFillImage; // Присвой в инспекторе компонент Image из CastleSlider.Fill
    private Color defaultCastleColor; // Исходный цвет
    private Coroutine castleDamageFlashCoroutine;
    public Text castleHPText;
    public CanvasGroup defeatCanvas;  // Присваивается вручную
    public Text defeatText;           // Присваивается вручную
    public Text emeraldsText;         // ← Новый текст для отображения изумрудов
    public Button mainMenuButton;     // ← Кнопка для возврата в главное меню

    private CharacterCustomization characterCustomization;
    private WaveManager waveManager;
    private bool resultDisplayed = false;

    public bool digging = false;
    
    // Массив для хранения оружия (GameObject)
    public GameObject[] weaponsPrefabs;

    // Индекс текущего оружия
    public int currentWeaponIndex = -1;
    public GameObject currentWeapon;
    public GameObject SavecurrentWeapon = null;

    public GameObject Map;

    private Transform nearestEnemy;

    private bool isAttackHeld = false;
    private bool isComboLocked = false; // для 1 секунды блокировки

    private bool isInComboSequence = false;
    private bool waitingForNextAttack = false;

    private Coroutine comboRoutine;

    [SerializeField] private float attackDelay = 1.6f;
    [SerializeField] private float comboRestartDelay = 0.1f;

    private bool attackReleaseCooldown = false;

    public Button attackButton; // Присвой её в инспекторе
    public float autoClickInterval = 0.1f; // Пауза между "кликами"
    private Coroutine autoClickRoutine;

    private bool isInLava = false;
    private float lavaTime = 0f;
    private float lavaTickTimer = 0f;

    public ParticleSystem healEffectObject;
    private float lastDamageTime = -Mathf.Infinity;
    private bool isRegenerating = false;

    public Transform weaponBackSlot; // слот на спине
    private bool weaponOnBack = false;
    public GameObject sellConfirmationPanel;

    public int currentBootsIndex;
    public Image bootsSlotIcon;
    public float speedBonus = 0f;
    public float boostSpeed = 0f;

    public void SwapBoots(int i)
    {
        if (i == currentBootsIndex)
        {
            Debug.Log("Эти ботинки уже надеты");
            return;
        }

        //if (i < 0 || i >= bootsPrefabs.Length)
        //{
        //    Debug.LogWarning("Invalid boots index!");
        //    return;
        //}

        // Снимаем текущие ботинки (если не стартовые)
        if (currentBootsIndex != 0)
        {
            Item itemData = inventory.GetItemById(currentBootsIndex);
            if (itemData != null)
            {
                ItemInstance returnItem = new ItemInstance
                {
                    itemData = itemData,
                    amount = 1
                };

                int remaining = inventory.addItems(returnItem, 1);
                if (remaining > 0)
                {
                    inventory.DropItem(returnItem);
                }
            }
            else
            {
                Debug.LogWarning("ItemData не найден для текущих ботинок");
            }
        }

        // Устанавливаем текущие ботинки
        Item newBootsItem = inventory.GetItemById(i);
        if (newBootsItem is Boots newBoots)
        {
            currentBootsIndex = i;
            speedBonus = newBoots.speedBonus;
            bootsSlotIcon.sprite = newBoots.icon;
            bootsSlotIcon.enabled = true;

            Debug.Log($"Надеты ботинки {newBoots.bootsName}, новая скорость: {speed}");
        }
        else
        {
            Debug.LogWarning("Предмет не является ботинками");
            return;
        }

        // Удаляем из инвентаря, если это не стартовые
        if (i != 0)
        {
            for (int s = 0; s < inventory.slots.Count; s++)
            {
                if (inventory.slots[s].item.itemData.id == i)
                {
                    inventory.slots[s].amount--;

                    if (inventory.slots[s].amount <= 0)
                    {
                        inventory.slots.RemoveAt(s);
                    }

                    inventory.onInventoryChanged.Invoke();
                    break;
                }
            }
        }

        inventory.onInventoryChanged.Invoke(); // Обновляем UI
    }

    public void SwapWeapon(int i)
    {
        if (i == currentWeaponIndex)
        {
            Debug.Log("Это оружие уже надето");
            return;
        }

        if (i < 0 || i >= weaponsPrefabs.Length)
        {
            Debug.LogWarning("Invalid weapon index!");
            return;
        }

        // Всегда удаляем текущую модель оружия из руки
        if (currentWeapon != null)
        {
            // Только боевое оружие возвращаем в инвентарь
            if (currentWeaponIndex != 0 && currentWeaponIndex != 7 && currentWeaponIndex != 8)
            {
                Item itemData = inventory.GetItemById(currentWeaponIndex);
                if (itemData != null)
                {
                    ItemInstance returnItem = new ItemInstance
                    {
                        itemData = itemData,
                        amount = 1
                    };

                    int remaining = inventory.addItems(returnItem, 1);
                    if (remaining > 0)
                    {
                        inventory.DropItem(returnItem);
                    }
                }
                else
                {
                    Debug.LogWarning("ItemData не найден для текущего оружия");
                }
            }

            Destroy(currentWeapon); // всегда удаляем текущую модель
            FindObjectOfType<AudioManager>().Play("SwapWeapon");
        }

        // Создаём и надеваем новое оружие
        currentWeapon = Instantiate(weaponsPrefabs[i], weaponSlot.position, weaponSlot.rotation);
        currentWeapon.transform.SetParent(weaponSlot);
        currentWeapon.transform.localPosition = Vector3.zero;
        currentWeapon.transform.localRotation = Quaternion.identity;

        currentWeaponIndex = i;
        weaponOnBack = false;
        ActualWeaponID = i;

        // Удаляем надетый предмет из инвентаря (кроме стартовых инструментов)
        if (i != 0 && i != 7 && i != 8)
        {
            Item itemToRemove = inventory.GetItemById(i);
            if (itemToRemove != null)
            {
                for (int s = 0; s < inventory.slots.Count; s++)
                {
                    if (inventory.slots[s].item.itemData.id == i)
                    {
                        inventory.slots[s].amount--;

                        if (inventory.slots[s].amount <= 0)
                        {
                            inventory.slots.RemoveAt(s);
                        }

                        inventory.onInventoryChanged.Invoke();
                        break;
                    }
                }
            }
        }

        inventory.onInventoryChanged.Invoke(); // Обновим UI
    }

    public void MoveWeaponToBack()
    {
        if (currentWeapon != null && !weaponOnBack)
        {
            currentWeapon.transform.SetParent(weaponBackSlot);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
            weaponOnBack = true;
        }
    }

    public void MoveWeaponToHand()
    {
        if (currentWeapon != null && weaponOnBack)
        {
            currentWeapon.transform.SetParent(weaponSlot);
            currentWeapon.transform.localPosition = Vector3.zero;
            currentWeapon.transform.localRotation = Quaternion.identity;
            weaponOnBack = false;
        }
    }

    public void OpenMap()
    {
        Map.SetActive(true);
    }

    public void ShowVictoryUI()
    {
        if (resultDisplayed) return;
        resultDisplayed = true;

        Time.timeScale = 0f;

        if (defeatCanvas != null)
        {
            defeatCanvas.gameObject.SetActive(true);
            defeatCanvas.alpha = 1f;

            if (defeatText != null)
                defeatText.text = "VICTORY";

            if (emeraldsText != null)
                emeraldsText.text = "+100";
        }

        characterCustomization.RewardPlayer(100);
    }

    public void UpdateCastleUI(int currentHP)
    {
        

        CastleHP = currentHP;

        if (castleSlider != null)
            castleSlider.value = currentHP;

        if (castleHPText != null)
            castleHPText.text = $"{currentHP}/500";

        if (currentHP <= 0 && !resultDisplayed)
        {
            resultDisplayed = true;
            ShowDefeatUI();
        }
    }

    public void CastleTakeDamageUI()
    {
        if (castleFillImage != null)
        {
            if (castleDamageFlashCoroutine != null)
                StopCoroutine(castleDamageFlashCoroutine);
            castleDamageFlashCoroutine = StartCoroutine(FlashCastleHealthBar());

        }
    }

    private IEnumerator FlashCastleHealthBar()
    {
        float duration = 0.15f; // Время на смену цвета
        float holdTime = 0.3f;  // Время удержания красного
        float elapsed = 0f;

        // Переход к красному
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            castleFillImage.color = Color.Lerp(defaultCastleColor, Color.red, t);
            yield return null;
        }

        castleFillImage.color = Color.red;

        yield return new WaitForSeconds(holdTime);

        // Плавное возвращение к исходному цвету
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            castleFillImage.color = Color.Lerp(Color.red, defaultCastleColor, t);
            yield return null;
        }

        castleFillImage.color = defaultCastleColor;
    }

    void ShowDefeatUI()
    {
        Time.timeScale = 0f;

        int emeralds = 0;

        if (defeatCanvas != null)
        {
            defeatCanvas.gameObject.SetActive(true);
            defeatCanvas.alpha = 1f;

            if (defeatText != null)
                defeatText.text = "DEFEAT";

            
            if (waveManager != null)
            {
                float time = waveManager.GetElapsedTime();
                emeralds = Mathf.Min(50, Mathf.FloorToInt(time / 36f));
            }

            if (emeraldsText != null)
                emeraldsText.text = $"+{emeralds}";
        }

        characterCustomization.RewardPlayer(emeralds);
    }

    void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // Восстановить время
        SceneManager.LoadScene("MainMenu"); // Убедись, что сцена добавлена в Build Settings
    }

    public void Healing(int i)
    {
        currentHealth += i;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        sliderHP.value = currentHealth;
        UpdateUI();
        //if (effectCoroutine != null) StopCoroutine(effectCoroutine); // Останавливаем предыдущий эффект, если он есть
        //effectCoroutine = StartCoroutine(PlayHealingEffect());
        HealeffectObject.Play();
    }

    void Start()
    {
        if (castleFillImage != null)
            defaultCastleColor = castleFillImage.color;

        StartCoroutine(RegenerateHealth());
        SwapWeapon(0);

        characterCustomization = FindObjectOfType<CharacterCustomization>();
        waveManager = FindObjectOfType<WaveManager>();
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        playerint = GetComponent<PlayerInteraction>(); 
        FindObjectOfType<AudioManager>().PlayMusic("Background");
        FindObjectOfType<AudioManager>().PlayMusic("Forest");
        anim = GetComponent<Animator>();
        cam = Camera.main;
        controller = GetComponent<CharacterController>();
        agent = GetComponent<NavMeshAgent>();
        inventory = GetComponent<Inventory>();

        // Инициализация здоровья и опыта
        UpdateHealth();
        UpdateXP();
        UpdateUI(); // Обновляем начальные значения UI

        moneyBalance = 100;
    }

    void UpdateEnemyDebugUI()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        if (enemyDebugText != null)
        {
            if (enemiesInRange.Length == 0)
            {
                enemyDebugText.text = "Враги не обнаружены.";
            }
            else
            {
                string list = "Враги рядом:\n";
                foreach (var col in enemiesInRange)
                {
                    list += $"- {col.gameObject.name}\n";
                }
                enemyDebugText.text = list;
            }
        }
    }

    void Update()
    {
        if (FindObjectOfType<PlayerSkills>().isTurret) speed = 0f;
        else speed = baseSpeed + speedBonus + boostSpeed;

        if (isAttacking) lastDamageTime = Time.time; // ⏱️ обновляем время последнего урон

        if (isInLava)
        {
            lavaTime += Time.deltaTime;
            lavaTickTimer -= Time.deltaTime;

            if (lavaTickTimer <= 0f)
            {
                int damage = 1 + Mathf.FloorToInt(lavaTime) * 1;
                TakeDamage(damage);
                lavaTickTimer = 0.3f; // урон каждую секунду
            }
        }

        if (comboStage > 0 && !isAttacking)
        {
            comboResetTimer -= Time.deltaTime;
            if (comboResetTimer <= 0f)
            {
                comboStage = 0;
                anim.SetTrigger("NotAttack"); // Возврат в состояние покоя
            }
        }

        if (!digging && !FindObjectOfType<PlayerSkills>().isTurret && !FindObjectOfType<PlayerSkills>().isTeleporting) InputMagnitude();

        // Управление положением оружия при передвижении с использованием тегов
        float blendValue = anim.GetFloat("Blend");
        AnimatorStateInfo currentState = anim.GetCurrentAnimatorStateInfo(0);

        if (blendValue > 0.3f && currentState.IsTag("Movement") && !weaponOnBack)
        {
            MoveWeaponToBack();
        }
        else if ((blendValue <= 0.3f || !currentState.IsTag("Movement")) && weaponOnBack)
        {
            MoveWeaponToHand();
        }

        // Автосмена оружия в зависимости от режима
        if (digging && currentWeaponIndex != 7)
        {
            // Сохраняем боевое оружие (не молоток и не кирка)
            if (currentWeaponIndex != 7 && currentWeaponIndex != 8)
            {
                lastCombatWeaponID = currentWeaponIndex;
            }

            SwapWeapon(7); // Надеваем кирку
        }
        else if (!digging && buildMode && currentWeaponIndex != 8)
        {
            SwapWeapon(8); // Надеваем молоток
        }
        else if (!digging && !buildMode && currentWeaponIndex != lastCombatWeaponID)
        {
            SwapWeapon(lastCombatWeaponID); // Возврат к боевому оружию
        }

        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            verticalVel = 0;
        }
        else
        {
            verticalVel -= 1;
        }
        moveVector = new Vector3(0, verticalVel * .2f * Time.deltaTime, 0);
        controller.Move(moveVector);

        // Проверка на наличие врагов в радиусе обнаружения
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        //Transform nearestEnemy = null;
        Transform nearestTower = null;

        if (enemiesInRange.Length > 0)
        {
            nearestEnemy = GetNearestEnemy(enemiesInRange);

            // Если враг находится в малом радиусе, игрок поворачивается к нему
            if (nearestEnemy != null && Vector3.Distance(transform.position, nearestEnemy.position) <= smallDetectionRadius)
            {
                RotateTowards(nearestEnemy); // Поворачиваемся к врагу
                GameObject.Find("Camera").GetComponent<Follow>().StartCombat(nearestEnemy);
                blockRotationPlayer = true;  // Блокируем поворот в сторону движения
            }
            else if (buildMode == true)
            {
                blockRotationPlayer = true; // Блокируем поворот в сторону движения
                GameObject.Find("Camera").GetComponent<Follow>().StartCombat(nearestTower);
            }
            else
            {
                blockRotationPlayer = false; // Разрешаем вращение, если враги далеко
                GameObject.Find("Camera").GetComponent<Follow>().EndCombat();
            }
        }
        else
        {
            blockRotationPlayer = false; // Разрешаем вращение при отсутствии врагов
        }

        UpdateXP();
        UpdateUI(); // Обновляем начальные значения UI
        
        HandleRaycast();
    }

    public void AddMoney(int amount)
    {
        moneyBalance += amount;
    }

    public void Sell(int amount)
    {
        moneyBalance += amount;
        //sound
    }

    public void Buy(ItemInstance itemToBuy, int amount, int pricePerUnit)
    {
        Inventory inventory = GetComponent<Inventory>();
        int totalPrice = amount * pricePerUnit;

        // Проверка баланса
        if (moneyBalance < totalPrice)
        {
            ShowMessage("Not enough money!");
            return;
        }

        // Создаем копию предмета для безопасной проверки
        ItemInstance testItem = new ItemInstance
        {
            itemData = itemToBuy.itemData,
            amount = amount
        };

        // Пробуем добавить — не записываем результат
        int remaining = inventory.addItems(testItem, amount);

        // Если все влезло — покупка прошла успешно
        if (remaining == 0)
        {
            SpendMoney(totalPrice);
            ShowMessage(testItem.itemData.itemName + " was bought");
            FindObjectOfType<AudioManager>().Play("Pickup");
        }
        else
        {
            ShowMessage("Not enough space in inventory!");
        }
    }

    public void SpendMoney(int amount)
    {
        moneyBalance -= amount;
    }

    public void ShowMessage(string message, float duration = 2f)
    {
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(ShowMessageRoutine(message, duration));
    }

    IEnumerator ShowMessageRoutine(string message, float duration)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);
        yield return new WaitForSeconds(duration);
        messageText.gameObject.SetActive(false);
    }

    public void PickaxeDig()
    {
        FindObjectOfType<AudioManager>().Play("Dig");
    }

    IEnumerator PerformRoll()
    {
        isRolling = true; // Блокируем движение
        anim.SetTrigger("Roll");

        // Длительность переката — подстроить под длительность анимации
        yield return new WaitForSeconds(1.0f);

        isRolling = false; // Разблокируем движение после завершения переката
    }


    public void RotateTowards(Transform target)
    {
        if (!isDead)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

            // Плавный поворот к цели
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);

        }
    }

    private IEnumerator DashTowards(Transform enemy)
    {
        Vector3 startPosition = transform.position;
        Vector3 direction = (enemy.position - transform.position).normalized;

        // Ограничиваем dash расстоянием
        float dashDistance = Mathf.Min(this.dashDistance, Vector3.Distance(transform.position, enemy.position) - stopDistance);

        // Рассчитываем целевую позицию
        Vector3 targetPosition = startPosition + direction * dashDistance;

        float elapsedTime = 0f;

        // Увеличиваем скорость dasha
        float dashSpeedMultiplier = 5f; // Коэффициент ускорения рывка
        float actualDashSpeed = dashSpeed * dashSpeedMultiplier;

        // Плавный dash
        while (elapsedTime < dashSpeed / actualDashSpeed)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / (dashSpeed / actualDashSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Точно устанавливаем игрока в конечной позиции
        transform.position = targetPosition;
    }

    public void PerformDash()
    {
        // Определение ближайшего врага
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        nearestEnemy = GetNearestEnemy(enemiesInRange);

        if (nearestEnemy != null)
        {
            StartCoroutine(DashTowards(nearestEnemy)); // Выполняем dash к врагу
        }
    }

    private void StartCombo()
    {
        comboStage = 1;
        isInComboSequence = true;
        isAttacking = true;

        anim.SetTrigger("Attack" + comboStage);
        RotateTowards(nearestEnemy);
    }


    private bool hasAttackEnded = false;

    public void AttackEnds()
    {
        Debug.Log("Attack завершена");
        isAttacking = false;
    }

    private IEnumerator ComboLoop()
    {
        isAttacking = true;
        isInComboSequence = true;
        comboStage = 1;

        while (isAttackHeld && nearestEnemy != null)
        {
            hasAttackEnded = false;

            RotateTowards(nearestEnemy);
            anim.SetInteger("ComboStage", comboStage); // это запускает нужную анимацию

            yield return new WaitUntil(() => hasAttackEnded); // ждём завершения удара

            comboStage++;
            if (comboStage > 3)
            {
                comboStage = 1;
                yield return new WaitForSeconds(comboRestartDelay); // пауза перед новым кругом
            }

            yield return new WaitForSeconds(0.05f); // короткая пауза от залипания
        }

        EndCombo();
    }

    private void EndCombo()
    {
        comboStage = 0;
        isAttacking = false;
        isInComboSequence = false;

        anim.SetInteger("ComboStage", 0); // обнуляем
        anim.SetBool("IsAttackHeld", false);
        anim.SetTrigger("NotAttack");
    }

    private IEnumerator LockComboInputForSeconds(float seconds)
    {
        isComboLocked = true;
        yield return new WaitForSeconds(seconds);
        isComboLocked = false;
    }

    public void PlayerAttack()
    {
        if (!FindObjectOfType<PlayerSkills>().isTurret && !FindObjectOfType<PlayerSkills>().isTeleporting)
        {
            if (isDead || isAttacking || buildMode || digging || nearestEnemy == null)
            {
                if (buildMode && playerint != null) ShowMessage("You can't fight in build mode");
                
                if (nearestEnemy == null) ShowMessage("No enemy close to you");
                return;
            }

            // Сброс, если вышли за пределы 3 атак или прошло слишком много времени
            if (comboStage >= 3 || comboResetTimer <= 0f)
            {
                comboStage = 0;
            }

            comboStage++;
            comboResetTimer = comboResetTime;

            RotateTowards(nearestEnemy);

            // Отталкивание
            EnemyAI enemyAI = nearestEnemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.ApplyKnockback(transform.position, knockbackForce);
            }

            anim.SetTrigger("Attack" + comboStage);
            isAttacking = true;
        }
    }

    private IEnumerator ResetAttackAfterDelay()
    {
        yield return new WaitForSeconds(0.5f); // Задержка — пока идёт анимация
        isAttacking = false;
    }

    public void AttackAnimation1()
    {
        attackEffectObject.Play();
        AttackRealisation();
    }

    public void AttackAnimation2()
    {
        attackEffectObject1.Play();
        AttackRealisation();
    }

    public void AttackAnimation3()
    {
        attackEffectObject2.Play();
        AttackRealisation();
    }

    public void AttackRealisation()
    {
        Collider[] enemiesHit = Physics.OverlapSphere(transform.position + transform.forward * 1.0f, attackRadius, enemyLayer);
        if (enemiesHit.Length == 0) return;

        EnemyAI closestEnemy = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider col in enemiesHit)
        {
            EnemyAI ai = col.GetComponent<EnemyAI>();
            if (ai != null && !ai.isDead)
            {
                float dist = Vector3.Distance(transform.position, ai.transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestEnemy = ai;
                }
            }
        }

        foreach (Collider col in enemiesHit)
        {
            EnemyAI ai = col.GetComponent<EnemyAI>();
            if (ai != null && !ai.isDead)
            {
                if (ai == closestEnemy)
                {
                    ai.TakeDamage(strength); // Полный урон
                }
                else
                {
                    ai.TakeDamage(Mathf.RoundToInt(strength * 0.5f)); // Половина урона
                }
            }
        }
    }

    private bool comboStartCooldown = false;

    public void StartAutoClick()
    {
        //if (autoClickRoutine == null)
        //    autoClickRoutine = StartCoroutine(AutoClickLoop());
    }

    public void StopAutoClick()
    {
        //if (autoClickRoutine != null)
        //{
        //    StopCoroutine(autoClickRoutine);
        //    autoClickRoutine = null;
        //}
    }

    IEnumerator AutoClickLoop()
    {
        while (true)
        {
            attackButton.onClick.Invoke(); // эмулируем нажатие
            yield return new WaitForSeconds(autoClickInterval);
        }
    }

    private void ResetCombo()
    {
        comboStage = 0;
        isAttacking = false;
        anim.SetInteger("ComboStage", 0);
        anim.SetBool("IsAttackHeld", false);
        anim.SetTrigger("NotAttack");
    }

    private void StartNextAttack()
    {
        if (!isAttackHeld || isAttacking || nearestEnemy == null)
            return;

        isAttacking = true;

        comboStage++;
        if (comboStage > 3)
            comboStage = 1;

        anim.SetInteger("ComboStage", comboStage);
        RotateTowards(nearestEnemy);
    }

    private IEnumerator AttackReleaseCooldown()
    {
        attackReleaseCooldown = true;
        yield return new WaitForSeconds(1f); // время блокировки после отпускания
        attackReleaseCooldown = false;
    }

    private IEnumerator Attack(Transform target)
    {
        attackEffectObject.Play(); // Включаем эффект атаки

        Collider[] enemiesHit = Physics.OverlapSphere(transform.position + transform.forward * 1.0f, attackRadius, enemyLayer);
        foreach (Collider enemy in enemiesHit)
        {
            EnemyAI enemyHealth = enemy.GetComponent<EnemyAI>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(strength);
                // break; ← Удаляем, чтобы урон нанёсся всем в зоне
            }
        }

        float attackDelay = 1f;

        if (comboBar != null)
            comboBar.StartFilling(attackDelay);

        yield return new WaitForSeconds(attackDelay);
    }

    // Метод для нахождения ближайшего врага
    Transform GetNearestEnemy(Collider[] enemies)
    {
        nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider col in enemies)
        {
            EnemyAI ai = col.GetComponentInParent<EnemyAI>();
            if (ai == null || ai.isDead) continue;

            float distance = Vector3.Distance(transform.position, ai.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = ai.transform;
            }
        }

        return nearestEnemy;
    }

    // Обновление опыта и уровня
    public void GainXP(float xp)
    {
        currentXP += xp;

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }

        // Обновить отображение на SliderXP
        UpdateXP();
        UpdateUI(); // Обновляем UI после получения опыта
    }

    public void LevelUp()
    {
        ResetAttackAfterDelay();
        if (isDead) return;
        FindObjectOfType<PlayerSkills>().LevelUp();

        currentLevel = currentLevel + 1;
        currentXP -= xpToNextLevel; // Сбрасываем опыт для нового уровня

        // Повышаем требуемый опыт для следующего уровня
        xpToNextLevel = Mathf.Floor(xpToNextLevel * 1.2f);

        // Обновляем здоровье и UI
        UpdateHealth();
        UpdateUI();

        // Запускаем эффект уровня
        //if (effectCoroutine != null) StopCoroutine(effectCoroutine); // Останавливаем предыдущий эффект, если он есть
        //effectCoroutine = StartCoroutine(PlayLevelUpEffect());

        effectObject.Play();
    }

    public void UpdateXP()
    {
        sliderXP.maxValue = xpToNextLevel;
        sliderXP.value = currentXP;
    }

    // Обновление здоровья игрока в зависимости от уровня
    void UpdateHealth()
    {
        maxHealth = 100 + (currentLevel - 1) * 20; // Увеличение здоровья с каждым уровнем
        //currentHealth = maxHealth;

        sliderHP.maxValue = maxHealth;
        sliderHP.value = currentHealth;
    }

    // Обновление UI-текстов
    public void UpdateUI()
    {
        xpText.text = $"{currentXP}/{xpToNextLevel}";    // Опыт (сейчас/необходимо)
        healthText.text = $"{currentHealth}/{maxHealth}"; // Здоровье (фактическое/максимальное)
        levelText.text = $"{currentLevel}";            // Уровень
        statsText.text = $"{strength}\n{speed:F1}";
        moneyText.text = moneyBalance.ToString();
    }

    void InputMagnitude()
    {
        if (FindObjectOfType<PlayerSkills>().isTurret || FindObjectOfType<PlayerSkills>().isTeleporting) return;
        if (isDead) return;
        //if (isAttacking) return; // блокируем передвижение во время атак
        // Получаем значения осей ввода
        InputX = SimpleInput.GetAxis("Horizontal");
        InputZ = SimpleInput.GetAxis("Vertical");

        // Рассчитываем величину ввода
        float targetSpeed = new Vector2(InputX, InputZ).sqrMagnitude;

        if (!FindObjectOfType<PlayerSkills>().isTurret) anim.speed =  speed / 3.5f;

        // Плавное приближение текущей скорости к целевой
        Speed = Mathf.Lerp(Speed, targetSpeed * speed, Time.deltaTime * 20f); // 5f - коэффициент сглаживания
        if (!isRolling && !isAttacking)
        {
            // Физическое движение персонажа
            if (Speed > allowPlayerRotation)
            {
                anim.SetFloat("Blend", Speed, StartAnimTime, Time.deltaTime);
                PlayerMoveAndRotation();
            }
            else if (Speed < allowPlayerRotation)
            {
                anim.SetFloat("Blend", Speed, StopAnimTime, Time.deltaTime);
            }
        }
    }

    void PlayerMoveAndRotation()
    {
        // Получаем оси ввода
        InputX = SimpleInput.GetAxis("Horizontal");
        InputZ = SimpleInput.GetAxis("Vertical");

        // Получаем направление от камеры
        var camera = Camera.main;
        var forward = cam.transform.forward;
        var right = cam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        // Задаем новое направление движения
        Vector3 inputDirection = forward * InputZ + right * InputX;

        // Если есть входное направление, устанавливаем его как последнее направление движения
        if (inputDirection != Vector3.zero)
        {
            lastMoveDirection = inputDirection;
            currentSpeed = Mathf.Lerp(currentSpeed, speed, Time.deltaTime * 5f); // Плавный разгон до полной скорости
        }
        else
        {
            // Если нет ввода, постепенно замедляем движение
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * 3f); // Плавное замедление
        }

        // Поворот персонажа в сторону движения, если desiredMoveDirection не нулевой
        if (!blockRotationPlayer && lastMoveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastMoveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, desiredRotationSpeed);
        }

        // Двигаем персонажа в сохраненном направлении с текущей скоростью
        controller.Move(lastMoveDirection * currentSpeed * Time.deltaTime);
    }

    private float damageAnimCooldown = 0.4f;
    private float lastDamageAnimTime = -Mathf.Infinity;

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        Handheld.Vibrate();

        lastDamageTime = Time.time; // ⏱️ обновляем время последнего урона

        damageEffectObject.Play();
        FindObjectOfType<AudioManager>().Play("RHit");

        if (Time.time - lastDamageAnimTime >= damageAnimCooldown)
        {
            anim.SetTrigger("TakeDamage");
            lastDamageAnimTime = Time.time;
            isAttacking = false;
            comboStage = 0;
            anim.SetInteger("ComboStage", 0);
            anim.ResetTrigger("Attack1");
            anim.ResetTrigger("Attack2");
            anim.ResetTrigger("Attack3");
        }

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            StartCoroutine(RespawnPlayer());
        }
        

        sliderHP.value = currentHealth;
        UpdateUI();
    }

    //public void Teleport()
    //{
    //    GetComponent<CharacterController>().enabled = false;
    //    transform.position = new Vector3(0, 3, -20);
    //    GetComponent<CharacterController>().enabled = true;
    //}

    private IEnumerator RespawnPlayer()
    {
        
        Debug.Log("Player died. Respawning...");

        isDead = true;
        anim.SetTrigger("Dead1");

        // Отключаем движение и врагов
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // Игнорируемый слой
        controller.enabled = false;

        // Выключаем оружие и анимации
        if (currentWeapon != null)
            currentWeapon.SetActive(false);

        // Ждем перед возрождением
        yield return new WaitForSeconds(4f); // Даем немного времени "смерти"

        Vector3 respawnPosition = new Vector3(0, 3, -20);
        transform.position = respawnPosition;

        currentHealth = maxHealth;
        moneyBalance = Mathf.Max(0, moneyBalance - 15);
        sliderHP.value = currentHealth;
        UpdateUI();

        // Включаем обратно
        controller.enabled = true;
        gameObject.layer = LayerMask.NameToLayer("Player");

        if (currentWeapon != null)
            currentWeapon.SetActive(true);

        isDead = false;

        Follow cameraFollow = Camera.main.GetComponent<Follow>();
        if (cameraFollow != null)
        {
            cameraFollow.player = this.transform;
            cameraFollow.EndCombat();
        }

        Debug.Log("Player respawned at (0,3,-20) with full health, -15 coins.");
    }

    IEnumerator RegenerateHealth()
    {
        while (true)
        {
            
            if (!isDead && Time.time - lastDamageTime >= 5f && currentHealth < maxHealth)
            {
                currentHealth = Mathf.Min(currentHealth + 1, maxHealth);

                if (healEffectObject != null && !healEffectObject.isPlaying)
                    healEffectObject.Play();

                sliderHP.value = currentHealth;
                UpdateUI();

                yield return new WaitForSeconds(0.3f);
            }
            else
            {
                if (healEffectObject != null && healEffectObject.isPlaying)
                    healEffectObject.Stop();

                yield return null;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Рисуем сферу радиуса атаки для отладки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.0f, 1.5f);
    }

    public void BuildModeOn()
    {
        if (FindObjectOfType<PlayerSkills>().isTurret || FindObjectOfType<PlayerSkills>().isTeleporting) return;
        BuildModeButton.image.sprite = WeaponModeUI;
        buildMode = true;
    }

    public void BuildModeOff()
    {
        if (FindObjectOfType<PlayerSkills>().isTurret || FindObjectOfType<PlayerSkills>().isTeleporting) return;
        BuildModeButton.image.sprite = BuildModeUI;
        buildMode = false;
    }

    public void OpenCloseInventory()
    {
        if (FindObjectOfType<PlayerSkills>().isTurret || FindObjectOfType<PlayerSkills>().isTeleporting) return;
        if (Inventory.activeSelf)
        {
            InventoryButton.image.sprite = openInventorySprite;
            Inventory.SetActive(false);
        }
        else
        {
            InventoryButton.image.sprite = closeInventorySprite;
            Inventory.SetActive(true);
        }
    }

    public void PickUpItem(ItemInstance item, int totalAmount)
    {
        StartCoroutine(PickUpItemCoroutine(item, totalAmount, null));
    }

    private IEnumerator PickUpItemCoroutine(ItemInstance item, int totalAmount, ItemContainer itemContainer)
    {
        Inventory inventory = GetComponent<Inventory>();

        for (int i = 0; i < totalAmount; i++)
        {
            int remaining = inventory.addItems(item, 1);

            if (remaining < 1)
            {
                ShowMessage(item.itemData.itemName + " was picked up");
                FindObjectOfType<AudioManager>().Play("Pickup");
            }
            else
            {
                ShowMessage("Not enough space in inventory!");
                inventory.DropItem(item); // ← выброс если нет места
                break;
            }

            yield return new WaitForSeconds(0.25f);
        }

        if (itemContainer != null)
        {
            Destroy(itemContainer.gameObject);
        }
    }

    void HandleRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward); // Рейкаст от позиции игрока вперед

        if (Physics.Raycast(ray, out hit, interactionRange, items))
        {
            // Если впереди предмет, показываем его имя в UI
            ItemContainer itemContainer = hit.transform.GetComponent<ItemContainer>();
            if (itemContainer != null)
            {
                description.text = itemContainer.item.itemData.itemName;
            }
        }
        else
        {
            //description.text = "";  // Очищаем текст, если предмета нет перед персонажем
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (isDead) return;

        if (other.CompareTag("Item"))
        {
            ItemContainer itemContainer = other.GetComponent<ItemContainer>();

            if (itemContainer == null || itemContainer.isBeingPickedUp)
                return;

            // Защита от немедленного подбора выброшенного самим собой предмета
            if (itemContainer.droppedBy == this.gameObject && !itemContainer.hasExitedDropper)
            {
                Debug.Log("You must leave and re-enter to pick this item.");
                return;
            }

            itemContainer.isBeingPickedUp = true;
            StartCoroutine(PickUpItemCoroutine(itemContainer.item, itemContainer.amount, itemContainer));
        }

        if (other.CompareTag("Lava"))
        {
            isInLava = true;
            lavaTime = 0f;
            lavaTickTimer = 0.5f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Lava"))
        {
            isInLava = false;
            lavaTime = 0f;
            lavaTickTimer = 0f;
        }
    }

    public void Drop(int i)
    {
        //FindObjectOfType<QuestManager>().UpdateQuestProgress(1);
        inventory.DropItem(inventory.getItem(i));
    }

    public void Use(int i)
    {
        if (FindObjectOfType<PlayerSkills>().isTurret || FindObjectOfType<PlayerSkills>().isTeleporting) return;
        if (isDead) return;
        // Получаем предмет из инвентаря
        ItemInstance itemInstance = inventory.getItem(i);

        if (itemInstance.itemData.itemType == ItemType.Armor)
        {
        }
        else
        {
            itemInstance.use(); // Используем остальные типы предметов
        }

        //inventory.RemoveItem(itemInstance); // Удаляем предмет из инвентаря
        inventory.onInventoryChanged.Invoke(); // Обновляем UI
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}