using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum States
{
    Idle,
    Wander,
    Chase,
    Attack,
    Death
}
public class EnemyAI : MonoBehaviour {

    private Rigidbody m_rb;
    private Animator m_animator;

    //States Stuff
    States m_currentState = States.Idle;
    public Transform m_target;
    bool isDead = false;

    //Wander Stuff
    public Vector3 m_startPos;
    public Vector3 m_endPos;
    public float m_wanderOffset;
    bool m_moveRight = true;
    bool m_moveLeft = false;


    //Range Stuff
    float m_chaseRange = 5.0f;

    //Ground Stuff
    public Transform m_groundCheck;
    public bool m_grounded = false;
    public LayerMask m_groundLayer;

    //Movement Stuff
    public bool m_facingRight = true;
    float m_currentSpeed;
    public float m_chaseSpeed = 0.75f;
    public float m_wanderSpeed = 0.3f;

    //Attack Stuff
    public Collider[] m_attackSpheres;
    float m_damage = 10.0f;
    public float m_attackRange = 0.5f;
    public float m_attackCoolDown = 3.0f;
    private float m_nextAttack;

    //PickUp Stuff
    public GameObject coinPickUp;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_animator = GetComponent<Animator>();
    }

    private void Start()
    {
        m_currentSpeed = m_wanderSpeed;
        m_startPos = new Vector3(transform.position.x - m_wanderOffset, transform.position.y, transform.position.z);
        m_endPos = new Vector3(transform.position.x + m_wanderOffset, transform.position.y, transform.position.z);

    }

    private void Update()
    {
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        if(isDead == false)
            UpdateStates();


        switch (m_currentState) {
            case States.Idle:
                if (m_grounded)
                    Idle();
                break;
            case States.Wander:
                if (m_grounded)
                    Wander();
                break;
            case States.Chase:
                if (m_grounded)
                    Chase();
                break;
            case States.Attack:
                if (m_grounded)
                    Attack(m_attackSpheres[0]);
                break;
            case States.Death:
                Death();
                break;
            default:
                break;
        }

        if (m_rb.velocity.x > 0 && !m_facingRight)
            Flip();
        else if (m_rb.velocity.x < 0 && m_facingRight)
            Flip();

        if (m_currentState == States.Idle)
            Debug.Log("I am Idle!");
        else if (m_currentState == States.Wander)
            Debug.Log("Wandering");
        else if (m_currentState == States.Chase)
            Debug.Log("Chasing!");
        else if (m_currentState == States.Death)
            Debug.Log("Enemy Dead");
    }

    void GroundCheck()
    {
        //Is the player grounded?
        m_grounded = Physics.CheckBox(m_groundCheck.position, new Vector3(0.5f, 0.1f, 0.5f), Quaternion.identity, m_groundLayer);
        m_animator.SetBool("Ground", m_grounded);
        if (!m_grounded) Debug.Log("Not Grounded");
    }
    void UpdateStates()
    {
        float distance = Vector3.Distance(m_target.position, transform.position);
        if (distance < m_chaseRange && distance > 0.5f )
            m_currentState = States.Chase;
        else if (distance < 0.5f)
            m_currentState = States.Idle;
        else
            m_currentState = States.Wander;

        if (distance < 1.1f && Time.time > m_nextAttack)
        {
            m_currentState = States.Attack;
            m_nextAttack = Time.time + m_attackCoolDown;
        }

        if (this.gameObject.GetComponent<Rigidbody>().isKinematic)
            m_currentState = States.Death;
            
    }

    void UpdateAnimations()
    {
        if (m_currentState == States.Chase || m_currentState == States.Wander)
        {
            m_animator.SetBool("Idle", false);
            m_animator.SetBool("Walk", true);
        } else if (m_currentState == States.Idle)
        {
            m_animator.SetBool("Idle", true);
            m_animator.SetBool("Walk", false);
        }

        if (m_currentState == States.Attack)
            m_animator.SetBool("Attack", true);
        else
            m_animator.SetBool("Attack", false);

        if (m_currentState == States.Death || isDead)
            m_animator.SetBool("Death", true);
        else
            m_animator.SetBool("Death", false);
    }
    void Flip()
    {
        m_facingRight = !m_facingRight;
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }

    void Idle()
    {
        m_rb.velocity = new Vector3(0.0f, 0.0f, 0.0f);
        Debug.Log(this.gameObject.name + " is Idle");
    }

    void Wander()
    {
        if(transform.position.x > m_endPos.x && m_moveRight)
        {
            m_currentSpeed = -1;
            m_moveRight = false;
            m_moveLeft = true;
        }else if(transform.position.x < m_startPos.x && m_moveLeft)
        {
            m_currentSpeed = 1;
            m_moveRight = true;
            m_moveLeft = false;
        }
        m_rb.velocity = new Vector3(m_currentSpeed, m_rb.velocity.y, m_currentSpeed);

    }

    void Chase()
    {
        m_currentSpeed = m_chaseSpeed;
        Vector3 direction = m_target.position - transform.position;
        Vector3 hDirection = new Vector3(direction.x, 0.0f, direction.z);
        Vector3 velocity = hDirection * m_currentSpeed;
        m_rb.velocity = velocity;

    }

    void Attack(Collider col)
    {
        Collider[] cols = Physics.OverlapSphere(col.transform.position, m_attackRange, LayerMask.GetMask("Player"));
        foreach (Collider c in cols)
        {
            if (c.transform.parent == transform)
                continue;

            if (!c.gameObject.GetComponent<HealthBar>())
                continue;

            m_damage = 10.0f;
            c.gameObject.GetComponent<HealthBar>().TakeDamage(m_damage);
            c.gameObject.GetComponent<PlayerMovement>().IsHit();
            //c.SendMessageUpwards("TakeDamage", m_damage);
        }
    }

    public void IsHit()
    {
        m_animator.SetTrigger("IsHit");
        Debug.Log(this.gameObject.name + " is hit!");
    }

    public void Death()
    {
        Instantiate(coinPickUp, m_rb.transform.position, m_rb.rotation);
        this.gameObject.GetComponent<BoxCollider>().enabled = false;
        isDead = true;
    }

}
