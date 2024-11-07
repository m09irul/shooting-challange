using System.Collections;
using UnityEngine;

public class Boss : EnemyBase
{
    [Header("Boss Stages")]
    [SerializeField] private float normalThreshold = 0.5f; 
    [SerializeField] private float dashThreshold = 0.25f; 
    [SerializeField] private float stageTransitionTime = 1f;

    [Header("Attack Patterns")]
    [SerializeField] private int bulletsPerSpread = 3;
    [SerializeField] private float spreadAngle = 15f;

    [Header("Dash")]
    [SerializeField] private float dashCooldown = 2f;
    private float nextDashTime;

    public enum BossStage { Normal, Dash, Minions, Rage }
    public BossStage currentStage = BossStage.Normal;

    private bool isInvulnerable;
    private bool isDashing, isMinionSpawning;
    WaveManager waveManager;
    private bool isRagePositioned = false;
    [SerializeField] private float upDownDuration = 7f;
    private bool isPreparingForRage = false;

    protected override void StartBehavior()
    {
        base.StartBehavior();

        waveManager = WaveManager.instance;

        waveManager.OnBossMinionFinished += OnMinionFinished;
    }

    protected override void UpdateWhenFormed()
    {
        base.UpdateWhenFormed();

        UpdateStage();
        UpdateBehavior();
    }

    private void UpdateStage()
    {
        float healthPercent = (float)health / maxHealth;
        BossStage newStage = currentStage;

        if (healthPercent > normalThreshold)
        {
            newStage = BossStage.Normal;
        }
        else if (healthPercent > dashThreshold)
        {
            newStage = BossStage.Dash;
        }
        else if(currentStage != BossStage.Rage)
            newStage = BossStage.Minions;

        if (newStage != currentStage)
        {
            StartCoroutine(TransitionStage(newStage));
        }
    }

    private void UpdateBehavior()
    {
        if (isDashing || isMinionSpawning) return;

        switch (currentStage)
        {
            case BossStage.Normal:
                NormalBehavior();
                break;
            case BossStage.Dash:
                DashBehavior();
                break;
            case BossStage.Minions:
                MinionBehavior();
                break;
            case BossStage.Rage:
                RageBehavior();
                break;
        }
    }

    private void NormalBehavior()
    {
        if (Time.time >= nextShootTime)
        {
            ShootSpreadPattern();

            UpdateNextShootTime(minShootInterval);
        }
    }

    private void DashBehavior()
    {
        if (Time.time >= nextDashTime && !isDashing)
        {
            StartDashPreparation();
        }
    }

    private void MinionBehavior()
    {
        isMinionSpawning = true;

        StartCoroutine(SpawnMinions());
    }

    private void RageBehavior()
    {
        if (!isRagePositioned && !isPreparingForRage)
        {
            isPreparingForRage = true;

            Vector3 pos = new Vector3(GameManager.instance.rightScreenBound - 3.5f, 0, 0);

            LeanTween.move(gameObject, pos, 4f)
                .setEase(LeanTweenType.easeOutCubic)
                .setOnComplete(() =>
                {
                    isRagePositioned = true;
                    StartUpDownMovement();
                });
        }
        else if (isRagePositioned)
        { 
            if (Time.time >= nextShootTime)
            {
                ShootSpreadPattern();

                var interval = (minShootInterval * 0.5f);
                UpdateNextShootTime(interval);
            }
        }
    }
    private void StartUpDownMovement()
    {
        float topPos = GameManager.instance.topScreenBound;
        float bottomPos = GameManager.instance.bottomScreenBound;

        LeanTween.moveY(gameObject, bottomPos, upDownDuration) 
            .setOnComplete(() =>
            {
                LeanTween.moveY(gameObject, topPos, upDownDuration)
                    .setLoopPingPong();
            });
    }
    private void ShootSpreadPattern()
    {
        if (playerTransform == null) return;

        Vector2 dirToPlayer = (playerTransform.position - shootPoint.transform.position).normalized;

        for (int i = 0; i < bulletsPerSpread; i++)
        {
            float angle = (i - (bulletsPerSpread - 1) / 2f) * spreadAngle;
            Vector2 rotatedDir = Quaternion.Euler(0, 0, angle) * dirToPlayer;

            Shoot(StringManager.BOSS_BULLET_AUDIO, StringManager.BOSS_BULLET, StringManager.BOSS_MUZZLE, shootPoint, rotatedDir);
        }
    }
    private void StartDashPreparation()
    {
        if (playerTransform == null) return;

        isDashing = true;

        var originalPos = transform.position;

        // Shake enemy
        LeanTween.moveLocal(gameObject, originalPos, 1.2f)
            .setOnUpdate((Vector3 val) =>
            {
                transform.position = originalPos + Random.insideUnitSphere * 0.1f;
            })
            .setOnComplete(() =>
            {
                StartDash();
            });

        cameraShakeManager.ShakeCamera(15, 3f);
    }
    private void StartDash()
    {
        Vector3 playerLastPosition = playerTransform.position;

        LeanTween.cancel(gameObject);

        LeanTween.move(gameObject, playerLastPosition, 0.5f)
            .setEase(LeanTweenType.easeInQuad)
            .setOnComplete(() => 
            {
                isDashing = false;
                nextDashTime = Time.time + dashCooldown;
            });
    }


