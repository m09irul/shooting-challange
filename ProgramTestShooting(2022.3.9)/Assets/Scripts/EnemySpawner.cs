using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnOffsetX = 15f;
    [SerializeField] private float minSpacingBetweenEnemies = 1f;
    [SerializeField] private float formationWidth = 10f;
    [SerializeField] private float formationHeight = 6f;

    private List<Vector3> occupiedPositions = new List<Vector3>();
    private WaveManager waveManager;
    private Camera mainCam;
    private float formationRightEdge;

    ObjectPoolManager poolManager;
    GameManager gameManager;
    AudioManager audioManager;
    public Gradient bossAppearanceScreenFlashColor;

    private void Start()
    {
        poolManager = ObjectPoolManager.instance;
        audioManager = AudioManager.instance;

        waveManager = WaveManager.instance;
        mainCam = Camera.main;
        gameManager = GameManager.instance;

        formationRightEdge = gameManager.rightScreenBound - 1f;

        waveManager.OnGroupSpawnRequested += SpawnGroup;
        waveManager.OnBossSpawnRequested += SpawnBoss;
    }

    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnGroupSpawnRequested -= SpawnGroup;
            waveManager.OnBossSpawnRequested -= SpawnBoss;
        }
    }

    private Vector3 FindFormationPosition()
    {
        int attempts = 0;
        const int maxAttempts = 30;
        Vector3 position;

        do
        {
            position = new Vector3(
                Random.Range(formationRightEdge - formationWidth, formationRightEdge),
                Random.Range(-formationHeight / 2, formationHeight / 2),
                0
            );
        } while (!IsPositionValid(position) && ++attempts < maxAttempts);

        if (attempts < maxAttempts) occupiedPositions.Add(position);
        return position;
    }

    public void SpawnGroup(GroupData group)
    {
        for (int i = 0; i < group.enemyCount; i++)
        {
            SpawnEnemy(group.enemyName);
        }   
    }

    private bool IsPositionValid(Vector3 position)
    {
        return occupiedPositions.TrueForAll(occupiedPos => Vector3.Distance(position, occupiedPos) >= minSpacingBetweenEnemies);
    }

    private void SpawnEnemy(string prefabName, bool isBoss = false)
    {
        Vector3 formationPos = FindFormationPosition();
        Vector3 spawnPos = new Vector3(
            gameManager.rightScreenBound + spawnOffsetX,
            Random.Range(-formationHeight / 2, formationHeight / 2),
            0
        );

        GameObject enemy = poolManager.GetPooledObject(prefabName); 
        enemy.transform.position = spawnPos;
        enemy.transform.rotation = Quaternion.identity;

        if (enemy.TryGetComponent(out EnemyBase enemyComponent))
        {
            enemyComponent.Initialize(formationPos);
            enemyComponent.enemyName = prefabName;

            if(!isBoss) waveManager.RegisterEnemy();
        }
    }

    private void SpawnBoss(string bossPrefabName)
    {
        gameManager.FlashScreen(bossAppearanceScreenFlashColor, 0.3f);
        CameraShakeManager.instance.ShakeCamera(10, 0.5f);

        audioManager.FadeOut(StringManager.INGAME_AUDIO, 1f);
        audioManager.Play(StringManager.BOSS_STAGE_AUDIO);

        SpawnEnemy(bossPrefabName, true);   
    }

    public void ReleasePosition(Vector3 position) => occupiedPositions.Remove(position);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 formationCenter = new Vector3(
            formationRightEdge - formationWidth / 2,
            0,
            0
        );
        Gizmos.DrawWireCube(formationCenter, new Vector3(formationWidth, formationHeight, 0));
    }
}
