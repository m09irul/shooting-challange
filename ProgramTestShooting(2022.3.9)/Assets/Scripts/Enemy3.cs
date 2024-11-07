using UnityEngine;

public class Enemy3 : EnemyBase
{
    [Header("Dash Settings")]
    [SerializeField] private float targetingTime = 1f;

    private bool isTargeting;
    private bool isDashing;
    private float dashTimer;
    private Vector3 originalPos;
    private Vector3 targetPos;

    protected override void StartBehavior()
    {
        base.StartBehavior();
    }

    protected override void UpdateWhenFormed()
    {
        base.UpdateWhenFormed();

        if (!isDashing && !isTargeting)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0)
            {
                StartDashPreparation();
            }
        }
        else if (isDashing)
        {
            DashMovement();
        }
    }

    private void StartDashPreparation()
    {   
        isTargeting = true;
        originalPos = transform.position;

        // Shake enemy
        LeanTween.moveLocal(gameObject, originalPos, 1.2f)
            .setOnUpdate((Vector3 val) =>
            {
                transform.position = originalPos + Random.insideUnitSphere * 0.1f;
            })
            .setOnComplete(() =>
            {
                isTargeting = false;
                isDashing = true;
            });

        cameraShakeManager.ShakeCamera(15, 3f);
    }
    private void DashMovement()
    {
        if (playerTransform == null) return;
        targetPos = playerTransform.position;

        transform.Translate((targetPos - originalPos).normalized * moveSpeed * Time.deltaTime);

    }

    private void OnDisable()
    {
        // Clean up tweens when disabled
        LeanTween.cancel(gameObject);
    }
}
