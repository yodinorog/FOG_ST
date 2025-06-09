using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System.Linq;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private bool Ranger = false;
    [SerializeField] private float projectileSpeed = 5f;
    [SerializeField] private float projectileLifetime = 5f; // 2–3 метра на 5 м/с ≈ 0.6 сек

    private float detect_radius = 4f;       // Радиус обнаружения игрока
    private float atk_radius = 3f;          // Радиус атаки
    public float attackAngle = 45f;  // Угол обзора атаки
    private Animator anim;
    public int HP;                     // Здоровье врага
    public int MaxHP;
    public int damage;                 // Урон врага
    public LayerMask playerLayer;           // Слой игрока
    public LayerMask buildsLayer;           // Слой замка
    bool canMove = true;

    public float distanceThreshold = 2f; // Расстояние для перехода к следующей точке

    public float returnThreshold = 3f;     // Порог отклонения для возврата к точке

    int ind = 0;                            // Индекс текущей точки пути
    NavMeshAgent agent;

    public GameObject deathParticlesPrefab; // Префаб частиц, выпадающих при смерти
    public float particleAttractionSpeed = 5f; // Скорость притяжения частиц к игроку
    public GameObject itemPrefab;           // Префаб предмета для выпадения
    public float dropChance;         // Шанс выпадения предмета (например, 20%)
    public bool isDead = false;

    public Transform playerTransform;       // Ссылка на ближайшего игрока
    public LineRenderer circleRenderer;     // LineRenderer для отрисовки круга
    public float circleRadius = 1.5f;       // Радиус круга
    public int circleSegments = 50;         // Количество сегментов в круге

    private float circleDrawDistance = 8f;  // Расстояние, на котором отображается круг
    private EnemySound enemySound;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;   // Префаб полосы здоровья
    private GameObject healthBar;        // Экземпляр полосы здоровья
    private Slider healthSlider;         // Слайдер для отображения здоровья
    private Slider healthSliderB;
    private Text healthSliderText;
    private Camera mainCamera;           // Камера игрока для UI
    private Coroutine damageEffectCoroutine;

    public GameObject hitEffectPrefab; // Префаб эффекта удара
    public float knockbackDuration = 0.2f; // Длительность отталкивания
    private bool isKnockedBack = false;   // Флаг отталкивания

    public float moveSpeed = 3f;         // Скорость врага
    public float slowAmount = 1f;    // Исходная скорость врага
    public float magicSlow = 0f;
    public Color OriginalColor { get; private set; } // Исходный цвет врага

    [Header("Forest Camp Settings")]
    public Transform campCenter;
    public float campRadius = 5f;
    public float returnRadius = 5f;
    public bool isReturning = false;
    public bool isForestMob = false;    // Является ли моб лесным
    private Vector3 returnTarget;        // Точка возврата в лагерь

    public LineRenderer attackConeRenderer;
    public int coneSegments = 32;
    public float coneRadius = 3f;
    private float coneAngle = 45f;

    private Vector3 storedDirection;
    private Vector3 storedFirePosition;

    [Header("Special Enemy Types")]
    public bool isGhost = false;
    public bool isNecromancer = false;
    public bool isUndead = false;
    public bool Assasin = false;

    [Header("Necromancer Settings")]
    public GameObject[] undeadPrefabs; // Префабы для призыва
    public Transform summonPoint;      // Точка призыва
    public float summonCooldown = 10f;  // Перезарядка способности
    private float summonTimer = 0f;

    public bool boss;

    private Coroutine castRoutine;

    Transform player;

    private float lastHitTime;
    private Coroutine regenCoroutine;
    [SerializeField] private float regenRate = 2f; // HP/сек
    [SerializeField] private float regenDelay = 5f; // сек до начала регена

    void ReturnToCamp()
    {
        isReturning = true;
        agent.SetDestination(campCenter.position);
        anim.SetBool("RUN", true);
        anim.SetBool("Attack", false);
        HP = MaxHP;
    }

    private void Start()
    {
        player = FindObjectOfType<MovementInput>()?.transform;
        if (isGhost)
        {
            anim.SetBool("GhostIdle", true); // Не забудь добавить в аниматор состояние для призрака
        }
        MaxHP = HP;
        OriginalColor = GetComponentInChildren<Renderer>().material.color;

        agent = GetComponent<NavMeshAgent>();
        //isForestMob = name.Contains("Camp");

        mainCamera = Camera.main;

        // Ищем объект EnemyHealthbars внутри Canvas
        Transform enemyHealthbarsContainer = GameObject.Find("Canvas").transform.Find("EnemyHealthbars");
        if (enemyHealthbarsContainer == null)
        {
            Debug.LogError("EnemyHealthbars object not found in Canvas!");
            return;
        }

        // Инстанциируем полоску здоровья как дочерний объект EnemyHealthbars
        healthBar = Instantiate(healthBarPrefab, enemyHealthbarsContainer);
        healthBar.SetActive(false); // Скрываем сразу после создания

        // Получаем компоненты полосок и текста
        Slider[] sliders = healthBar.GetComponentsInChildren<Slider>();
        healthSlider = sliders[1];     // Основной слайдер
        healthSliderB = sliders[0];    // Задний слайдер (например, для задержки урона)
        healthSliderText = healthBar.GetComponentInChildren<Text>();

        // Устанавливаем значения
        healthSliderB.maxValue = HP;
        healthSlider.maxValue = HP;
        healthSlider.value = HP;

        anim = GetComponentInChildren<Animator>();
        if (anim == null)
            Debug.LogError("Аниматор не найден у объекта врага или его дочерних объектов.");

        enemySound = GetComponent<EnemySound>();

        // Отрисовка круга
        circleRenderer = gameObject.AddComponent<LineRenderer>();
        circleRenderer.positionCount = circleSegments + 1;
        circleRenderer.loop = true;
        circleRenderer.startWidth = 0.1f;
        circleRenderer.endWidth = 0.1f;
        circleRenderer.material = new Material(Shader.Find("Sprites/Default"));
        circleRenderer.startColor = Color.green;
        circleRenderer.endColor = Color.green;
        circleRenderer.enabled = false;

        // Конус — на дочернем объекте
        GameObject coneObj = new GameObject("AttackConeRenderer");
        coneObj.transform.parent = this.transform;
        coneObj.transform.localPosition = Vector3.zero;
        attackConeRenderer = coneObj.AddComponent<LineRenderer>();
        attackConeRenderer.positionCount = coneSegments + 2;
        attackConeRenderer.loop = true;
        attackConeRenderer.startWidth = 0.05f;
        attackConeRenderer.endWidth = 0.05f;
        attackConeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        attackConeRenderer.startColor = Color.red;
        attackConeRenderer.endColor = Color.red;
        attackConeRenderer.enabled = false;

        if (isGhost) anim.SetBool("isGhost", true);
    }

    float timerUpdate;
    void Update()
    {
        
        timerUpdate += Time.deltaTime;
        if (timerUpdate >= 0.02f)
        {
            timerUpdate = 0;
            CustomUpdate(); // самописный метод вместо Update
        }
    }

    void CustomUpdate()
    {
        agent.speed = moveSpeed * slowAmount + magicSlow;
        UpdateHealthBar();
        FindClosestPlayer();

        if (isForestMob && campCenter != null)
        {
            if (isReturning)
            {
                agent.SetDestination(campCenter.position);
                if (Vector3.Distance(transform.position, campCenter.position) <= 2f)
                {
                    isReturning = false;
                    agent.SetDestination(transform.position);
                    anim.SetBool("RUN", false);
                }
                return;
            }

            if (Vector3.Distance(transform.position, campCenter.position) > returnRadius)
            {
                ReturnToCamp();
                return;
            }
        }

        if (boss)
        {
            anim.speed = 0.6f;
            atk_radius = 4f;
            attackAngle = 75f;
        }

        

        if (playerTransform != null && Vector3.Distance(playerTransform.position, transform.position) <= circleDrawDistance)
        {
            DrawEnemyCircle(transform.position, circleRadius, circleSegments);
            circleRenderer.enabled = true;
        }
        else
        {
            circleRenderer.enabled = false;
        }
         
        Collider[] cols = Physics.OverlapSphere(transform.position, atk_radius-1f, playerLayer);
        if (isForestMob)
        {
            cols = Physics.OverlapSphere(transform.position, detect_radius, playerLayer);
            if (Ranger) cols = Physics.OverlapSphere(transform.position, detect_radius, playerLayer);
        }
        else if (Ranger) cols = Physics.OverlapSphere(transform.position, atk_radius * 1.5f, playerLayer);

        Collider[] colsBuilds = Physics.OverlapSphere(transform.position, 9999f, buildsLayer);

        if (isNecromancer)
        {
            summonTimer -= Time.deltaTime;
        }

        if (summonTimer <= 0f && undeadPrefabs.Length > 0)
        {
            SummonUndead();
            summonTimer = summonCooldown;
            anim.SetTrigger("Summon"); // Уникальная анимация призыва
            agent.SetDestination(transform.position); // Прекращает движение
            anim.SetBool("RUN", false);
            anim.SetBool("Attack", false);
            return; // Приоритет выше атаки
        }

        if (cols.Length > 0 && canMove)
        {
            Transform player = cols[0].transform;
            RotateTowards(player);
            if (Ranger)
            { 
                if (Vector3.Distance(transform.position, player.position) <= atk_radius*2)
                {
                    agent.SetDestination(transform.position);
                    anim.SetBool("RUN", false);
                    anim.SetBool("Attack", true);
                }
                else
                {
                    agent.SetDestination(player.position);
                    anim.SetBool("RUN", true);
                    anim.SetBool("Attack", false);
                }
            }
            else
            {
                if (Vector3.Distance(transform.position, player.position) <= atk_radius-1f)
                {
                    agent.SetDestination(transform.position);
                    anim.SetBool("RUN", false);
                    anim.SetBool("Attack", true);
                }
                else
                {
                    agent.SetDestination(player.position);
                    anim.SetBool("RUN", true);
                    anim.SetBool("Attack", false);
                }
            }
        }
        else if (isForestMob && campCenter != null && canMove)
        {
            float distanceToCenter = Vector3.Distance(transform.position, campCenter.position);
            if (distanceToCenter > campRadius)
            {
                agent.SetDestination(campCenter.position);
                anim.SetBool("RUN", true);
                anim.SetBool("Attack", false);
            }
            else
            {
                agent.SetDestination(transform.position);
                anim.SetBool("RUN", false);
                anim.SetBool("Attack", false);
            }
        }
        else if (colsBuilds.Length > 0 && canMove)
        {
            Transform build = colsBuilds[0].transform;
            if (Vector3.Distance(transform.position, build.position) <= atk_radius)
            {
                RotateTowards(build);

                agent.SetDestination(transform.position);
                anim.SetBool("RUN", false);
                anim.SetBool("Attack", true);
            }
            else
            {
                agent.SetDestination(build.position);
                anim.SetBool("RUN", true);
                anim.SetBool("Attack", false);
            }
        }
        else
        {
            anim.SetBool("RUN", false);
            anim.SetBool("Attack", false);
        }

        if (boss && canMove) TryUseBossAbilities();
    }


    void SummonUndead()
    {
        if (summonPoint == null)
        {
            summonPoint = this.transform; // по умолчанию, если не задана
        }

        int index = UnityEngine.Random.Range(0, undeadPrefabs.Length);
        GameObject summon = Instantiate(undeadPrefabs[index], summonPoint.position + Vector3.right * UnityEngine.Random.Range(-1f, 1f), Quaternion.identity);

        Debug.Log("Некромант призвал: " + summon.name);
    }

    public void ModifySpeed(float multiplier)
    {
        slowAmount = multiplier; // ✅ корректно
    }

    void UpdateHealthBar()
    {
        if (healthBar == null || mainCamera == null) return;

        
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Скрыть, если далеко
        if (distanceToPlayer > 15f)
        {
            if (healthBar.activeSelf)
                healthBar.SetActive(false);
            return;
        }

        // Показать, если в радиусе
        if (!healthBar.activeSelf)
            healthBar.SetActive(true);

        // Обновление значений
        healthSlider.value = HP;
        healthSliderText.text = HP.ToString();

        if (damageEffectCoroutine != null)
            StopCoroutine(damageEffectCoroutine);

        damageEffectCoroutine = StartCoroutine(SmoothBackHealth(healthSliderB.value, HP));

        // Позиционирование над врагом
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position + Vector3.up * 2f);

        // Доп. защита: не показывать, если враг за камерой
        if (screenPosition.z < 0)
        {
            healthBar.SetActive(false);
            return;
        }

        healthBar.transform.position = screenPosition;
    }

    private IEnumerator SmoothBackHealth(float startValue, float targetValue)
    {
        float duration = 0.7f; // Время, за которое белая полоса догоняет текущее здоровье
        float elapsed = 0f;

        Image sliderImage = healthSliderB.fillRect.GetComponent<Image>();
        Color originalColor = sliderImage.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            healthSliderB.value = Mathf.Lerp(startValue, targetValue, t);
            sliderImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f - t * 0.5f); // чуть затемняется при проигрывании

            yield return null;
        }

        healthSliderB.value = targetValue;
        sliderImage.color = originalColor;
        damageEffectCoroutine = null;
    }

    // Метод для уничтожения полосы здоровья при смерти
    void DestroyHealthBar()
    {
        if (healthBar != null)
        {
            Destroy(healthBar);  // Уничтожаем объект полосы здоровья
        }
    }

    // Метод для поиска ближайшего игрока
    void FindClosestPlayer()
    {
        const float radius = 5f;
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, radius, playerLayer);
        Transform bestTarget = null;
        float minSqrDist = float.MaxValue;
        Vector3 currentPosition = transform.position;

        for (int i = 0; i < playersInRange.Length; i++)
        {
            Vector3 dirToTarget = playersInRange[i].transform.position - currentPosition;
            float dSqrToTarget = dirToTarget.sqrMagnitude;
            if (dSqrToTarget < minSqrDist)
            {
                minSqrDist = dSqrToTarget;
                bestTarget = playersInRange[i].transform;
            }
        }

        if (bestTarget != null)
            playerTransform = bestTarget;
    }

    void ShootArrow()
    {
        GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        arrow.transform.position = storedFirePosition;
        arrow.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f); // В 2 раза больше
        arrow.name = "Arrow";

        Collider[] enemyColliders = FindObjectsOfType<EnemyAI>()
            .Select(e => e.GetComponent<Collider>())
            .Where(c => c != null)
            .ToArray();

        Collider arrowCollider = arrow.GetComponent<Collider>();
        foreach (Collider enemyCol in enemyColliders)
            Physics.IgnoreCollision(arrowCollider, enemyCol); // Игнорируем врагов

        Rigidbody rb = arrow.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.velocity = storedDirection * projectileSpeed;

        Destroy(arrow, projectileLifetime);

        ArrowDamage damage = arrow.AddComponent<ArrowDamage>();
        damage.damage = this.damage;
    }

    public class ArrowDamage : MonoBehaviour
    {
        public int damage;

        private void OnCollisionEnter(Collision collision)
        {
            // Игнорируем врагов
            if (collision.gameObject.GetComponent<EnemyAI>()) return;

            // Наносим урон игроку
            if (collision.gameObject.CompareTag("Player"))
            {
                MovementInput movementInput = collision.gameObject.GetComponent<MovementInput>();
                if (movementInput != null)
                    movementInput.TakeDamage(damage);
            }
            if (collision.gameObject.CompareTag("Builds"))
            {
                CastleScript castle = collision.gameObject.GetComponent<CastleScript>();
                if (castle != null)   
                    castle.TakeDamage(damage);
            }

            Destroy(gameObject); // Уничтожаем стрелу при любом другом столкновении
        }
    }

    private void Attack()
    {
        if (Ranger)
        {
            ShootArrow();
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 forward = transform.forward;
        float atkRadiusSqr = atk_radius * atk_radius;
        float halfAngle = attackAngle * 0.5f;

        bool hitSomething = false;

        Collider[] targets = Physics.OverlapSphere(origin, atk_radius, playerLayer | buildsLayer);

        foreach (Collider col in targets)
        {
            Vector3 toTarget = col.transform.position - origin;
            toTarget.y = 0;
            if (toTarget.sqrMagnitude > atkRadiusSqr) continue;

            float angle = Vector3.Angle(forward, toTarget.normalized);
            if (angle > halfAngle) continue;

            var player = col.GetComponent<MovementInput>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hitSomething = true;
                break;
            }

            var castle = col.GetComponent<CastleScript>();
            if (castle != null)
            {
                castle.TakeDamage(damage);
                hitSomething = true;
                break;
            }
        }

        if (hitSomething)
            FlashConeColor(Color.white);

        if (!isForestMob) Wait(1f);
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"Получен урон: {damage} от {gameObject.name}");
        HP -= damage;
        lastHitTime = Time.time;

        // Сброс регенерации при получении урона
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;
        }

        FindObjectOfType<AudioManager>().Play("Hit");

        if (enemySound != null)
        {
            enemySound.PlayRandomDamageSound();
        }

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }

        if (HP <= 0)
        {
            Dead();
        }
        else if (isForestMob)
        {
            regenCoroutine = StartCoroutine(RegenerateHP());
        }
    }

    private IEnumerator RegenerateHP()
    {
        yield return new WaitForSeconds(regenDelay);

        while (HP < MaxHP)
        {
            // Если урон был недавно — остановить
            if (Time.time - lastHitTime < regenDelay)
            {
                regenCoroutine = null;
                yield break;
            }

            HP += Mathf.CeilToInt(regenRate * Time.deltaTime);
            HP = Mathf.Min(HP, MaxHP);
            UpdateHealthBar();
            yield return null;
        }

        regenCoroutine = null;
    }

    public void ApplyKnockback(Vector3 sourcePosition, float force)
    {
        if (isKnockedBack) return; // Если враг уже отталкивается, выходим

        Vector3 knockbackDirection = (transform.position - sourcePosition).normalized;
        StartCoroutine(KnockbackRoutine(knockbackDirection, force));
    }

    private IEnumerator KnockbackRoutine(Vector3 direction, float force)
    {
        isKnockedBack = true;

        float timer = 0f;
        while (timer < knockbackDuration)
        {
            agent.Move(direction * (force * Time.deltaTime)); // Двигаем врага
            timer += Time.deltaTime;
            yield return null;
        }

        isKnockedBack = false;
    }

    void Dead()
    {
        if (isDead) return;  // чтобы не вызывался повторно
        isDead = true;

        // Останавливаем врага
        agent.speed = 0;
        agent.isStopped = true;

        // Удаление полосы здоровья и анимаций
        DestroyHealthBar();
        anim.SetBool("RUN", false);
        anim.SetBool("Attack", false);
        anim.SetBool("dead", true);

        // Даем награды игроку
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, 9999f, playerLayer);
        if (playersInRange.Length > 0 && !isUndead)
        {
            Transform player = playersInRange[0].transform;
            MovementInput playerHealth = player.GetComponent<MovementInput>();
            if (isForestMob)
            {
                playerHealth.AddMoney(MaxHP/2);
                playerHealth.GainXP(MaxHP*2);
            }
            else
            {
                playerHealth.AddMoney(MaxHP/10);
                playerHealth.GainXP(MaxHP/5);
            }
        }

        // Проверка на выпадение предметов
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        if (randomValue <= dropChance)
        {
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }

        // Воспроизведение звука и задержка уничтожения
        //if (enemySound != null)
        //{
        //    enemySound.PlayRandomDeathSound();
        //    StartCoroutine(WaitForDeathSound(enemySound.GetDeathSoundLength()));
        //}
        //else
        //{
            Destroy(gameObject); // если звуков нет — уничтожаем сразу
        //}

        if (CompareTag("FBoss"))
        {
            MovementInput playerScript = GameObject.FindGameObjectWithTag("Player")?.GetComponent<MovementInput>();
            if (playerScript != null)
            {
                playerScript.ShowVictoryUI();
            }
        }
    }

    public void SpawnParticles()
    {
        // Спавн частиц смерти
        GameObject particles = Instantiate(deathParticlesPrefab, transform.position, Quaternion.identity);

        // Привлечение частиц к игроку
        ParticleAttraction particleAttraction = particles.GetComponent<ParticleAttraction>();
        if (particleAttraction != null)
        {
            particleAttraction.SetTarget(GameObject.FindWithTag("Player").transform);
        }
    }

    IEnumerator WaitForDeathSound(float duration)
    {
        yield return new WaitForSeconds(duration);  // Ждем до завершения звука
        SpawnParticles();
        Destroy(gameObject);  // Уничтожаем врага после проигрывания звука
    }

    IEnumerator Wait(float duration)
    {
        yield return new WaitForSeconds(duration);  // Ждем до завершения звука
    }

    void AttackEnd()
    {
        attackConeRenderer.enabled = false;
        canMove = true;
    }

    void AttackStart()
    {
        if (Ranger)
        {
            DrawRectAttackZone();
            attackConeRenderer.enabled = true;
        }
        else
        {
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 forward = transform.forward;
            DrawAttackCone(origin, forward);
        }

        canMove = false;
    }

    void DrawEnemyCircle(Vector3 center, float radius, int segments)
    {
        if (circleRenderer == null) return;

        circleRenderer.positionCount = segments + 1;
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            Vector3 point = new Vector3(center.x + x, center.y + 0.05f, center.z + z); // Чуть выше земли
            circleRenderer.SetPosition(i, point);
        }

        circleRenderer.startColor = Color.green;
        circleRenderer.endColor = Color.green;
        circleRenderer.startWidth = 0.05f;
        circleRenderer.endWidth = 0.05f;
        circleRenderer.loop = true;
        circleRenderer.enabled = true;
    }

    GameObject DrawCircle(Vector3 pos, float radius, Color color, float duration)
    {
        GameObject circle = new GameObject("AbilityCircle");
        LineRenderer lr = circle.AddComponent<LineRenderer>();

        lr.useWorldSpace = true;
        lr.loop = true;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;

        int segments = 40;
        lr.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 point = new Vector3(x, 0.05f, z) + pos;
            lr.SetPosition(i, point);
        }

        Destroy(circle, duration);
        return circle;
    }

    void RecolorCircle(GameObject circleObject, Color newColor)
    {
        if (circleObject == null) return;

        LineRenderer lr = circleObject.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.startColor = newColor;
            lr.endColor = newColor;
        }
    }

    private Color defaultConeColor = new Color(1f, 0.5f, 0f, 0.5f); // оранжевый по умолчанию
    private Coroutine coneFlashRoutine;

    void DrawAttackCone(Vector3 origin, Vector3 forward)
    {
        if (attackConeRenderer == null) return;

        int coneSegments = 30;
        float coneAngle = attackAngle;
        float coneRadius = atk_radius;

        attackConeRenderer.positionCount = coneSegments + 2;
        attackConeRenderer.SetPosition(0, origin);

        float halfAngle = coneAngle / 2f;
        for (int i = 0; i <= coneSegments; i++)
        {
            float angle = -halfAngle + (coneAngle / coneSegments) * i;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 direction = rotation * forward;
            Vector3 point = origin + direction * coneRadius;
            attackConeRenderer.SetPosition(i + 1, point);
        }

        attackConeRenderer.startColor = defaultConeColor;
        attackConeRenderer.endColor = defaultConeColor;
        attackConeRenderer.enabled = true;
    }

    void FlashConeColor(Color flashColor)
    {
        if (attackConeRenderer == null) return;

        if (coneFlashRoutine != null)
            StopCoroutine(coneFlashRoutine);

        coneFlashRoutine = StartCoroutine(FlashRoutine(flashColor));
    }

    IEnumerator FlashRoutine(Color flashColor)
    {
        attackConeRenderer.startColor = flashColor;
        attackConeRenderer.endColor = flashColor;

        yield return new WaitForSeconds(0.2f);

        attackConeRenderer.startColor = defaultConeColor;
        attackConeRenderer.endColor = defaultConeColor;
    }

    void DrawRectAttackZone()
    {
        storedDirection = transform.forward;
        storedFirePosition = transform.position + storedDirection * 1.5f + Vector3.up;

        float width = 1.0f;
        float length = atk_radius * 2;

        Vector3 origin = transform.position + storedDirection * (length / 2f);
        Vector3 right = transform.right;

        Vector3[] corners = new Vector3[5];
        corners[0] = origin + (-right * width / 2) - (storedDirection * length / 2);
        corners[1] = origin + (right * width / 2) - (storedDirection * length / 2);
        corners[2] = origin + (right * width / 2) + (storedDirection * length / 2);
        corners[3] = origin + (-right * width / 2) + (storedDirection * length / 2);
        corners[4] = corners[0];

        attackConeRenderer.positionCount = corners.Length;
        attackConeRenderer.SetPositions(corners);
    }

    void RotateTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Плавный поворот к цели
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    #region Boss Abilities

    public GameObject aoeEffectPrefab;
    public GameObject burnEffectPrefab;
    public GameObject healEffectPrefab;
    public GameObject meteorEffectPrefab;

    private bool isUsingAbility = false;
    private float abilityCooldown1 = 10f;
    private float abilityCooldown2 = 20f;
    private float abilityCooldown3 = 40f;
    private float abilityCooldown4 = 60f;
    private float nextAbilityCheckTime = 0f;

    private float lastAbility1Time = -Mathf.Infinity;
    private float lastAbility2Time = -Mathf.Infinity;
    private float lastAbility3Time = -Mathf.Infinity;
    private float lastAbility4Time = -Mathf.Infinity;

    private float abilityPauseBetween = 5f;

    private void TryUseBossAbilities()
    {
        if (!boss || isUsingAbility || Time.time < nextAbilityCheckTime) return;

        if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) <= atk_radius * 2)
        {
            if (MaxHP >= 5000 && Time.time - lastAbility4Time >= abilityCooldown4)
            {
                StartCoroutine(UseMeteorShower());
                nextAbilityCheckTime = Time.time + abilityPauseBetween;
            }
            else if (MaxHP >= 2500 && Time.time - lastAbility3Time >= abilityCooldown3)
            {
                StartCoroutine(UseAreaHeal());
                nextAbilityCheckTime = Time.time + abilityPauseBetween;
            }
            else if (MaxHP >= 1000 && Time.time - lastAbility2Time >= abilityCooldown2)
            {
                StartCoroutine(UseBurningAura());
                nextAbilityCheckTime = Time.time + abilityPauseBetween;
            }
            else if (MaxHP >= 500 && Time.time - lastAbility1Time >= abilityCooldown1)
            {
                StartCoroutine(UseAOEAttack());
                nextAbilityCheckTime = Time.time + abilityPauseBetween;
            }
        }
    }

    IEnumerator UseAOEAttack()
    {
        isUsingAbility = true;
        canMove = false;

        yield return StartCoroutine(StartCasting(1f));
        yield return new WaitForSeconds(0.5f);

        if (aoeEffectPrefab)
        {
            GameObject aoe = Instantiate(aoeEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
            aoe.GetComponent<ParticleSystem>()?.Play();
            Destroy(aoe, 3f);
        }

        DrawRadiusIndicator(transform.position, atk_radius, Color.red, 2f);
        yield return new WaitForSeconds(1.5f);

        Collider[] cols = Physics.OverlapSphere(transform.position, atk_radius, playerLayer);
        foreach (var col in cols)
            col.GetComponent<MovementInput>()?.TakeDamage(damage);

        isUsingAbility = false;
        canMove = true;
        lastAbility1Time = Time.time;
    }

    IEnumerator UseBurningAura()
    {
        if (isUsingAbility) yield break;
        isUsingAbility = true;
        canMove = false;

        yield return StartCoroutine(StartCasting(1f));

        float duration = 3f;
        float tick = 0.05f;

        // Визуальный эффект (частицы)
        GameObject burn = null;
        if (burnEffectPrefab)
        {
            burn = Instantiate(burnEffectPrefab, transform.position, Quaternion.identity);
            var ps = burn.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(burn, duration);
        }

        // Отрисовка круга
        DrawRadiusIndicator(transform.position, atk_radius, Color.red, duration);

        // Запускаем урон синхронно с эффектами
        float elapsed = 0f;
        Collider[] hits = new Collider[20];
        int hitCount;

        while (elapsed < duration)
        {
            hitCount = Physics.OverlapSphereNonAlloc(transform.position, atk_radius, hits, playerLayer);
            for (int i = 0; i < hitCount; i++)
            {
                MovementInput target = hits[i].GetComponent<MovementInput>();
                if (target != null)
                {
                    target.TakeDamage(3);
                }
            }

            yield return new WaitForSeconds(tick);
            elapsed += tick;
        }

        isUsingAbility = false;
        canMove = true;
        lastAbility2Time = Time.time;
    }

    IEnumerator UseAreaHeal()
    {
        isUsingAbility = true;
        canMove = false;

        yield return StartCoroutine(StartCasting(1f));

        if (healEffectPrefab)
        {
            GameObject heal = Instantiate(healEffectPrefab, transform.position, Quaternion.identity);
            heal.GetComponent<ParticleSystem>()?.Play();
            Destroy(heal, 3f);
        }

        DrawRadiusIndicator(transform.position, atk_radius * 2, Color.green, 3f);

        float duration = 3f;
        float tick = 0.1f;
        float elapsed = 0f;

        List<Collider> cached = new List<Collider>(20);
        LayerMask enemyMask = LayerMask.GetMask("Enemy");

        while (elapsed < duration)
        {
            Physics.OverlapSphereNonAlloc(transform.position, atk_radius * 2, cached.ToArray(), enemyMask);
            foreach (var col in cached)
            {
                if (col == null) continue;
                var ai = col.GetComponent<EnemyAI>();
                if (ai != null && !ai.isDead)
                    ai.HP = Mathf.Min(ai.MaxHP, ai.HP + 5);
            }

            elapsed += tick;
            yield return new WaitForSeconds(tick);
        }

        isUsingAbility = false;
        canMove = true;
        lastAbility3Time = Time.time;
    }

    IEnumerator UseMeteorShower()
    {
        isUsingAbility = true;
        canMove = false;

        yield return StartCoroutine(StartCasting(1f));

        float duration = 6f;
        float interval = 0.5f;
        float delayBeforeImpact = 2f;
        float radius = 3f;
        float range = 7f;

        List<Vector3> positions = new List<Vector3>();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 candidate = playerTransform.position + new Vector3(
                UnityEngine.Random.Range(-range, range), 0, UnityEngine.Random.Range(-range, range)
            );

            bool tooClose = false;
            foreach (var pos in positions)
            {
                if (Vector3.Distance(candidate, pos) < radius+0.2f)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                positions.Add(candidate);
                StartCoroutine(DoMeteorImpact(candidate, radius, delayBeforeImpact));
                elapsed += interval;
                yield return new WaitForSeconds(interval);
            }

            
        }

        yield return new WaitForSeconds(delayBeforeImpact + 0.5f);
        isUsingAbility = false;
        canMove = true;
        lastAbility4Time = Time.time;
    }

    IEnumerator DoMeteorImpact(Vector3 pos, float radius, float delay)
    {
        // Спавн эффекта метеорита на позиции
        if (meteorEffectPrefab != null)
        {
            GameObject meteor = Instantiate(meteorEffectPrefab, pos, Quaternion.identity);
            Destroy(meteor, delay + 0.1f); // автоматическое уничтожение
        }

        yield return new WaitForSeconds(delay);

        Collider[] hits = Physics.OverlapSphere(pos, radius, playerLayer);
        foreach (var col in hits)
        {
            MovementInput player = col.GetComponent<MovementInput>();
            if (player != null)
                player.TakeDamage(75);
        }
    }

    IEnumerator StartCasting(float duration)
    {
        if (anim != null)
        {
            anim.SetBool("RUN", false);
            anim.SetBool("Attack", false);
            anim.SetBool("Cast", true);
        }

        agent.SetDestination(transform.position);
        agent.isStopped = true;

        yield return new WaitForSeconds(duration);

        agent.isStopped = false;

        if (anim != null)
            anim.SetBool("Cast", false);
    }

    private GameObject DrawRadiusIndicator(Vector3 center, float radius, Color color, float duration = 3f)
    {
        GameObject obj = new GameObject("RadiusIndicator");
        LineRenderer lr = obj.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.widthMultiplier = 0.1f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = color;

        int segments = 40;
        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0.05f, Mathf.Sin(angle)) * radius + center;
            lr.SetPosition(i, pos);
        }

        Destroy(obj, duration);
        return obj;
    }

    #endregion
}