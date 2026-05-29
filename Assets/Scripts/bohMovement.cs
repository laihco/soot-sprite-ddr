using UnityEngine;

public class SineMover : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float moveSpeed = 2f;
    public float moveDistance = 3f;

    [Header("Bounce Movement")]
    public float bounceHeight = 1f;
    public float bounceSpeed = 2f;

    private Vector3 startPosition;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Calculate movement
        float x = Mathf.Sin(Time.time * moveSpeed) * moveDistance;
        float y = Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;

        // Apply movement
        transform.position = startPosition + new Vector3(x, y, 0f);

        // Flip sprite when moving right
        if (spriteRenderer != null)
        {
            float horizontalDirection = Mathf.Cos(Time.time * moveSpeed);

            // Facing right
            if (horizontalDirection > 0)
                spriteRenderer.flipX = true;

            // Facing left
            else if (horizontalDirection < 0)
                spriteRenderer.flipX = false;
        }
    }
}