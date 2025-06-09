using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;


public class Player : MonoBehaviour
{
    private Rigidbody rb;
    public float speed;
    private Vector3 moveVector;
    public bool canMove = true;
    private Animator anim;
    public Text txt;
    public int HP = 100;
    public int damage = 5;
    public int atk_radius = 3;
    public LayerMask enemyLayer;

    private CharacterController CharCtrl;

    void Awake()
    {
        

    }

    void Start()
    {
        CharCtrl = this.GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        if (canMove == true)
        {
            moveVector.x = SimpleInput.GetAxis("Horizontal");
            moveVector.z = SimpleInput.GetAxis("Vertical");

            //rb.MovePosition(transform.position + moveVector * 1f * Time.deltaTime);
            //Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * Time.fixedDeltaTime);
            //rb.MoveRotation(rb.rotation * deltaRotation);
            Vector3 NextDir = new Vector3(SimpleInput.GetAxisRaw("Horizontal"), 0, SimpleInput.GetAxisRaw("Vertical"));



            //txt.text = (rb.position + moveVector * 0.1f * Time.deltaTime).ToString();

            CharCtrl.Move(NextDir * Time.deltaTime * speed);
            if (anim != null)
                if (moveVector.x + moveVector.z != 0)
                {
                    anim.SetInteger("A", 1);
                }
                else anim.SetInteger("A", 0);

            Collider[] cols = Physics.OverlapSphere(transform.position, 10, enemyLayer);

            if (cols.Length > 0)
            {
                // Найдем ближайшего врага
                Transform nearestEnemy = GetNearestEnemy(cols);

                // Поворачиваемся к ближайшему врагу
                if (nearestEnemy != null)
                {
                    RotateTowards(nearestEnemy);
                }
                //transform.rotation = Quaternion.LookRotation(cols[0].transform.position - transform.position);
                //txt.text = Quaternion.LookRotation(cols[0].transform.position).ToString();
            }
            else if (NextDir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(NextDir);
        }
    }

    public void player_attack()
    {
        canMove = false;
        anim.SetInteger("A", 2);
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
    }

    public void AttackAnimation()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 10, enemyLayer);
        if (cols.Length > 0)
        {
            EnemyAI Enemy = cols[0].transform.GetComponent<EnemyAI>();
            if (Enemy != null) Enemy.TakeDamage(damage);
        }
        canMove = true;
    }

    Transform GetNearestEnemy(Collider[] enemies)
    {
        Transform nearestEnemy = null;
        float shortestDistance = Mathf.Infinity;

        foreach (Collider enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        return nearestEnemy;
    }

    // Метод для поворота персонажа в сторону врага
    void RotateTowards(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Плавный поворот к цели
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }
}
