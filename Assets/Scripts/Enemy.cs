using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Rigidbody2D player;
    private Rigidbody2D rb;

    float health, maxHealth = 3f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();   
        health = maxHealth; 
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 direction = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) // Destroy enemy when health reaches zero. Probably makes sense to replace with a dead enemy object or something for limb extraction
            Destroy(gameObject);  
    }
}
