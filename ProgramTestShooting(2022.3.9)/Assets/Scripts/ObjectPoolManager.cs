using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        [Tooltip("empty name will use the prefab name by default..")]
        public string name;
        public GameObject prefab;
        public int size;
    }

    [Header("Pool Settings")]
    public Pool[] pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        instance = this;
        InitializePools();
    }

    private void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform);
                objectPool.Enqueue(obj);
            }

            if(pool.name == "")
                pool.name = pool.prefab.name;
            poolDictionary.Add(pool.name, objectPool);
        }
    }

    public GameObject GetPooledObject(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }

        GameObject obj = poolDictionary[tag].Dequeue();

        obj.SetActive(true);

        poolDictionary[tag].Enqueue(obj);
        return obj;
    }

    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.SetActive(false);
    }
}