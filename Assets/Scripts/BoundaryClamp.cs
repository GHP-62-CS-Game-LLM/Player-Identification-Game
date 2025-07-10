using UnityEngine;

public class BoundaryClamp : MonoBehaviour
{
    public Vector2 minBounds = new Vector2(-49.5f, -33f);
    public Vector2 maxBounds = new Vector2(49.5f, 33f);

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 position = rb.position;

        // Only clamp if position is out of bounds
        bool outOfBounds = false;

        float clampedX = position.x;
        float clampedY = position.y;

        if (position.x < minBounds.x)
        {
            clampedX = minBounds.x;
            outOfBounds = true;
        }
        else if (position.x > maxBounds.x)
        {
            clampedX = maxBounds.x;
            outOfBounds = true;
        }

        if (position.y < minBounds.y)
        {
            clampedY = minBounds.y;
            outOfBounds = true;
        }
        else if (position.y > maxBounds.y)
        {
            clampedY = maxBounds.y;
            outOfBounds = true;
        }

        if (outOfBounds)
        {
            rb.MovePosition(new Vector2(clampedX, clampedY));
        }
    }
}