    private IEnumerator SpawnMinions()
    {
        LeanTween.cancel(gameObject);

        Vector3 pos = new Vector3(GameManager.instance.rightScreenBound + 1, 0,0);

        LeanTween.move(gameObject, pos, 1).setEase(LeanTweenType.easeOutCubic);

        yield return new WaitForSeconds(1);

        var wave = WaveManager.instance.GetWaveData();

        for (int i = 0; i < wave.groups.Length - 1; i++)
        {
           waveManager.enemySpawner.SpawnGroup(wave.groups[i]);
           yield return new WaitForSeconds(wave.delayBetweenGroups);
        }
        waveManager.enemySpawner.SpawnGroup(wave.groups[wave.groups.Length - 1]);
        waveManager.isGroupInProgress = false;

    }
    void OnMinionFinished()
    {
        currentStage = BossStage.Rage;
        isMinionSpawning = false;

        StartCoroutine(TransitionStage(currentStage));
    }
    private IEnumerator TransitionStage(BossStage newStage)
    {
        isInvulnerable = true;
        isDashing = false;
        rb.velocity = Vector2.zero;

        // Visual feedback
        LeanTween.scale(gameObject, Vector2.one * 1.5f, stageTransitionTime * 0.5f)
            .setEase(LeanTweenType.easeOutQuad)
            .setLoopPingPong(1);

        yield return new WaitForSeconds(stageTransitionTime);

        currentStage = newStage;
        isInvulnerable = (currentStage == BossStage.Minions);
    }

    public override void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        base.TakeDamage(damage);
    }

    protected override void OnDeath()
    {
        LeanTween.cancel(gameObject);

        audioManager.PlayOneShot(StringManager.BOSS_DESTROY_AUDIO);
        audioManager.FadeOut(StringManager.BOSS_STAGE_AUDIO, 4.5f);
        cameraShakeManager.ShakeCamera(50, 3f);
        GameManager.instance.GameOver();
        GameObject vfx = poolManager.GetPooledObject(StringManager.BOSS_VFX);
        vfx.transform.SetParent(gameObject.transform);
        vfx.transform.localPosition = Vector3.zero;
        StartCoroutine(DeathVFXRoutine(vfx));

    }
    IEnumerator DeathVFXRoutine(GameObject vfx)
    {
        yield return new WaitForSeconds(3f);
        spriteRenderer.gameObject.SetActive(false);
        yield return new WaitForSeconds(2f);
        poolManager.ReturnToPool(StringManager.BOSS_VFX, vfx);
        poolManager.ReturnToPool(enemyName, gameObject);
        //gameover scene..

        GameManager.instance.DrawGameOverPanel(GameManager.instance.stageLoop.m_game_score);

    }
}
