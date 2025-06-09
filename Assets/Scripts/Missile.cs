using UnityEngine;

public class Missile : MonoBehaviour
{
    private Transform target;
    private Collider targetCollider;
    private float speed;
    private int damage;

    public GameObject explosionEffect;  // Эффект взрыва

    public void Seek(Transform targetEnemy, float missileSpeed, int missileDamage)
    {
        target = targetEnemy;
        targetCollider = target.GetComponent<Collider>(); // Получаем коллайдер цели
        speed = missileSpeed;
        damage = missileDamage;
    }

    void Update()
    {
        // Если цель уничтожена, снаряд тоже уничтожается
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Целимся в центр коллайдера врага, если он есть
        Vector3 targetPoint = (targetCollider != null) ? targetCollider.bounds.center : target.position;
        Vector3 direction = targetPoint - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        // Проверяем, не долетел ли снаряд до цели
        if (direction.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        // Двигаем снаряд вперед
        transform.Translate(direction.normalized * distanceThisFrame, Space.World);
        transform.LookAt(targetPoint);
    }

    void HitTarget()
    {
        // Если цель уже уничтожена, просто удаляем снаряд без взрыва
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        FindObjectOfType<AudioManager>().Play("Explosion");

        // Создаём взрывной эффект
        GameObject effect = Instantiate(explosionEffect, transform.position, transform.rotation);
        Destroy(effect, 1f);  // Уничтожаем эффект через 1 секунду

        // Наносим урон основной цели
        Damage(target, damage);

        // Наносим урон по области
        ApplyAreaDamage();

        // Уничтожаем снаряд
        Destroy(gameObject);
    }

    void Damage(Transform enemy, float damageAmount)
    {
        if (enemy == null) return;

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        if (enemyAI != null && !enemyAI.isDead)  // Проверяем, жив ли враг
        {
            enemyAI.TakeDamage(Mathf.RoundToInt(damageAmount));
        }
    }

    void ApplyAreaDamage()
    {
        float explosionRadius = 5f; // Радиус взрыва
        float secondaryDamageMultiplier = 0.5f; // 50% урона по вторичным целям

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, explosionRadius, LayerMask.GetMask("Enemy"));

        foreach (Collider enemyCollider in hitEnemies)
        {
            if (enemyCollider.transform != target)
            {
                Damage(enemyCollider.transform, Mathf.RoundToInt(damage * secondaryDamageMultiplier));
            }
        }
    }

    // Взрыв при столкновении с врагом
    void OnTriggerEnter(Collider other)
    {
        if (other.transform == target) // Проверяем, что попали в цель
        {
            HitTarget();
        }
    }
}