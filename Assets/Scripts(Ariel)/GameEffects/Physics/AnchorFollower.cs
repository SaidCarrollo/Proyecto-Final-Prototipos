using System;
using UnityEngine;

[DisallowMultipleComponent]
public class AnchorFollower : MonoBehaviour
{
    public enum FollowMode { Kinematic, Dynamic }

    [Header("Follow Mode")]
    public FollowMode followMode = FollowMode.Dynamic;

    [Header("Smoothing (ambos modos)")]
    public float positionSmoothTime = 0.12f;   // Kinematic
    public float rotationLerpSpeed = 10f;     // ambos
    public float snapEpsilon = 0.001f;

    [Header("Sticky / Hysteresis")]
    public bool sticky = true;
    public float stickEnterRadius = 0.05f;
    public float stickExitRadius = 0.12f;
    public float minStickTime = 0.20f;
    public float detachHoldTime = 0.12f;
    public float settleSleepAfter = 0.20f;
    [Tooltip("Multiplica el umbral de salida: facilita despegar tirando.")]
    public float exitBoostFactor = 1.2f;

    [Header("Dynamic (físicas)")]
    public float posGain = 120f;  // fuerza de muelle
    public float velGain = 25f;   // amortiguación
    public float maxAccel = 150f;  // tope aceleración
    public float maxSpeed = 3.5f;  // tope velocidad

    public Action OnAutoDetached;

    private Transform target;
    private Transform pivot;
    private bool alignRotation = true;
    private bool active;

    private Rigidbody rb;
    private Vector3 velSd;
    private bool rbWasKinematic;
    private bool rbWasUsingGravity;

    private bool isStuck;
    private float stuckSince;
    private float exitTimer;
    private float nearTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rbWasKinematic = rb.isKinematic;
            rbWasUsingGravity = rb.useGravity;
        }
    }

    public void Attach(Transform target, Transform pivot = null, bool alignRotation = true,
                       float posSmoothTime = 0.12f, float rotLerpSpeed = 10f)
    {
        if (target == null) return;

        this.target = target;
        this.pivot = pivot;
        this.alignRotation = alignRotation;
        this.positionSmoothTime = Mathf.Max(0.0001f, posSmoothTime);
        this.rotationLerpSpeed = Mathf.Max(0f, rotLerpSpeed);
        this.active = true;
        velSd = Vector3.zero;

        isStuck = false;
        stuckSince = 0f;
        exitTimer = 0f;
        nearTimer = 0f;

        if (rb != null)
        {
            rbWasKinematic = rb.isKinematic;
            rbWasUsingGravity = rb.useGravity;

            if (followMode == FollowMode.Kinematic) rb.isKinematic = true;
            else rb.isKinematic = false; // Dynamic: dejar físico
        }
    }

    public void ConfigureStickiness(bool sticky, float enterR, float exitR, float minTime, float holdTime, float sleepAfter)
    {
        this.sticky = sticky;
        this.stickEnterRadius = Mathf.Max(0f, enterR);
        this.stickExitRadius = Mathf.Max(this.stickEnterRadius + 0.001f, exitR);
        this.minStickTime = Mathf.Max(0f, minTime);
        this.detachHoldTime = Mathf.Max(0f, holdTime);
        this.settleSleepAfter = Mathf.Max(0f, sleepAfter);
    }

    public void Detach()
    {
        active = false;
        target = null;
        isStuck = false;
        velSd = Vector3.zero;

        if (rb != null)
        {
            rb.isKinematic = rbWasKinematic;
            rb.useGravity = rbWasUsingGravity;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }
    }

    public void ForceDetach() => Detach();

    void FixedUpdate()
    {
        if (rb != null) Tick(Time.fixedDeltaTime, true);
    }
    void Update()
    {
        if (rb == null) Tick(Time.deltaTime, false);
    }

    void Tick(float dt, bool useRb)
    {
        if (!active || target == null) return;

        Vector3 pivotWorld = (pivot != null ? pivot.position : transform.position);
        Vector3 toAnchor = target.position - pivotWorld;
        float dist = toAnchor.magnitude;

        // Sticky
        if (sticky)
        {
            if (!isStuck)
            {
                if (dist <= stickEnterRadius)
                {
                    isStuck = true;
                    stuckSince = Time.time;
                    exitTimer = 0f;
                    nearTimer = 0f;
                    ZeroRB();
                }
            }
            else
            {
                float exitThresh = stickExitRadius * Mathf.Max(1f, exitBoostFactor);
                if (Time.time - stuckSince >= minStickTime)
                {
                    if (dist >= exitThresh)
                    {
                        exitTimer += dt;
                        if (exitTimer >= detachHoldTime)
                        {
                            Detach();
                            OnAutoDetached?.Invoke();
                            return;
                        }
                    }
                    else exitTimer = 0f;
                }

                if (dist <= snapEpsilon * 2f)
                {
                    nearTimer += dt;
                    if (nearTimer >= settleSleepAfter && rb != null)
                    {
                        ZeroRB();
                        rb.Sleep();
                    }
                }
                else nearTimer = 0f;
            }
        }

        // Seguir
        if (useRb)
        {
            if (followMode == FollowMode.Kinematic)
            {
                Vector3 desiredPos = transform.position + toAnchor;
                Vector3 newPos = Vector3.SmoothDamp(transform.position, desiredPos, ref velSd, positionSmoothTime, Mathf.Infinity, dt);
                if (dist <= snapEpsilon) newPos = transform.position + toAnchor;
                rb.MovePosition(newPos);
            }
            else
            {
                // Dynamic: muelle + amortiguador
                Vector3 spring = toAnchor * posGain;
                Vector3 damper = -rb.linearVelocity * velGain;
                Vector3 accel = spring + damper;

                if (accel.sqrMagnitude > maxAccel * maxAccel)
                    accel = accel.normalized * maxAccel;

                rb.AddForce(accel, ForceMode.Acceleration);

                if (rb.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
                    rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
        else
        {
            Vector3 desiredPos = transform.position + toAnchor;
            transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-dt / Mathf.Max(0.0001f, positionSmoothTime)));
        }

        // Rotación
        if (alignRotation)
        {
            Quaternion desiredRot = target.rotation;
            float t = 1f - Mathf.Exp(-rotationLerpSpeed * dt);
            if (useRb && rb != null) rb.MoveRotation(Quaternion.Slerp(transform.rotation, desiredRot, t));
            else transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, t);
        }
    }

    void ZeroRB()
    {
        if (rb == null) return;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public bool IsActive => active && target != null;
    public bool IsStuck => isStuck;
}