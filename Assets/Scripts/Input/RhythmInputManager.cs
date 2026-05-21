using UnityEngine;
using UnityEngine.InputSystem;
using SootDDR.Core;

namespace SootDDR.Input
{
    /// <summary>
    /// Thin wrapper around UnityEngine.InputSystem.Keyboard.
    /// All device-specific logic is isolated here so the rest of the codebase
    /// never touches Keyboard directly — swap this class to support DDR pads,
    /// MIDI controllers, or other input devices without touching game logic.
    /// </summary>
    public class RhythmInputManager : MonoBehaviour
    {
        /// <summary>
        /// Returns the direction matching the currently held arrow keys, or null if none held.
        /// Diagonal combinations (two keys held simultaneously) are checked before cardinal
        /// directions so UpRight is returned instead of Up or Right individually.
        /// </summary>
        public NoteDirection? GetActiveDirection()
        {
            var kb = Keyboard.current;
            if (kb == null) return null;

            bool u = kb.upArrowKey.isPressed;
            bool d = kb.downArrowKey.isPressed;
            bool l = kb.leftArrowKey.isPressed;
            bool r = kb.rightArrowKey.isPressed;

            // Diagonals take priority over cardinals
            if (u && r) return NoteDirection.UpRight;
            if (u && l) return NoteDirection.UpLeft;
            if (d && r) return NoteDirection.DownRight;
            if (d && l) return NoteDirection.DownLeft;

            if (u) return NoteDirection.Up;
            if (d) return NoteDirection.Down;
            if (l) return NoteDirection.Left;
            if (r) return NoteDirection.Right;

            return null;
        }

        /// <summary>
        /// Returns true on the first frame any arrow key transitions from released to pressed.
        /// Use this to trigger a timing check without registering held-key repeats.
        /// </summary>
        public bool AnyArrowPressedThisFrame()
        {
            var kb = Keyboard.current;
            if (kb == null) return false;

            return kb.upArrowKey.wasPressedThisFrame
                || kb.downArrowKey.wasPressedThisFrame
                || kb.leftArrowKey.wasPressedThisFrame
                || kb.rightArrowKey.wasPressedThisFrame;
        }
    }
}
