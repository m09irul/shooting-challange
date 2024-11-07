using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;
    ObjectPoolManager poolManager;
    [HideInInspector]public string bulletName;

    private void Awake()
    {
        poolManager = ObjectPoolManager.instance;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        Invoke(nameof(DestroyBullet), lifetime);
    }

    public void Init(Vector2 direction)
    {
        rb.velocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(StringManager.PLAYER_TAG))
        {
            if (other.TryGetComponent(out Player player))
            {
                player.TakeDamage(damage);
            }
            DestroyBullet();
        }
    }

    private void DestroyBullet()
    {
        poolManager.ReturnToPool(bulletName, gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(DestroyBullet));
    }
}
