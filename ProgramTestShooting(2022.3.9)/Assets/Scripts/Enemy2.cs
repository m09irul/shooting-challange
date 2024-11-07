using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2 : EnemyBase
{
    protected override void StartBehavior()
    {
        base.StartBehavior();

        var interval = Random.Range(minShootInterval, maxShootInterval);
        UpdateNextShootTime(interval);
    }

    protected override void UpdateWhenFormed()  
    {
        base.UpdateWhenFormed();

        transform.Translate(Vector3.left * moveSpeed * Time.deltaTime, Space.World);

        if (Time.time >= nextShootTime)
        {
            var shootDirection = (playerTransform.position - shootPoint.transform.position).normalized;
            Shoot(StringManager.ENEMY_BULLET_AUDIO, StringManager.ENEMY_BULLET_2_NAME, StringManager.ENEMY2_MUZZLE, shootPoint, shootDirection);
            var interval = Random.Range(minShootInterval, maxShootInterval);
            UpdateNextShootTime(interval);
        }
    }

}
