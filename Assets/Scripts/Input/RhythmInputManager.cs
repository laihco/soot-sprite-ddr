using UnityEngine;
using UnityEngine.InputSystem;
using SootDDR.Core;

namespace SootDDR.Input
{
    /// <summary>
    /// Handles rhythm game directional input.
    ///
    /// Arrow Keys:
    /// Up / Down / Left / Right
    ///
    /// Diagonals:
    /// Q = UpLeft
    /// E = UpRight
    /// A = DownLeft
    /// D = DownRight
    /// </summary>
    public class RhythmInputManager : MonoBehaviour
    {
        public NoteDirection? GetActiveDirection()
        {
            var kb = Keyboard.current;
            if (kb == null) return null;

            // Cardinal arrows
            bool up = kb.upArrowKey.isPressed;
            bool down = kb.downArrowKey.isPressed;
            bool left = kb.leftArrowKey.isPressed;
            bool right = kb.rightArrowKey.isPressed;

            // Dedicated diagonal keys
            bool upLeft = kb.qKey.isPressed;
            bool upRight = kb.eKey.isPressed;
            bool downLeft = kb.aKey.isPressed;
            bool downRight = kb.dKey.isPressed;

            // Diagonals first
            if (upLeft) return NoteDirection.UpLeft;
            if (upRight) return NoteDirection.UpRight;
            if (downLeft) return NoteDirection.DownLeft;
            if (downRight) return NoteDirection.DownRight;

            // Cardinals
            if (up) return NoteDirection.Up;
            if (down) return NoteDirection.Down;
            if (left) return NoteDirection.Left;
            if (right) return NoteDirection.Right;

            return null;
        }

        /// <summary>
        /// Returns true only on the first pressed frame.
        /// Prevents held input spam.
        /// </summary>
        public bool AnyArrowPressedThisFrame()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;

            return
                kb.upArrowKey.wasPressedThisFrame ||
                kb.downArrowKey.wasPressedThisFrame ||
                kb.leftArrowKey.wasPressedThisFrame ||
                kb.rightArrowKey.wasPressedThisFrame ||

                kb.qKey.wasPressedThisFrame ||
                kb.eKey.wasPressedThisFrame ||
                kb.aKey.wasPressedThisFrame ||
                kb.dKey.wasPressedThisFrame;
        }
    }
}