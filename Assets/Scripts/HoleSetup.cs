using UnityEngine;

public class HoleSetup : MonoBehaviour
{
    public int holeID;

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        // Instantiates the specific enemy type asked for by the beatmap.
        // Adding 'transform' at the end makes the spawned enemy a child of this hole in the hierarchy.
        GameObject enemy = Instantiate(
            enemyPrefab,
            transform.position,
            Quaternion.identity,
            transform 
        );

        EnemySoot enemySoot = enemy.GetComponent<EnemySoot>();

        if (enemySoot != null)
        {
            enemySoot.PlayPop();
        }
    }
}