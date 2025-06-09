using System;
using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class ParticleAttraction : MonoBehaviour
{
    private Transform playerTransform;
    private bool isAttracted = false;
    public float attractionSpeed = 5f; // Скорость притяжения
    public float attractionRadius = 1f; // Радиус для "всасывания" частиц
    public float xpAmount = 10f; // Количество опыта, которое дается за частицу

    public float launchForce = 5f; // Сила выброса
    public float randomAngleRange = 30f; // Угол разброса для направления выброса
    private Vector3 launchDirection; // Направление выброса
    private Rigidbody rb; // Rigidbody для управления движением частиц

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Генерируем случайное направление для выброса частиц
        float randomAngleX = UnityEngine.Random.Range(-randomAngleRange, randomAngleRange);
        float randomAngleZ = UnityEngine.Random.Range(-randomAngleRange, randomAngleRange);

        // Направление выброса под случайным углом вверх
        launchDirection = Quaternion.Euler(randomAngleX, 0, randomAngleZ) * Vector3.up;

        // Применяем силу к частице
        rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);
    }

    void Update()
    {
        // Если частицы притягиваются к игроку
        if (isAttracted && playerTransform != null)
        {
            // Отключаем физику (гравитацию) и двигаем частицы в сторону игрока
            rb.isKinematic = true;
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, attractionSpeed * Time.deltaTime);

            // Проверяем расстояние до игрока
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= attractionRadius)
            {
                // Частица достигает игрока и исчезает, добавляя опыт
                MovementInput playerXP = playerTransform.GetComponent<MovementInput>();
                if (playerXP != null)
                {
                    playerXP.GainXP(xpAmount);
                }

                Destroy(gameObject); // Уничтожаем частицы после достижения игрока
            }
        }
    }

    public void SetTarget(Transform player)
    {
        playerTransform = player;
        isAttracted = true; // Частицы теперь притягиваются к игроку
    }
}