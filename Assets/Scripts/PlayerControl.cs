using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class KnockoutPlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float maxSpeed = 8f;
    public float acceleration = 40f;
    public float deceleration = 50f;
    [Range(0f, 1f)]
    public float airControl = 0.5f;
    public bool cameraRelative = false;

    [Header("Jumping")]
    public float jumpHeight = 1.5f;
    public float coyoteTime = 0.1f;      //Meep meep. Grace period to jump after walking off a ledge
    public float jumpBufferTime = 0.1f;  //Grace period to register a jump pressed just before landing

    [Header("Gravity")]
    public float fallGravityMultiplier = 2.5f;     //Extra gravity while falling
    public float lowJumpGravityMultiplier = 2f;    //Variable jump height

    [Header("Ground Check")]
    public LayerMask groundLayers = ~0;  //Layers that count as ground
    public float groundCheckDistance = 0.1f;
    public float groundCheckRadiusScale = 0.9f;

    [Header("Knockback Pulse")]
    public float pulseRadius = 3f;
    public float pulseForce = 12f;
    public float upwardForce = 3f;
    public float pulseCooldown = 1f;
    public LayerMask pulseLayers = ~0;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector2 moveInput;
    private bool jumpHeld;
    private bool isGrounded;
    private float nextPulseTime;
    private float knockbackUntil;
    private float lastGroundedTime = -999f;
    private float lastJumpPressedTime = -999f;

    private readonly Collider[] groundHits = new Collider[8];

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();

        //Keep the capsule upright, no physics driven rotation
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        //Zero friction so player doesnt stick to walls
        PhysicsMaterial mat = new PhysicsMaterial("PlayerNoFriction")
        {
            dynamicFriction = 0f,
            staticFriction = 0f,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Minimum
        };
        capsule.material = mat;
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        //Movement input
        float x = 0f;
        float z = 0f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) z += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) z -= 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) x += 1f;

        moveInput = Vector2.ClampMagnitude(new Vector2(x, z), 1f);

        //Jump input
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            lastJumpPressedTime = Time.time;

        jumpHeld = Keyboard.current.spaceKey.isPressed;

        //Knockback pulse
        if (Keyboard.current.eKey.wasPressedThisFrame && Time.time >= nextPulseTime)
        {
            EmitPulse();
            nextPulseTime = Time.time + pulseCooldown;
        }
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleHorizontalMovement();
        HandleJump();
        ApplyExtraGravity();
    }

    //Ground detection
    void CheckGround()
    {
        Bounds b = capsule.bounds;
        float radius = capsule.radius *
                       Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) *
                       groundCheckRadiusScale;

        //Sphere bottom sits below capsule feet
        Vector3 origin = new Vector3(
            b.center.x,
            b.min.y + radius - groundCheckDistance,
            b.center.z
        );

        int count = Physics.OverlapSphereNonAlloc(
            origin, radius, groundHits, groundLayers,
            QueryTriggerInteraction.Ignore
        );

        isGrounded = false;
        for (int i = 0; i < count; i++)
        {
            //Ignore own colliders
            if (groundHits[i].transform.root == transform.root)
                continue;

            isGrounded = true;
            break;
        }

        if (isGrounded)
            lastGroundedTime = Time.time;
    }

    //Horizontal movement
    void HandleHorizontalMovement()
    {
        //skip control while stunned so the shove carries
        if (Time.time < knockbackUntil)
            return;

        Vector3 inputDir = GetWorldMoveDirection();
        Vector3 targetVelocity = inputDir * maxSpeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);

        float rate = inputDir.sqrMagnitude > 0.001f ? acceleration : deceleration;
        if (!isGrounded)
            rate *= airControl;

        horizontal = Vector3.MoveTowards(horizontal, targetVelocity, rate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector3(horizontal.x, velocity.y, horizontal.z);
    }

    Vector3 GetWorldMoveDirection()
    {
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cameraRelative && Camera.main != null)
        {
            forward = Camera.main.transform.forward;
            right = Camera.main.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
        }

        return forward * moveInput.y + right * moveInput.x;
    }

    //Jumping
    void HandleJump()
    {
        bool wantsToJump = Time.time - lastJumpPressedTime <= jumpBufferTime;
        bool canJump = Time.time - lastGroundedTime <= coyoteTime;

        if (wantsToJump && canJump)
        {
            //v = sqrt(2 * g * h)
            float gravity = Mathf.Abs(Physics.gravity.y);
            float jumpVelocity = Mathf.Sqrt(2f * gravity * jumpHeight);

            Vector3 v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(v.x, jumpVelocity, v.z);

            //Consume both so can't double trigger a jump
            lastJumpPressedTime = -999f;
            lastGroundedTime = -999f;
        }
    }

    //Snappier gravity
    void ApplyExtraGravity()
    {
        if (isGrounded)
            return;

        float vy = rb.linearVelocity.y;

        if (vy < 0f)
        {
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }
        else if (vy > 0f && !jumpHeld)
        {
            rb.AddForce(Physics.gravity * (lowJumpGravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    //called by enemies when they hit the player
    public void ReceiveKnockback(Vector3 force, float stunTime)
    {
        knockbackUntil = Time.time + stunTime;
        rb.AddForce(force, ForceMode.Impulse);
    }

    //Knockback pulse
    void EmitPulse()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            pulseRadius,
            pulseLayers,
            QueryTriggerInteraction.Ignore
        );

        foreach (Collider hit in hits)
        {
            //Dont knock player back
            if (hit.transform.root == transform.root)
                continue;

            Rigidbody targetRb = hit.attachedRigidbody;

            if (targetRb == null || targetRb.isKinematic)
                continue;

            //Push outward, with an upward launch
            Vector3 direction = targetRb.worldCenterOfMass - transform.position;

            if (direction.sqrMagnitude < 0.001f)
                direction = Vector3.up;

            direction.Normalize();

            Vector3 force = direction * pulseForce + Vector3.up * upwardForce;

            targetRb.AddForce(force, ForceMode.Impulse);
        }
    }

    //Display pulse radius in the scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pulseRadius);
    }
}