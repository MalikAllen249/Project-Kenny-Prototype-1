using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    //Health Stuff
    public Image m_healthBar;
    public float m_maxHealth = 100.0f;
    private float m_currentHealth;

    //Death Stuff
    bool m_isDead;

    private void Start()
    {
        m_currentHealth = m_maxHealth;

    }
    public bool TakeDamage(float damage)
    {
        m_isDead = false;
        if (m_currentHealth <= 0.0f)
        {
            Debug.Log(this.gameObject.name + " is Dead!");
            this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
            if (this.gameObject.tag == "Enemy")
                this.gameObject.GetComponent<EnemyAI>().Death();
            if (this.gameObject.tag == "Player")
                this.gameObject.GetComponent<PlayerMovement>().Dead();
            //When ever a body is turned kinematic I will mae it play the death animation!!
            return !m_isDead;
        }

        m_currentHealth -= damage;
        m_healthBar.fillAmount = m_currentHealth / m_maxHealth;

        if (m_currentHealth <= 0.0f)
        {
            Debug.Log(this.gameObject.name + " is Dead!");
            this.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            this.gameObject.GetComponent<BoxCollider>().enabled = false;
            if (this.gameObject.tag == "Enemy")
                this.gameObject.GetComponent<EnemyAI>().Death();
            if (this.gameObject.tag == "Player")
                this.gameObject.GetComponent<PlayerMovement>().Dead();

            return !m_isDead;
        }
        return m_isDead;
    }

}

