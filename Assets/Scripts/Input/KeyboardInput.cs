using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInput : MonoBehaviour, PadInput
{
    public bool Up => Keyboard.current.upArrowKey.isPressed;
    public bool Down => Keyboard.current.downArrowKey.isPressed;
    public bool Left => Keyboard.current.leftArrowKey.isPressed;
    public bool Right => Keyboard.current.rightArrowKey.isPressed;
}
