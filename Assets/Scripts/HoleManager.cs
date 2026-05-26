using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HoleManager : MonoBehaviour
{
    public HoleSetup[] holes;

    private Dictionary<int, HoleSetup> holeLookup;

    void Awake()
    {
        holeLookup = holes.ToDictionary(
            hole => hole.holeID,
            hole => hole
        );
    }

    public void SpawnEnemyInHole(
        int holeID,
        GameObject enemyPrefab
    )
    {
        if (holeLookup.TryGetValue(
            holeID,
            out HoleSetup hole))
        {
            hole.SpawnEnemy(enemyPrefab);
        }
    }
}