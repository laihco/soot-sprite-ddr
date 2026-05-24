using UnityEngine;

/*

Update Json to include:
{"beat": 8, "hole": "Left", "enemyID": 0}

*/

public class EnemySoot : MonoBehaviour
{
    [Header("Unique ID for JSON")]
    public int enemyID;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayPop()
    {
        // Restart animation from beginning
        animator.Play(0, 0, 0f);
    }
}