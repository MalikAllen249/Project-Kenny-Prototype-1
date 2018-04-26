using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    private Rigidbody m_rb;
    private Animator m_animator;
    
    //Ground Stuff
    public Transform m_groundCheck;
    public bool m_grounded = false;
    public LayerMask m_groundLayer;


    public bool m_facingRight = true;
    float m_currentSpeed;
    public float m_speed = 1.0f;

    //Sprint Stuff
    bool m_firstButtonPressed1 = false;
    bool m_firstButtonPressed2 = false;
    bool m_doubleClick1 = false; //Verifying the double click to allow animation to stop :)
    bool m_doubleClick2 = false;
    bool m_reset = false;
    float m_firstClickTime;
    float m_doubleClickSpeed = 0.3f;

    //Attack Stuff
    public Collider[] m_attackSpheres;
    float m_damage = 10.0f;
    public float m_attackRange = 0.5f;

    private void Awake()
    {
        m_rb = GetComponent<Rigidbody>();
        m_animator = GetComponent<Animator>();
    }

    private void Start()
    {
        m_currentSpeed = m_speed;
    }

    private void FixedUpdate()
    {
        GroundCheck();
        
        float hSpeed = Input.GetAxisRaw("Horizontal");
        float vSpeed = Input.GetAxisRaw("Vertical");

        if (m_grounded)
        {
            Walk(hSpeed, vSpeed);
            Sprint();
            m_rb.velocity = new Vector3(hSpeed * m_currentSpeed, m_rb.velocity.y, vSpeed * m_currentSpeed);
            //m_rb.velocity = m_rb.velocity.normalized * m_currentSpeed;
        }
        //Changing the Direction of the Sprite
        if (hSpeed > 0.0f && !m_facingRight)
            Flip();
        else if (hSpeed < 0.0f && m_facingRight)
            Flip();
        
        //Playing the punching and kick animations
        //The collider array will hold the respective attack's collider
        //The collider called into the Attack function will be for the animation played
        //Two ways of explaining the same thing ^^
        if (m_grounded)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                m_animator.SetBool("Punch", true);
                Attack(m_attackSpheres[0]);
            }
            else
                m_animator.SetBool("Punch", false);


            if (Input.GetKeyDown(KeyCode.O))
            {
                m_animator.SetBool("Kick", true);
                Attack(m_attackSpheres[1]);
            }
            else
                m_animator.SetBool("Kick", false);
        }
        
    }

    void GroundCheck()
    {
        //Is the player grounded?
        m_grounded = Physics.CheckBox(m_groundCheck.position, new Vector3(0.5f, 0.1f, 0.5f), Quaternion.identity, m_groundLayer);
        m_animator.SetBool("Ground", m_grounded);
        if (!m_grounded) Debug.Log("Not Grounded");
    }

    void Flip()
        {
            m_facingRight = !m_facingRight;
            Vector3 newScale = transform.localScale;
            newScale.x *= -1;
            transform.localScale = newScale;
        }

    void Walk(float h, float v)
    {
        if (h != 0 || v != 0)
            m_animator.SetBool("Walk", true);
        else
            m_animator.SetBool("Walk", false);
    }

    void Sprint()
    {
        
        //Sprint using 'D'
        if ((Input.GetKeyUp(KeyCode.D) && !m_firstButtonPressed1))
        {
            m_firstButtonPressed1 = true;
            m_firstClickTime = Time.time;
            //Debug.Log("First Click =" + m_firstClickTime);
        }

        if (Input.GetKey(KeyCode.D) && m_firstButtonPressed1 && ((Time.time - m_firstClickTime) < m_doubleClickSpeed))
        {
            //Debug.Log("Difference = " + (Time.time - m_firstClickTime));
            m_firstClickTime += Time.time;
            //Debug.Log("Click Update =" + m_firstClickTime);
            m_animator.SetBool("Sprint", true);
            m_animator.SetBool("Walk", false);
            m_doubleClick1 = true;
            m_currentSpeed = 2.0f;
            
        }
        else if (m_doubleClick1)
        {
            //Debug.Log("I have reset");
            m_currentSpeed = 1.0f;
            m_animator.SetBool("Sprint", false);
            m_reset = true;
            if (m_reset)
            {
                m_firstButtonPressed1 = false;
                m_reset = false;
                m_doubleClick1 = false;
            }
        }
        else if (Input.anyKey)
        {
            m_firstButtonPressed1 = false;
        }

        //Sprint using 'A'
        if ((Input.GetKeyUp(KeyCode.A) && !m_firstButtonPressed2))
        {
            m_firstButtonPressed2 = true;
            m_firstClickTime = Time.time;
        }
        if ((Input.GetKey(KeyCode.A) && m_firstButtonPressed2) && ((Time.time - m_firstClickTime) < m_doubleClickSpeed))
        {
            //Debug.Log("Double Clicked!");
            m_firstClickTime += Time.time;
            m_animator.SetBool("Sprint", true);
            m_animator.SetBool("Walk", false);
            m_doubleClick2 = true;
            m_currentSpeed = 2.0f;
        }
        else if (m_doubleClick2)
        {
            //Debug.Log("I have resset");
            m_currentSpeed = 1.0f;
            m_animator.SetBool("Sprint", false);
            m_reset = true;
            if (m_reset)
            {
                m_firstButtonPressed2 = false;
                m_reset = false;
                m_doubleClick2 = false;
            }
        }
        else if (Input.anyKey)
        {
            m_firstButtonPressed2 = false;
        }
        
    }

    void Attack(Collider col)
    {
        //Nice implementation, if I do say so myself!
        Collider[] cols = Physics.OverlapSphere(col.transform.position, m_attackRange, LayerMask.GetMask("EnemyHitBox"));
        foreach (Collider c in cols)
        {
            if (c.transform.parent == transform)
                continue;

            if (!c.gameObject.GetComponent<HealthBar>())
                continue;

            m_damage = 10.0f;
            c.gameObject.GetComponent<HealthBar>().TakeDamage(m_damage);
            if(c.gameObject.tag == "Enemy")
                c.gameObject.GetComponent<EnemyAI>().IsHit();
            //c.SendMessageUpwards("TakeDamage", m_damage);
        }
        
    }

    public void IsHit()
    {
        m_animator.SetTrigger("IsHit");
        Debug.Log(this.gameObject.name + " has been hit!");   
    }

    public void Dead() 
    {
        if(m_grounded)
            m_animator.SetBool("Death", true);
        else
            m_animator.SetBool("Death", false);
    }
    
}
