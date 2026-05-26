using UnityEngine;

public class HoleSetup : MonoBehaviour
{
    public int holeID;

    [Header("Enemy Spawn Adjustments")]
    [Tooltip("Move the enemy up/down relative to the hole center. Increase Y to move it up.")]
    public Vector3 positionOffset = new Vector3(0f, 0.5f, 0f); 

    [Tooltip("Adjust the size of the enemy when it spawns.")]
    public Vector3 spawnScale = new Vector3(1f, 1f, 1f);

    [Tooltip("Forces the enemy to render in front of the hole.")]
    public int sortOrderBonus = 10;

    public void SpawnEnemy(GameObject enemyPrefab)
    {
        // 1. Instantiate the enemy and automatically make it a child of the hole
        GameObject enemy = Instantiate(enemyPrefab, transform);

        // 2. Apply the local position offset (moves it relative to the hole, e.g., slightly higher)
        enemy.transform.localPosition = positionOffset;

        // 3. Apply your desired scale
        enemy.transform.localScale = spawnScale;

        // 4. Grab the SpriteRenderer and boost its Sorting Order so it renders IN FRONT of the hole
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder += sortOrderBonus;
        }

        // 5. Trigger the animation
        EnemySoot enemySoot = enemy.GetComponent<EnemySoot>();
        if (enemySoot != null)
        {
            enemySoot.PlayPop();
        }
    }
}