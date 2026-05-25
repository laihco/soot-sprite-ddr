using UnityEngine;

public class EnemySoot : MonoBehaviour
{
    [Header("Settings")]
    public int enemyID;
    
    [Tooltip("Type the EXACT name of your animation state from the Animator window here")]
    public string popAnimationName = "SootPop"; 
    
    [Tooltip("How long the enemy stays on screen before disappearing")]
    public float timeOnScreen = 1.5f;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        // This prevents enemies from infinitely piling up in the same hole.
        // It automatically destroys this game object after 'timeOnScreen' seconds.
        Destroy(gameObject, timeOnScreen);
    }

    public void PlayPop()
    {
        if (animator != null)
        {
            // Plays the animation using the string name you set in the inspector
            animator.Play(popAnimationName, 0, 0f);
        }
    }
}