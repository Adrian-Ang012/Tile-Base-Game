using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [Header("Teleport Settings")]
    public Transform destination;
    public float cooldown = 0.5f;

    float lastTeleportTime;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time - lastTeleportTime < cooldown) return;

        lastTeleportTime = Time.time;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = destination.position;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            other.transform.position = destination.position;
        }
    }
}
