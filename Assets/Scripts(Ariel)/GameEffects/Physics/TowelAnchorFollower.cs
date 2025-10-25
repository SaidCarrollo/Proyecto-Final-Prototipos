using System;
using UnityEngine;

[DisallowMultipleComponent]
public class TowelAnchorFollower : MonoBehaviour
{
    [Header("Smoothing")]
    public float positionSmoothTime = 0.12f;
    public float rotationLerpSpeed = 10f;
    public float snapEpsilon = 0.001f;

    [Header("Sticky / Hysteresis")]
    public bool sticky = true;
    public float stickEnterRadius = 0.05f;   // radio para ENTRAR a pegado
    public float stickExitRadius = 0.12f;   // radio para SALIR (debe ser > enter)
    public float minStickTime = 0.20f;   // cuánto mínimo permanece pegado
    public float detachHoldTime = 0.12f;   // cuánto tiempo mantener > exit para soltar

    [Header("Follow Strength")]
    [Tooltip("Velocidad máx. a la que el follower persigue el anchor (m/s).")]
    public float followMaxSpeed = 3.0f;

    public Action OnAutoDetached;

    private Transform target;
    private Transform pivot;
    private bool alignRotation;
    private bool active;

    private Rigidbody rb;
    private Cloth clothRef;
    private Vector3 vel;

    // guardamos estado original del RB
    private bool rbWasKinematic, rbWasUsingGravity;

    // Sticky state
    private bool isStuck;
    private float stuckSince, exitTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        clothRef = GetComponent<Cloth>();
        if (rb != null)
        {
            rbWasKinematic = rb.isKinematic;
            rbWasUsingGravity = rb.useGravity;
        }
    }

    public void Attach(Transform target, Transform pivot = null, bool alignRotation = true,
                       float posSmoothTime = 0.12f, float rotLerpSpeed = 10f)
    {
        this.target = target;
        this.pivot = pivot;
        this.alignRotation = alignRotation;
        this.positionSmoothTime = Mathf.Max(0.0001f, posSmoothTime);
        this.rotationLerpSpeed = Mathf.Max(0f, rotLerpSpeed);
        this.active = (target != null);
        vel = Vector3.zero;

        // Reinicia sticky
        isStuck = false;
        stuckSince = 0f;
        exitTimer = 0f;

        // Importante: NO tocar isKinematic aquí.
        // Solo lo ponemos true cuando realmente "pegó" (dist <= enter).
    }

    public void ConfigureStickiness(bool sticky, float enterR, float exitR, float minTime, float holdTime, float sleepAfter /*unused*/)
    {
        this.sticky = sticky;
        this.stickEnterRadius = Mathf.Max(0f, enterR);
        this.stickExitRadius = Mathf.Max(this.stickEnterRadius + 0.001f, exitR);
        this.minStickTime = Mathf.Max(0f, minTime);
        this.detachHoldTime = Mathf.Max(0f, holdTime);
    }

    public void Detach()
    {
        active = false;
        target = null;
        isStuck = false;
        vel = Vector3.zero;

        if (rb != null)
        {
            // Volver al estado original
            rb.isKinematic = rbWasKinematic;
            rb.useGravity = rbWasUsingGravity;
            rb.WakeUp();
        }
    }

    void FixedUpdate()
    {
        if (rb != null) Tick(Time.fixedDeltaTime, useRb: true);
    }

    void Update()
    {
        if (rb == null) Tick(Time.deltaTime, useRb: false);
    }

    void Tick(float dt, bool useRb)
    {
        if (!active || target == null) return;

        Vector3 pivotWorld = (pivot != null ? pivot.position : transform.position);
        Vector3 toAnchor = target.position - pivotWorld;
        float dist = toAnchor.magnitude;

        bool allowFollow = true;

        if (sticky)
        {
            if (!isStuck)
            {
                // Entrar a pegado
                if (dist <= stickEnterRadius)
                {
                    isStuck = true;
                    stuckSince = Time.time;
                    exitTimer = 0f;

                    // Ahora sí: kinematic mientras está pegado, para posicionar exacto
                    if (rb != null)
                    {
                        rbWasKinematic = rb.isKinematic;
                        rbWasUsingGravity = rb.useGravity;
                        rb.isKinematic = true;
                        rb.linearVelocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    clothRef?.ClearTransformMotion();
                }
            }
            else
            {
                // Intento de alejamiento: si supera el radio de salida, dejamos de seguir y contamos
                if (Time.time - stuckSince >= minStickTime)
                {
                    if (dist >= stickExitRadius)
                    {
                        allowFollow = false; // deja de atraer
                        exitTimer += dt;

                        // Y MUY IMPORTANTE: dejemos de ser kinematic para que puedas jalar
                        if (rb != null && rb.isKinematic)
                        {
                            rb.isKinematic = false;
                            rb.useGravity = rbWasUsingGravity;
                            rb.WakeUp();
                        }

                        if (exitTimer >= detachHoldTime)
                        {
                            Detach();
                            OnAutoDetached?.Invoke();
                            return;
                        }
                    }
                    else
                    {
                        exitTimer = 0f;
                    }
                }
            }
        }

        // Seguir al anchor con tope de velocidad (si allowFollow)
        Vector3 desiredPos = transform.position + toAnchor;
        float maxSpeed = allowFollow ? Mathf.Max(0.01f, followMaxSpeed) : 0f;
        Vector3 newPos = Vector3.SmoothDamp(transform.position, desiredPos, ref vel, positionSmoothTime, maxSpeed, dt);

        // Snap exacto muy cerca
        if (dist <= snapEpsilon && allowFollow)
            newPos = transform.position + toAnchor;

        if (useRb && rb != null && rb.isKinematic) rb.MovePosition(newPos);
        else transform.position = newPos;

        if (alignRotation)
        {
            Quaternion desiredRot = target.rotation;
            float t = 1f - Mathf.Exp(-rotationLerpSpeed * dt);
            Quaternion newRot = Quaternion.Slerp(transform.rotation, desiredRot, t);

            if (useRb && rb != null && rb.isKinematic) rb.MoveRotation(newRot);
            else transform.rotation = newRot;
        }
    }

    public bool IsActive => active && target != null;
    public bool IsStuck => isStuck;
}
