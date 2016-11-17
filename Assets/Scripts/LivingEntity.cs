using UnityEngine;
using System.Collections;

public class LivingEntity : MonoBehaviour, IDamageable {

    public float startHealth;
    [SerializeField]
    protected float health;
    protected bool dead;

    public event System.Action OnDeath;

    protected virtual void Start()
    {
        health = startHealth;
    }

	public virtual void TakeHit(float dmg, RaycastHit hit)
    {
        //Do some stuff with hit here (particles ect)
        TakeDamage(dmg);
    }

    public void TakeDamage(float dmg)
    {
        health -= dmg;

        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        dead = true;
        if(OnDeath != null)
        {
            OnDeath();
        }
        GameObject.Destroy(gameObject);
    }
}
