using UnityEngine;

namespace SootDDR.Core
{
    /// <summary>The eight possible note directions, matching the DDR-style cross + diagonals.</summary>
    public enum NoteDirection
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
    }

    public static class NoteDirectionHelper
    {
        /// <summary>
        /// Returns the outward direction vector from the center for a given note direction.
        /// Used to compute both spawn position and ring-edge target position.
        /// </summary>
        public static Vector2 ToVector(this NoteDirection dir)
        {
            return dir switch
            {
                NoteDirection.Up        => Vector2.up,
                NoteDirection.Down      => Vector2.down,
                NoteDirection.Left      => Vector2.left,
                NoteDirection.Right     => Vector2.right,
                NoteDirection.UpLeft    => new Vector2(-1f,  1f).normalized,
                NoteDirection.UpRight   => new Vector2( 1f,  1f).normalized,
                NoteDirection.DownLeft  => new Vector2(-1f, -1f).normalized,
                NoteDirection.DownRight => new Vector2( 1f, -1f).normalized,
                _                       => Vector2.zero,
            };
        }

        /// <summary>
        /// Returns the world-space grid position for this direction using a uniform square grid.
        /// Unlike ToVector(), diagonals are NOT normalized — all holes sit on the same square grid.
        ///   Up → (0, spacing)   UpLeft → (-spacing, spacing)   etc.
        /// </summary>
        public static Vector2 ToGridPosition(this NoteDirection dir, float spacing = 1f)
        {
            return dir switch
            {
                NoteDirection.Up        => new Vector2( 0f,       spacing),
                NoteDirection.Down      => new Vector2( 0f,      -spacing),
                NoteDirection.Left      => new Vector2(-spacing,   0f),
                NoteDirection.Right     => new Vector2( spacing,   0f),
                NoteDirection.UpLeft    => new Vector2(-spacing,   spacing),
                NoteDirection.UpRight   => new Vector2( spacing,   spacing),
                NoteDirection.DownLeft  => new Vector2(-spacing,  -spacing),
                NoteDirection.DownRight => new Vector2( spacing,  -spacing),
                _                       => Vector2.zero,
            };
        }

        /// <summary>Case-insensitive parse from the JSON "hole" string field.</summary>
        public static bool TryParse(string value, out NoteDirection dir) =>
            System.Enum.TryParse(value, ignoreCase: true, out dir);
    }
}
