using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Base Enemy Settings")]
    [SerializeField]protected float maxHealth;
    [SerializeField] protected float health;
    [SerializeField] protected float moveSpeed = 3f;

    [Header("Formation Movement")]
    protected float formationMoveTime;
    protected Vector3 targetPosition;

    [Header("Color Settings")]
    [SerializeField] private Color healthyColor = Color.white;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float colorChangeDuration = 0.2f;

    protected SpriteRenderer spriteRenderer;

    protected bool isFormed = false, isDead = false;

    protected AudioManager audioManager;
    protected CameraShakeManager cameraShakeManager;
    protected ObjectPoolManager poolManager;
    protected Transform playerTransform;

    private float lastDamageTime;


    [SerializeField] protected Transform shootPoint;
    [SerializeField] protected float minShootInterval = 1f;
    [SerializeField] protected float maxShootInterval = 3f;
    protected float nextShootTime = 0;

    protected Rigidbody2D rb;
    protected GameManager gameManager;
    [HideInInspector]public string enemyName;

    public virtual void Initialize(Vector3 formationPosition)
    {
        rb = GetComponent<Rigidbody2D>();
        playerTransform = GameObject.FindGameObjectWithTag(StringManager.PLAYER_TAG).transform;
        audioManager = AudioManager.instance;
        cameraShakeManager = CameraShakeManager.instance;
        poolManager = ObjectPoolManager.instance;
        gameManager = GameManager.instance;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        health = maxHealth;

        targetPosition = formationPosition;
        formationMoveTime = Random.Range(3f, 7f);
        MoveToFormation();

    }

    protected virtual void MoveToFormation()
    {
        Vector3 startPos = transform.position;
        float midX = (startPos.x + targetPosition.x) * 0.5f;

        Vector3[] pathPoints = new Vector3[]
        {
            startPos,
            new Vector3(midX + Random.Range(-2f, 2f), Random.Range(gameManager.bottomScreenBound, gameManager.topScreenBound), 0f),
            new Vector3(midX + Random.Range(-2f, 2f), Random.Range(gameManager.bottomScreenBound, gameManager.topScreenBound), 0f),
            targetPosition
        };

        LTBezierPath bezierPath = new LTBezierPath(pathPoints);

        LeanTween.move(gameObject, bezierPath.pts, formationMoveTime)
            .setEase(LeanTweenType.easeOutCubic)
            .setOnComplete(() =>
            {
                isFormed = true;
                StartBehavior();
            });
    }

    //enemies are same. but boss overrides..
    protected virtual void OnDeath()
    {
        audioManager.PlayOneShot(StringManager.ENEMY_DESTROY_AUDIO);
        cameraShakeManager.ShakeCamera(4, 0.2f);

        WaveManager.instance.EnemyDestroyed();

        GameObject vfx = poolManager.GetPooledObject(StringManager.ENEMY_VFX);
        vfx.transform.SetParent(gameObject.transform);
        vfx.transform.localPosition = Vector3.zero;
        StartCoroutine(DeathVFXRoutine(vfx));
    }
    IEnumerator DeathVFXRoutine(GameObject vfx)
    {
        yield return new WaitForSeconds(1f);
        poolManager.ReturnToPool(enemyName, gameObject);
        poolManager.ReturnToPool(StringManager.ENEMY_VFX, vfx);
    }
    IEnumerator HitVFXRoutine(GameObject vfx)
    {
        yield return new WaitForSeconds(1f);
        poolManager.ReturnToPool(StringManager.BULLET_HIT_VFX, vfx);
    }
    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;

        health -= damage;
        audioManager.PlayOneShot(StringManager.ENEMY_HIT_AUDIO);

        GameObject vfx = poolManager.GetPooledObject(StringManager.BULLET_HIT_VFX);
        vfx.transform.SetParent (gameObject.transform);
        vfx.transform.localPosition = Vector2.zero;
        StartCoroutine(HitVFXRoutine(vfx));

        // Update color based on health
        float healthPercent = (float)health / maxHealth;
        Color targetColor = Color.Lerp(damageColor, healthyColor, healthPercent);
        spriteRenderer.color = targetColor;

        if (health <= 0)
        {
            isDead = true;
            OnDeath();
        }       
    }

    protected void Shoot(string audioName, string bulletName, string muzzleName, Transform shootPoint, Vector2 shootDirection)
    {
        audioManager.PlayOneShot(audioName);
        
        EnemyBullet bullet = poolManager.GetPooledObject(bulletName).GetComponent<EnemyBullet>();

        GameObject muzzle = poolManager.GetPooledObject(muzzleName);

        if (bullet != null && muzzle != null)
        {
            bullet.transform.position = shootPoint.position;
            bullet.transform.rotation = Quaternion.identity;

            bullet.Init(shootDirection);
            bullet.bulletName = bulletName;

            muzzle.transform.SetParent(shootPoint);
            muzzle.transform.localPosition = Vector3.zero;

            StartCoroutine(MuzzleRoutine(muzzleName, muzzle));
        }

    }
    IEnumerator MuzzleRoutine(string name, GameObject muzzle)
    {
        yield return new WaitForSeconds(0.5f);
        poolManager.ReturnToPool(name, muzzle);
    }
    protected void UpdateNextShootTime(float interval)
    {
        nextShootTime = Time.time + interval;
    }
    protected virtual void StartBehavior()
    {
        WaveManager.instance.enemySpawner.ReleasePosition(targetPosition);
    }
    protected virtual void Update()
    {
        if (!isFormed || gameManager.isGameOver || isDead) return;

        UpdateWhenFormed();
    }

    protected virtual void UpdateWhenFormed()
    {
        if (transform.position.x < gameManager.leftScreenBound - 4f
            || transform.position.y < gameManager.bottomScreenBound - 4f
            || transform.position.y > gameManager.topScreenBound + 4f)
        {
            WaveManager.instance.EnemyDestroyed();
            poolManager.ReturnToPool(enemyName, gameObject);
        }
        
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag(StringManager.PLAYER_TAG))
        {
            if (Time.time - lastDamageTime >= 0.1f)
            {
                if (!isDead)
                    other.GetComponent<Player>().TakeDamage(10);

                lastDamageTime = Time.time;
                
            }
        }
    }

}
