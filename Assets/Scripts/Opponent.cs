using UnityEngine;

public class Opponent : MonoBehaviour
{
    public float moveForce = 10f;
    public float maxSpeed = 5f;

    [Header("Lunge")]
    public float lungeRange = 3f;
    public float windupTime = 0.4f;
    public float lungeForce = 6f;
    public float lungeCooldown = 2.5f;

    [Header("Contact Knockback")]
    public float hitForce = 12f;
    public float hitUpward = 3f;
    public float hitStunTime = 0.35f;
    public float hitCooldown = 0.75f;

    public Transform player;

    private Rigidbody rb;
    private bool winding;
    private float windupEnd;
    private float nextLungeTime;
    private float nextHitTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
            }
        }
    }

    void FixedUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;
        direction.Normalize();

        //windup brakes then fires the lunge
        if (winding)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.8f, rb.linearVelocity.y, rb.linearVelocity.z * 0.8f);

            if (Time.time >= windupEnd)
            {
                rb.AddForce(direction * lungeForce, ForceMode.Impulse);
                winding = false;
                nextLungeTime = Time.time + lungeCooldown;
            }
            return;
        }

        if (distance <= lungeRange && Time.time >= nextLungeTime)
        {
            winding = true;
            windupEnd = Time.time + windupTime;
            return;
        }

        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVelocity.magnitude < maxSpeed)
        {
            rb.AddForce(direction * moveForce);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (Time.time < nextHitTime) return;

        KnockoutPlayerMovement playerMove = collision.collider.GetComponentInParent<KnockoutPlayerMovement>();
        if (playerMove == null) return;

        Vector3 push = playerMove.transform.position - transform.position;
        push.y = 0f;
        push.Normalize();

        playerMove.ReceiveKnockback(push * hitForce + Vector3.up * hitUpward, hitStunTime);
        nextHitTime = Time.time + hitCooldown;
    }
}