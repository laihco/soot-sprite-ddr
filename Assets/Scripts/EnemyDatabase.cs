using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemyDatabase : MonoBehaviour
{
    public EnemySoot[] enemyPrefabs;

    private Dictionary<int, GameObject> enemyLookup;

    void Awake()
    {
        enemyLookup = enemyPrefabs.ToDictionary(
            enemy => enemy.enemyID,
            enemy => enemy.gameObject
        );
    }

    public GameObject GetEnemy(int enemyID)
    {
        if (enemyLookup.TryGetValue(
            enemyID,
            out GameObject prefab))
        {
            return prefab;
        }

        Debug.LogWarning(
            $"Enemy ID {enemyID} not found!"
        );

        return null;
    }
}