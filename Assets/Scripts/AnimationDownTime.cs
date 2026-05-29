using UnityEngine;

public class AnimationDownTime : StateMachineBehaviour
{
    [Header("Timing Settings")]
    [Tooltip("Exact time (in seconds) to wait before playing the animation.")]
    public float delayTime = 4f; 
    
    [Header("Animator Settings")]
    public string triggerName = "Play";

    private float _timer;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Reset the timer the moment we enter the Idle state
        _timer = 0f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        _timer += Time.deltaTime;

        if (_timer >= delayTime)
        {
            animator.SetTrigger(triggerName);
            
            // Reset timer so we don't fire the trigger multiple times before the transition happens
            _timer = 0f; 
        }
    }
}