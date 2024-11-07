using System.Collections;
using UnityEngine;

/// <summary>
/// Handles player bullet movement and collision
/// </summary>
public class PlayerBullet : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float lifetime = 2f;
    [SerializeField] private int damage = 5;

    public Rigidbody2D rb;
    ObjectPoolManager poolManager;
    Coroutine bulletRoutine;

    private void Awake()
    {
        poolManager = ObjectPoolManager.instance;
        rb = GetComponent<Rigidbody2D>();        
    }
    private void OnEnable()
    {
        rb.velocity = Vector2.right * moveSpeed;

        Invoke(nameof(DestroyObject), lifetime);
    }
    private void OnDisable()
    {
        CancelInvoke(nameof(DestroyObject));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(StringManager.ENEMY_TAG) && !GameManager.instance.isGameOver)
        {
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            enemy?.TakeDamage(damage);

            DestroyObject();
        }
    }

    private void DestroyObject()
    {
        poolManager.ReturnToPool(StringManager.PLAYER_BULLET_NAME, gameObject);
    }
}
