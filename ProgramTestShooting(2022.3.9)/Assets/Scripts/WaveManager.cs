using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager instance { get; private set; }

    [Header("Wave Settings")]
    [SerializeField] private WaveData[] waves;
    [SerializeField] private float delayBetweenWaves = 2f;
    [SerializeField] private float initialDelay = 1f;

    public int currentWaveIndex = -1;
    public int activeEnemies = 0;
    public bool isWaveInProgress;
    public bool isGroupInProgress;

    public event Action<GroupData> OnGroupSpawnRequested;
    public event Action<string> OnBossSpawnRequested;
    public event Action OnBossMinionFinished;

    public EnemySpawner enemySpawner;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void StartGame() => StartCoroutine(StartWaveSequence());

    private IEnumerator StartWaveSequence()
    {
        yield return new WaitForSeconds(initialDelay);
        StartNextWave();
    }

    public void StartNextWave()
    {
        if (isWaveInProgress || ++currentWaveIndex >= waves.Length)
        {
            if (currentWaveIndex >= waves.Length) Debug.Log("All waves completed!");
            return;
        }

        StartCoroutine(ProcessWave(waves[currentWaveIndex]));
    }

    private IEnumerator ProcessWave(WaveData wave)
    {
        StageLoop.Instance.AddScore(1);

        isWaveInProgress = isGroupInProgress  = true;

        Debug.Log($"Processing wave: {currentWaveIndex}");

        if (wave.isBossWave)
        {
            OnBossSpawnRequested?.Invoke(wave.bossPrefabName);
            yield break;
        }

        for (int i = 0; i < wave.groups.Length - 1 ; i++)
        {
            OnGroupSpawnRequested?.Invoke(wave.groups[i]);
            yield return new WaitForSeconds(wave.delayBetweenGroups);
        }
        OnGroupSpawnRequested?.Invoke(wave.groups[wave.groups.Length - 1]);
        isGroupInProgress = false;
    }
    public WaveData GetWaveData()
    {
        return waves[currentWaveIndex];
    }
    public void RegisterEnemy() => activeEnemies++;

    public void EnemyDestroyed()
    {
        if (--activeEnemies <= 0 && !isGroupInProgress) StartCoroutine(PrepareNextWave());
    }

    private IEnumerator PrepareNextWave()
    {
        if (waves[currentWaveIndex].isBossWave)
            OnBossMinionFinished?.Invoke();

        isWaveInProgress = false;
        yield return new WaitForSeconds(delayBetweenWaves);
        StartNextWave();
    }
}
