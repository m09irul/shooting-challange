using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveData", menuName = "Game/Wave Data")]
public class WaveData : ScriptableObject
{
    public GroupData[] groups;
    public float delayBetweenGroups = 2f;
    public bool isBossWave = false;
    public String bossPrefabName;
}

[Serializable]
public class GroupData
{
    public String enemyName;
    public int enemyCount = 5;
    public Vector2 spawnAreaMin = new Vector2(6f, -4f); // Formation area boundaries
    public Vector2 spawnAreaMax = new Vector2(12f, 4f);
    public float minEnemySpacing = 1f; // Minimum distance between enemies
}
