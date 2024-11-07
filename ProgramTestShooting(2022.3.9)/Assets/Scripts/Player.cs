using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float normalMoveSpeed = 5f;
    public float focusedMoveSpeed = 2f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float offset = 1f;
    [SerializeField] Image dashFill;

    [Header("Shooting")]
    public PlayerBullet bulletPrefab;
    public float normalFireRate = 0.2f;
    public float focusedFireRate = 0.1f;
    public float displacementRange = 0.1f;
    public Transform shootPoint;

    [Header("Health")]
    [SerializeField] private float maxHealth = 100;
    public float currentHealth;

    [Space]
    private float currentMoveSpeed;
    private float currentFireRate;
    private bool isFocusMode;
    public Color focusModeColor;
    private bool isDashing;
    private bool canDash = true;
    private Vector2 moveDirection;
    private float nextFireTime;
    private float nextDashTime;
    private bool isInvincible;
    public float invincibilityDuration = 2f;
    Vector3 originalScale;

    AudioManager audioManager;
    CameraShakeManager cameraShakeManager;
    ObjectPoolManager poolManager;
    GameManager gameManager;
    public GameObject tail;

    [Header("Flash")]
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    public Gradient screenFlashColor;

    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color flashColor = Color.white;
    public void StartRunning()
    {
        tail.SetActive(true);

        audioManager = AudioManager.instance;
        cameraShakeManager = CameraShakeManager.instance;
        poolManager = ObjectPoolManager.instance;
        gameManager = GameManager.instance;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalScale = gameObject.transform.localScale;
        dashFill.fillAmount = 1;
        InitializePlayer();
    }

    private void InitializePlayer()
    {
        currentHealth = maxHealth;
        gameManager.playerHealthFill.fillAmount = currentHealth/maxHealth;

        currentMoveSpeed = normalMoveSpeed;
        currentFireRate = normalFireRate;
        StartCoroutine(MainCoroutine());
    }

    private IEnumerator MainCoroutine()
    {
        while (true && !gameManager.isGameOver)
        {
            HandleMovement();
            HandleShooting();
            yield return null;
        }
    }

    private void HandleMovement()
    {
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        UpdateFocusMode();
        if (Input.GetKeyDown(KeyCode.Space) && canDash && Time.time >= nextDashTime)
        {
            StartCoroutine(DashCoroutine());
        }

        if (!isDashing)
        {
            MovePlayer();
        }
    }

    private void UpdateFocusMode()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            audioManager.PlayOneShot(StringManager.FOCUS_MODE_AUDIO);
            LeanTween.scale(gameObject, originalScale *1.2f, 0.1f);
            SetFocusMode(true, focusedMoveSpeed, focusedFireRate);
        }
        else if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            LeanTween.scale(gameObject, originalScale, 0.1f);
            SetFocusMode(false, normalMoveSpeed, normalFireRate);
        }
    }

    private void SetFocusMode(bool enable, float speed, float fireRate)
    {
        if (isFocusMode != enable)
        {
            isFocusMode = enable;
            currentMoveSpeed = speed;
            currentFireRate = fireRate;
        }
    }

    private void MovePlayer()
    {
        transform.position = ClampPosition(moveDirection, currentMoveSpeed);

    }

    private void HandleShooting()
    {
        if (Input.GetKey(KeyCode.Z) && Time.time >= nextFireTime)
        {
            FireBullets();
            nextFireTime = Time.time + currentFireRate;
        }
    }

    private void FireBullets()
    {
        audioManager.PlayOneShot(StringManager.PLAYER_BULLET_AUDIO);
        cameraShakeManager.ShakeCamera(1, 0.1f);

        float displacement = isFocusMode ? 0 : Random.Range(-displacementRange, displacementRange);

        PlayerBullet bullet = poolManager.GetPooledObject(StringManager.PLAYER_BULLET_NAME).GetComponent<PlayerBullet>();
        GameObject muzzle = poolManager.GetPooledObject(StringManager.PLAYER_MUZZLE);

        if (bullet != null && muzzle != null)
        {
            bullet.transform.position = new Vector3(shootPoint.transform.position.x, shootPoint.transform.position.y + displacement, 0);
            bullet.transform.rotation = Quaternion.identity;
            bullet.transform.SetParent(transform.parent);

            muzzle.transform.SetParent(shootPoint);
            muzzle.transform.localPosition = Vector3.zero;

            StartCoroutine(MuzzleRoutine(muzzle));
        }

    }
    IEnumerator MuzzleRoutine(GameObject muzzle)
    { 
        yield return new WaitForSeconds(0.5f);
        poolManager.ReturnToPool(StringManager.PLAYER_MUZZLE, muzzle);
    }

    private IEnumerator DashCoroutine()
    {
        audioManager.PlayOneShot(StringManager.PLAYER_DASH_AUDIO);

        isDashing = true;
        canDash = false;
        nextDashTime = Time.time + dashCooldown;
        dashFill.fillAmount = 0;
        Vector3 dashDirection = moveDirection.normalized;
        float dashEndTime = Time.time + dashDuration;
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);
        StartCoroutine(InvincibilityCoroutine(dashDuration));

        while (Time.time < dashEndTime)
        {
            transform.position = ClampPosition(dashDirection, dashSpeed);

            yield return null;
        }
        spriteRenderer.color = originalColor;
        isDashing = false;

        LeanTween.value(gameObject, 0f, 1f, dashCooldown)
            .setOnUpdate((float value) =>
            {
                dashFill.fillAmount = value;
            })         
            .setOnComplete(() =>
            {
                canDash = true;
                audioManager.PlayOneShot(StringManager.PLAYER_DASH_READY_AUDIO);

            });
    }
    Vector3 ClampPosition(Vector3 direction, float speed)
    {
        Vector3 newPosition = transform.position + direction * speed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, gameManager.leftScreenBound + offset, gameManager.rightScreenBound - offset);
        newPosition.y = Mathf.Clamp(newPosition.y, gameManager.bottomScreenBound + offset, gameManager.topScreenBound - offset);

        return newPosition;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || gameManager.isGameOver) return;

        cameraShakeManager.ShakeCamera(14, 0.5f);
        PlayHitEffect();

        currentHealth -= damage;
        gameManager.playerHealthFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityCoroutine(0.7f));
        }
    }
    public void PlayHitEffect()
    {
        audioManager.PlayOneShot(StringManager.PLAYER_HIT_AUDIO);
        gameManager.FlashScreen(screenFlashColor, flashDuration);

        LeanTween.value(gameObject, 0f, 1f, flashDuration)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float value) =>
            {
                spriteRenderer.color = Color.Lerp(originalColor, flashColor, value);
            })
            .setLoopPingPong(1)
            .setOnComplete(() =>
            {
                spriteRenderer.color = originalColor;
            });
    }

    private void Die()
    {
        gameManager.GameOver();
        cameraShakeManager.ShakeCamera(30, 2f);
        audioManager.FadeOut(StringManager.INGAME_AUDIO, 1.5f);
        audioManager.FadeOut(StringManager.BOSS_STAGE_AUDIO, 1.5f);

        audioManager.PlayOneShot(StringManager.PLAYER_DESTROY_AUDIO);
        GameObject vfx = poolManager.GetPooledObject(StringManager.PLAYER_VFX);
        vfx.transform.SetParent(gameObject.transform);
        vfx.transform.localPosition = Vector3.zero;
        StartCoroutine(DeathVFXRoutine(vfx));
    }
    IEnumerator DeathVFXRoutine(GameObject vfx)
    {
        spriteRenderer.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        poolManager.ReturnToPool(StringManager.PLAYER_VFX, vfx);
        //gameover scene..
        gameManager.DrawGameOverPanel(gameManager.stageLoop.m_game_score-1);

    }

    private IEnumerator InvincibilityCoroutine(float invincibilityDuration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }
}
