using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class ForestEnemyAI : MonoBehaviour
{
    public Transform campCenter;       // Центр лагеря
    public float campRadius = 15f;     // Радиус лагеря
    private float detectRadius = 5f;   // Радиус обнаружения игрока
    private float atkRadius = 2f;      // Радиус атаки
    private float returnRadius = 18f;  // Радиус возврата в лагерь
    private Animator anim;
    public int HP;                // Здоровье юнита
    public int MaxHP;
    public int damage;            // Урон юнита
    public LayerMask playerLayer;      // Слой игрока
    bool canMove = true;

    private NavMeshAgent agent;
    private Transform playerTransform; // Игрок
    private bool isReturning = false;  // Флаг возврата в лагерь
    private EnemySound enemySound;
    public GameObject hitEffectPrefab; // Префаб эффекта удара
    public float dropChance = 0.2f;         // Шанс выпадения предмета (например, 20%)
    public bool isDead = false;

    public float moveSpeed = 3.5f;
    public float circleDrawDistance = 10f;
    public float circleRadius = 1f;
    public int circleSegments = 32;
    public float atk_radius = 1.5f;

    public LineRenderer circleRenderer;

    [Header("Health Bar")]
    public GameObject healthBarPrefab;  // Префаб полосы здоровья
    private GameObject healthBar;       // Экземпляр полосы здоровья
    private Slider healthSlider;        // Слайдер для отображения здоровья
    private Camera mainCamera;          // Камера для UI

    public float knockbackDuration = 0.2f; // Длительность отталкивания
    private bool isKnockedBack = false;   // Флаг отталкивания

    private void Start()
    {
        MaxHP = HP;
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
        mainCamera = Camera.main;

        // Создание полосы здоровья
        healthBar = Instantiate(healthBarPrefab, GameObject.Find("Canvas").transform);
        healthSlider = healthBar.GetComponentInChildren<Slider>();
        healthSlider.maxValue = HP;
        healthSlider.value = HP;
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        if (circleRenderer == null)
            return;

        circleRenderer.positionCount = segments + 1;
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            Vector3 pos = new Vector3(x, 0.1f, z) + center;
            circleRenderer.SetPosition(i, pos);
            angle += 360f / segments;
        }
    }

    void Update()
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

        agent.speed = moveSpeed; // Устанавливаем скорость
        UpdateHealthBar();       // Обновляем полосу здоровья
        FindClosestPlayer();     // Ищем ближайшего игрока

        // Отрисовка круга при обнаружении игрока
        if (playerTransform != null && Vector3.Distance(playerTransform.position, transform.position) <= circleDrawDistance)
        {
            DrawCircle(transform.position, circleRadius, circleSegments);
            circleRenderer.enabled = true;
        }
        else
        {
            circleRenderer.enabled = false;
        }

        // Проверяем игроков и строения в радиусе
        Collider[] cols = Physics.OverlapSphere(transform.position, detectRadius, playerLayer);

        if (cols.Length > 0 && canMove)
        {
            Transform player = cols[0].transform;
            RotateTowards(player);

            if (Vector3.Distance(transform.position, player.position) <= atk_radius)
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
            anim.SetBool("RUN", false);
            anim.SetBool("Attack", false);
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthSlider.value = HP;

            if (HP <= 0)
            {
                Destroy(healthBar);  // Удаляем полосу здоровья, если враг умер
                return;
            }

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(transform.position + new Vector3(0, 2, 0));

            // Если юнит в лагере, скрываем полосу здоровья
            if (isReturning && Vector3.Distance(transform.position, campCenter.position) <= 2f)
            {
                healthBar.SetActive(false);
            }
            else
            {
                healthBar.SetActive(true);
            }
            healthBar.transform.position = screenPosition;

        }
    }

    void FindClosestPlayer()
    {
        Collider[] playersInRange = Physics.OverlapSphere(transform.position, detectRadius, playerLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (Collider player in playersInRange)
        {
            RotateTowards(player.transform);
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                closestPlayer = player.transform;
            }
        }

        playerTransform = closestPlayer;
    }

    void EngagePlayer()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) <= atkRadius)
        {
            agent.SetDestination(transform.position);
            anim.SetBool("RUN", false);
            anim.SetBool("Attack", true);
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) <= campRadius)
        {
            agent.SetDestination(playerTransform.position);
            anim.SetBool("RUN", true);
            anim.SetBool("Attack", false);
        }
        else
        {
            ReturnToCamp();
        }
    }

    void ReturnToCamp()
    {
        isReturning = true;
        agent.SetDestination(campCenter.position);
        anim.SetBool("RUN", true);
        anim.SetBool("Attack", false);
    }

    void Patrol()
    {
        if (Vector3.Distance(transform.position, campCenter.position) > returnRadius)
        {
            ReturnToCamp();
        }
        else
        {
            anim.SetBool("RUN", false);
            anim.SetBool("Attack", false);
        }
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        FindObjectOfType<AudioManager>().Play("Hit");

        if (enemySound != null)
        {
            enemySound.PlayRandomDamageSound();  // Воспроизвести случайный звук получения урона
        }

        // Показываем эффект удара
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f); // Уничтожаем эффект через 0.5 секунды
        }

        if (HP <= 0)
        {
            Dead();
        }

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
        if (playersInRange.Length > 0)
        {
            Transform player = playersInRange[0].transform;
            MovementInput playerHealth = player.GetComponent<MovementInput>();
            playerHealth.AddMoney(1);
            playerHealth.GainXP(MaxHP);
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
    }

    // Метод для уничтожения полосы здоровья при смерти
    void DestroyHealthBar()
    {
        if (healthBar != null)
        {
            Destroy(healthBar);  // Уничтожаем объект полосы здоровья
        }
    }

    void RotateTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Если враг далеко — плавный поворот, если близко — мгновенный
        if (Vector3.Distance(transform.position, target.position) > 1.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
        else
        {
            transform.rotation = lookRotation; // Мгновенный поворот вблизи
        }
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
}