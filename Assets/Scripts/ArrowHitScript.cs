using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowHitScript : MonoBehaviour
{
    public int damage = 10;
    private HashSet<Transform> damagedTargets = new HashSet<Transform>();

    public float speed = 10f;
    public float lifeTime = 3f;
    private Vector3 direction;
    private float timer = 0f;

    private LineRenderer lr;
    private float fadeTime = 1f;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        timer = 0f;
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        timer += Time.deltaTime;
        
        if (timer >= fadeTime)
        {
            Destroy(lr);
            Destroy(this);
        }
        else
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, transform.position - transform.forward * 0.5f);
        }

        if (timer >= lifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Проверка, был ли этот объект уже повреждён
        if (damagedTargets.Contains(other.transform))
            return;

        // Если это игрок
        MovementInput player = other.GetComponent<MovementInput>();
        if (player != null)
        {
            player.TakeDamage(damage);
            damagedTargets.Add(other.transform);
            Debug.Log("Стрела попала в игрока");
            return;
        }

        // Если это замок
        CastleScript castle = other.GetComponent<CastleScript>();
        if (castle != null)
        {
            castle.TakeDamage(damage);
            damagedTargets.Add(other.transform);
            Debug.Log("Стрела попала в замок");
            return;
        }

        // Можно добавить: если это объект с тегом "Wall", отскакивать или залипать
    }
}