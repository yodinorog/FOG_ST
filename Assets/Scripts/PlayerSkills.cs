using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerSkills : MonoBehaviour
{
    public Image[] skillImages; // Изображения навыков для показа заполненности
    public Image[] skillPlusImages; // Кнопки "+" для прокачки
    public Text availablePointsText; // Текст для отображения доступных очков
    public Button upgradeToggleButton; // Кнопка для отображения "+" кнопок
    private bool upgradeButtonsVisible = false;
    public Button[] skillButtons; // Кнопки активных способностей

    private int[] skillLevels = new int[4]; // Текущий уровень каждого навыка
    private int availablePoints = 1; // Доступные очки для прокачки
    private int lastSkillIndex = -1; // Индекс последнего прокаченного навыка

    private int[] skillWaitPoints = new int[4]; // Очки ожидания для каждого навыка

    // cooldowns для способностей (например, телепорт)
    public Image[] cooldownImages; // Радиа́льные круги отката
    public Text[] cooldownTexts; // Текст секундного таймера
    private float[] cooldownDurations = new float[4] { 15f, 20f, 30f, 90f }; // AOE, Boost, Teleport, Turret // Длительности откатов (можно изменить по необходимости)
    private float[] cooldownTimers = new float[4];
    private bool[] isCooldownActive = new bool[4];

    private MovementInput movementInput;
    public EnemyAI closest = null;
    public GameObject FXs;

    public ParticleSystem TeleportEffectObject;

    public Slider turretTimerSlider;

    void Start()
    {
        movementInput = FindObjectOfType<MovementInput>();
        

        for (int i = 0; i < cooldownImages.Length; i++)
        {
            if (cooldownImages[i] != null)
                cooldownImages[i].fillAmount = 0f;

            if (cooldownTexts[i] != null)
                cooldownTexts[i].text = "";
        }

        UpdateUI();

        for (int i = 0; i < skillPlusImages.Length; i++)
        {
            skillPlusImages[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Обновление состояния турели
        if (isTurret)
        {
            movementInput.RotateTowards(closest.transform);
            Collider[] enemies = Physics.OverlapSphere(transform.position, turretRadius, enemyLayer);
            FXs.SetActive(enemies.Length > 0);
        }

        // Отключаем кнопки, если нельзя использовать способности
        bool shouldDisableSkills = isTurret || isTeleporting || movementInput.digging;
        SetSkillButtonsInteractable(!shouldDisableSkills);


        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (isCooldownActive[i])
            {
                cooldownTimers[i] -= Time.deltaTime;

                if (cooldownTimers[i] <= 0f)
                {
                    cooldownTimers[i] = 0f;
                    isCooldownActive[i] = false;

                    if (cooldownImages[i] != null)
                        cooldownImages[i].fillAmount = 0f;

                    if (cooldownTexts[i] != null)
                        cooldownTexts[i].text = "";

                    cooldownImages[i].gameObject.SetActive(false);
                }
                else
                {
                    if (cooldownImages[i] != null)
                        cooldownImages[i].fillAmount = cooldownTimers[i] / cooldownDurations[i];

                    if (cooldownTexts[i] != null)
                        cooldownTexts[i].text = Mathf.Ceil(cooldownTimers[i]).ToString();
                }
            }
        }

        for (int i = 0; i < skillPlusImages.Length; i++)
        {
            bool canUpgrade = availablePoints > 0 && CanUpgrade(i);
            skillPlusImages[i].gameObject.SetActive(upgradeButtonsVisible && canUpgrade);
        }
    }

    private void SetSkillButtonsInteractable(bool interactable)
    {
        foreach (var button in skillButtons)
        {
            if (button != null)
                button.interactable = interactable;
        }
    }

    public void ToggleUpgradeButtons()
    {
        upgradeButtonsVisible = !upgradeButtonsVisible;
    }

    public void LevelUp()
    {
        availablePoints++;
        UpdateUI();
    }

    public void UpgradeSkill(int skillIndex)
    {
        if (availablePoints <= 0 || !CanUpgrade(skillIndex))
            return;

        availablePoints--;
        skillLevels[skillIndex]++;
        lastSkillIndex = skillIndex;
        if (skillIndex == 3) skillWaitPoints[3] = skillWaitPoints[3] - 5;
        else skillWaitPoints[skillIndex] = 0;

        UpdateSkillUI(skillIndex);
        UpdateUI();
        ToggleUpgradeButtons();
    }

    private bool CanUpgrade(int skillIndex)
    {
        int current = skillLevels[skillIndex];
        int maxLevel = (skillIndex == 3) ? 4 : 6;

        // уже максимум?
        if (current >= maxLevel) return false;

        // нельзя дважды подряд
        if (skillIndex == lastSkillIndex)
            return false;

        if (skillIndex == 3) // Ульта
        {
            // Ограничения по уровню
            if (current == 0 && movementInput.currentLevel < 6) return false;
            if (current == 1 && movementInput.currentLevel < 12) return false;
            if (current == 2 && movementInput.currentLevel < 18) return false;
            if (current == 3 && movementInput.currentLevel < 24) return false;

            // Ограничение по очкам ожидания
            if (skillWaitPoints[3] < 5) return false;
        }

        return true;
    }

    private void UpdateSkillUI(int skillIndex)
    {
        int maxLevel = (skillIndex == 3) ? 4 : 6;
        int level = skillLevels[skillIndex];

        skillImages[skillIndex].fillAmount = Mathf.Clamp01((float)level / maxLevel);

        if (level >= maxLevel)
        {
            skillPlusImages[skillIndex].gameObject.SetActive(false);
        }
        else
        {
            skillPlusImages[skillIndex].gameObject.SetActive(availablePoints > 0 && CanUpgrade(skillIndex));
        }
    }

    private void UpdateUI()
    {
        availablePointsText.text = $"{availablePoints}";

        if (upgradeToggleButton != null)
        {
            bool hasUpgradeableSkill = false;

            for (int i = 0; i < skillLevels.Length; i++)
            {
                if (CanUpgrade(i))
                {
                    hasUpgradeableSkill = true;
                    break;
                }
            }

            upgradeToggleButton.gameObject.SetActive(availablePoints > 0 && hasUpgradeableSkill);
        }

        for (int i = 0; i < skillLevels.Length; i++)
        {
            UpdateSkillUI(i);

            if (cooldownImages[i] != null)
            {
                if (skillLevels[i] <= 0)
                {
                    cooldownImages[i].gameObject.SetActive(true);
                    cooldownImages[i].fillAmount = 1f;

                    if (cooldownTexts[i] != null)
                        cooldownTexts[i].text = "";
                }
                else if (!isCooldownActive[i])
                {
                    cooldownImages[i].gameObject.SetActive(false);
                }
            }

            if (i != lastSkillIndex)
            {
                if (i == 3 && movementInput.currentLevel < 2) { }
                else skillWaitPoints[i]++;
            }
        }

        if (availablePoints == 0 && upgradeButtonsVisible)
        {
            upgradeButtonsVisible = false;
            for (int i = 0; i < skillPlusImages.Length; i++)
            {
                skillPlusImages[i].gameObject.SetActive(false);
            }
        }
    }

    private void StartCooldown(int index)
    {
        cooldownTimers[index] = cooldownDurations[index];
        isCooldownActive[index] = true;

        if (cooldownImages[index] != null)
            cooldownImages[index].gameObject.SetActive(true);
    }

    public Transform basePosition; // Позиция базы
    public LayerMask enemyLayer;
    public LineRenderer turretRadiusRenderer;

    public GameObject playerModel;
    public GameObject turretModel;
    
    public float boostSpeed;
    public float turretRadius = 4f;

    private Coroutine turretRoutine;
    public bool isTurret = false;
    public bool isTeleporting = false;

    // КРУГОВАЯ АТАКА (AOE)
    private float[] aoeDamageMultiplier = { 2f, 2f, 2f, 2f, 2f, 2f};
    private float[] aoeCooldowns = { 15f, 13f, 11f, 9f, 7f, 5f};
    private float[] aoeRadiusLevels = { 2.5f, 3.0f, 3.5f, 4.0f, 4.5f, 5.0f};

    // УСКОРЕНИЕ (BOOST)
    private float[] boostSpeedLevels = { 0.5f, 0.8f, 1.1f, 1.4f, 1.7f, 2.0f};
    private float[] boostCooldowns = { 20f, 19f, 18f, 17f, 16f, 15f};
    private float[] boostDurations = { 5f, 6f, 7f, 8f, 9f, 10f };

    // ТЕЛЕПОРТ
    private float[] teleportDelays = { 2.5f, 2.0f, 1.5f, 1f, 0.5f, 0.1f };
    private int[] teleportHeal = { 10, 40, 70, 100, 130, 160 };
    private float[] teleportCooldowns = { 45f, 38f, 31f, 24f, 17f, 10f };

    // ТУРЕЛЬ (УЛЬТИМАТИВА)
    private float[] turretFireRates = { 0.14f, 0.11f, 0.08f, 0.05f};
    private float[] turretDurations = { 3f, 3.5f, 4f, 4.5f};
    private float[] turretCooldowns = { 90f, 80f, 70f, 60f};

    public ParticleSystem AOEEffectObject; // Игровой объект, представляющий эффект
    public GameObject SpeedEffectObject; // Игровой объект, представляющий эффект


    public void Cast(int i)
    {
        int lvl = skillLevels[i];
        if (lvl <= 0 || isCooldownActive[i]) return;

        switch (i)
        {
            case 0: // AOE
                movementInput.isAttacking = false;

                cooldownDurations[i] = aoeCooldowns[lvl - 1];
                float aoeRadius = aoeRadiusLevels[lvl - 1];
                float damageMultiplier = aoeDamageMultiplier[lvl - 1];
                DrawRadius(aoeRadius, 0.5f);
                movementInput.anim.SetTrigger("Cast");

                AOEEffectObject.Play();

                Collider[] enemies = Physics.OverlapSphere(transform.position, aoeRadius, enemyLayer);
                foreach (var enemy in enemies)
                {
                    EnemyAI e = enemy.GetComponent<EnemyAI>();
                    if (e != null)
                        e.TakeDamage(Mathf.RoundToInt(movementInput.strength * damageMultiplier));
                }

                cooldownTimers[i] = aoeCooldowns[lvl - 1];
                break;

            case 1: // Boost
                cooldownDurations[i] = boostCooldowns[lvl - 1];
                StartCoroutine(SpeedBoost(boostSpeedLevels[lvl - 1], boostDurations[lvl - 1]));
                cooldownTimers[i] = boostCooldowns[lvl - 1];
                break;

            case 2: // Teleport
                cooldownDurations[i] = teleportCooldowns[lvl - 1];
                StartCoroutine(TeleportWithDelay(i, teleportDelays[lvl - 1], teleportHeal[lvl - 1]));
                cooldownTimers[i] = teleportCooldowns[lvl - 1];
                break;

            case 3: // Turret
                cooldownDurations[i] = turretCooldowns[lvl - 1];
                movementInput.anim.SetBool("isTurret", true);
                StartTurretMode(turretDurations[lvl - 1], turretFireRates[lvl - 1]);
                cooldownTimers[i] = turretCooldowns[lvl - 1];
                break;
        }

        isCooldownActive[i] = true;
        if (cooldownImages[i] != null) cooldownImages[i].gameObject.SetActive(true);
    }

    IEnumerator TeleportWithDelay(int index, float delay, int healAmount)
    {
        movementInput.isAttacking = false;

        movementInput.anim.SetTrigger("Cast");
        TeleportEffectObject.Play();

        isTeleporting = true;

        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            controller.enabled = false;
            yield return new WaitForSeconds(delay);
            transform.position = new Vector3(0, 3, -20);
            controller.enabled = true;
            TeleportEffectObject.Stop();
        }

        movementInput.Healing(healAmount);

        isTeleporting = false;
    }

    IEnumerator SpeedBoost(float boostSpeed, float duration)
    {
        if (movementInput == null) yield break;

        float originalSpeed = 3.5f;
        movementInput.boostSpeed = boostSpeed;
        SpeedEffectObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        SpeedEffectObject.SetActive(false);
        movementInput.boostSpeed = 0f;
    }

    public void StartTurretMode(float duration, float fireRate)
    {
        if (turretRoutine == null)
            turretRoutine = StartCoroutine(TurretMode(duration, fireRate));
    }

    IEnumerator TurretMode(float duration, float fireRate)
    {
        movementInput.isAttacking = false;
        isTurret = true;
        // Прячем оружие за спину
        movementInput.MoveWeaponToBack();
        playerModel.SetActive(false);
        turretModel.SetActive(true);

        //float originalSpeed = movementInput != null ? movementInput.speed : 0f;
        //if (movementInput != null)
        //{
        //    movementInput.speed = 0f;
        //    movementInput.enabled = false;
        //}

        DrawRadius(turretRadius, duration);

        float elapsed = 0f;

        if (turretTimerSlider != null)
        {
            turretTimerSlider.gameObject.SetActive(true);
            turretTimerSlider.maxValue = duration;
            turretTimerSlider.value = duration;
        }

        while (elapsed < duration)
        {
            ShootNearestEnemy();

            float timeStep = Mathf.Min(fireRate, duration - elapsed);
            float stepElapsed = 0f;

            while (stepElapsed < timeStep)
            {
                float delta = Time.deltaTime;
                elapsed += delta;
                stepElapsed += delta;

                if (turretTimerSlider != null)
                    turretTimerSlider.value = Mathf.Clamp(duration - elapsed, 0, duration);

                yield return null;
            }
        }

        if (turretTimerSlider != null)
            turretTimerSlider.gameObject.SetActive(false);

        turretModel.SetActive(false);
        playerModel.SetActive(true);

        //if (movementInput != null)
        //{
        //    movementInput.speed = originalSpeed;
        //    movementInput.enabled = true;
        //}

        isTurret = false;
        // Возвращаем оружие в руку
        movementInput.MoveWeaponToHand();
        movementInput.anim.SetBool("isTurret", false);
        turretRoutine = null;
    }

    void ShootNearestEnemy()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, turretRadius, enemyLayer);
        if (enemies.Length == 0) return;

        
        float minDist = float.MaxValue;

        foreach (Collider col in enemies)
        {
            EnemyAI ai = col.GetComponent<EnemyAI>();
            if (ai == null || ai.isDead) continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = ai;
            }
        }

        if (closest != null)
        {
            closest.TakeDamage((int)movementInput.strength);
            Debug.Log("Турель нанесла урон: " + closest.name);
        }
    }

    private void DrawRadius(float radius, float duration)
    {
        GameObject go = new GameObject("RadiusRendererTemp");
        go.transform.position = transform.position;

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 50;
        lr.loop = true;
        lr.widthMultiplier = 0.05f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0f, 1f, 0f, 0.5f);
        lr.endColor = new Color(0f, 1f, 0f, 0.5f);
        lr.useWorldSpace = true;

        for (int i = 0; i < 50; i++)
        {
            float angle = i * (360f / 49f);
            float rad = Mathf.Deg2Rad * angle;
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * radius;
            lr.SetPosition(i, pos);
        }

        Destroy(go, duration);
    }
}
