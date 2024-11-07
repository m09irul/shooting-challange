using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1 : EnemyBase
{
    [SerializeField] private float forwardStepDistance = 1f;
    private bool movingUp = true;
    private bool isMovingForward = false;

    protected override void StartBehavior()
    {
        base.StartBehavior();

        var interval = Random.Range(minShootInterval, maxShootInterval);
        UpdateNextShootTime(interval);
    }
    protected override void UpdateWhenFormed() 
    {
        base.UpdateWhenFormed();

        if (isMovingForward)
        {
            MoveForwardStep();
        }
        else
        {
            VerticalMovement();
        }

        if (Time.time >= nextShootTime)
        {
            Shoot(StringManager.ENEMY_BULLET_AUDIO, StringManager.ENEMY_BULLET_1_NAME, StringManager.ENEMY1_MUZZLE, shootPoint, Vector2.left);

            var interval = Random.Range(minShootInterval, maxShootInterval);
            UpdateNextShootTime(interval);
        }
    }

    private void MoveForwardStep()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            isMovingForward = false;
        }
    }

    private void VerticalMovement()
    {
        float moveDirection = movingUp ? 1f : -1f;
        transform.Translate(Vector3.up * moveDirection * moveSpeed * Time.deltaTime);

        if (movingUp && transform.position.y >= gameManager.topScreenBound)
        {
            transform.position = new Vector3(transform.position.x, gameManager.topScreenBound, 0);
            PrepareForwardMove();
            movingUp = false;
        }
        else if (!movingUp && transform.position.y <= gameManager.bottomScreenBound)
        {
            transform.position = new Vector3(transform.position.x, gameManager.bottomScreenBound, 0);
            PrepareForwardMove();
            movingUp = true;
        }
    }

    private void PrepareForwardMove()
    {
        isMovingForward = true;
        targetPosition = transform.position + Vector3.left * forwardStepDistance;
    }
}
