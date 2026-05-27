using UnityEngine;
using UnityEngine.InputSystem;

public class RhythmGameInput : MonoBehaviour
{

    public SpriteRenderer panelUp;
    public SpriteRenderer panelDown;
    public SpriteRenderer panelLeft;
    public SpriteRenderer panelRight;
    public SpriteRenderer panelUpLeft;
    public SpriteRenderer panelUpRight;
    public SpriteRenderer panelDownLeft;
    public SpriteRenderer panelDownRight;

    public Color activateColor = Color.yellow;
    public Color inactivatedColor = Color.white;

    private PadInput pad;

    public enum padDirection {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight,
        None
    }

    private padDirection currentDirection = padDirection.None;
    private padDirection previousDirection = padDirection.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pad = GetComponent<KeyboardInput>();
        SetAll(inactivatedColor);
    }

    // Update is called once per frame
    void Update()
    {
        bool up = pad.Up;
        bool down = pad.Down;
        bool left = pad.Left;
        bool right = pad.Right;

        SetAll(inactivatedColor);

        if (up && right) {
            Set(panelUpRight);
        } else if (up && left) {
            Set(panelUpLeft);
        } else if (down && right) {
            Set(panelDownRight);
        } else if (down && left) {
            Set(panelDownLeft);
        } else if (up) {
            Set(panelUp);
        } else if (down) {
            Set(panelDown);
        } else if (left) {
            Set(panelLeft);
        } else if (right) {
            Set(panelRight);
        }
    }

    void Set(params SpriteRenderer[] panels) {
       foreach (var p in panels) {
            if (p) p.color = activateColor;
        }
    }

    void SetAll(Color color) {
        foreach (var p in new SpriteRenderer[] { panelUp, panelDown, panelLeft, panelRight, panelUpLeft, panelUpRight, panelDownLeft, panelDownRight }) {
            if (p) p.color = color;
        }
    }


}
