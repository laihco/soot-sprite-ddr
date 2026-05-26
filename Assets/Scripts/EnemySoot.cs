using UnityEngine;
using System.Collections; // Required for Coroutines

public class EnemySoot : MonoBehaviour
{
    [Header("Settings")]
    public int enemyID;
    
    [Tooltip("Type the EXACT name of your animation state from the Animator window here")]
    public string popAnimationName = "SootPop"; 

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayPop()
    {
        if (animator != null)
        {
            // Tell the animator to play the state
            animator.Play(popAnimationName, 0, 0f);
            
            // Start the self-destruct timer based on the animation length
            StartCoroutine(DestroyAfterAnimation());
        }
    }

    private IEnumerator DestroyAfterAnimation()
    {
        // WAIT ONE FRAME: Unity needs a split second to transition the animator 
        // into the new 'popAnimationName' state before we can measure it.
        yield return null;

        // Grab the length (in seconds) of whatever animation is currently playing on layer 0
        float exactAnimLength = animator.GetCurrentAnimatorStateInfo(0).length;

        // Destroy this exact enemy when the time runs out
        Destroy(gameObject, exactAnimLength);
    }
}