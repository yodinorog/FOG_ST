using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public int towerLevel = 1;

    public Transform partToRotate;  // Часть турели, которая будет вращаться
    public float rotationSpeed = 3f;  // Скорость вращения турели
    public float detectionRadius = 15f;  // Радиус обнаружения врагов
    public LayerMask enemyLayer;  // Слой врагов для обнаружения
    public GameObject missilePrefab;  // Префаб ракеты
    public Transform firePoint;  // Точка, откуда будет выстреливаться ракета
    private float fireRate = 0.5f;  // Скорострельность (раз в сколько секунд стрелять)
    private float missileSpeed = 15f;  // Скорость полета ракеты
    private int missileDamage = 2;  // Урон ракеты

    public ParticleSystem fireEffect;  // Эффект при выстреле

    private Transform targetEnemy;  // Ссылка на текущую цель
    private float fireCooldown = 0f;  // Таймер перезарядки стрельбы

    void Update()
    {
        fireRate = 0.3f + 0.1f * towerLevel;
        missileDamage = 1 + towerLevel;

        if (targetEnemy == null || targetEnemy.GetComponent<EnemyAI>()?.isDead == true)
        {
            targetEnemy = null;
        }

        FindClosestEnemy();

        if (targetEnemy != null)
        {
            RotateTurret();

            if (fireCooldown <= 0f)
            {
                FireMissile();
                fireCooldown = 1f / fireRate;
            }

            fireCooldown -= Time.deltaTime;
        }
    }

    // Поиск ближайшего врага в радиусе
    void FindClosestEnemy()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        float shortestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        foreach (Collider enemyCollider in enemiesInRange)
        {
            // Получаем скрипт EnemyAI для проверки, жив ли враг
            EnemyAI enemyAI = enemyCollider.GetComponent<EnemyAI>();

            // Проверяем, жив ли враг
            if (enemyAI != null && !enemyAI.isDead)
            {
                float distanceToEnemy = Vector3.Distance(transform.position, enemyCollider.transform.position);
                if (distanceToEnemy < shortestDistance)
                {
                    shortestDistance = distanceToEnemy;
                    closestEnemy = enemyCollider.transform;
                }
            }
        }

        if (closestEnemy != null)
        {
            targetEnemy = closestEnemy;
        }
        else
        {
            targetEnemy = null;
        }
    }

    // Поворот турели в сторону врага
    void RotateTurret()
    {
        Vector3 direction = targetEnemy.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * rotationSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(0f, rotation.y, 0f);  // Поворот только по оси Y
    }

    // Выстрел ракеты
    void FireMissile()
    {
        if (missilePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Missile prefab or fire point is missing!");
            return;
        }

        // Запускаем эффект выстрела
        if (fireEffect != null)
        {
            FindObjectOfType<AudioManager>().Play("Shot");
            fireEffect.Play();  // Активируем эффект
        }

        GameObject missileGO = Instantiate(missilePrefab, firePoint.position, firePoint.rotation);
        Missile missile = missileGO.GetComponent<Missile>();

        if (missile != null)
        {
            missile.Seek(targetEnemy, missileSpeed, missileDamage);
        }
        else
        {
            Debug.LogWarning("Missile script missing on missile prefab!");
        }
    }
}