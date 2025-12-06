using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is used for the visual effect of a crafted item moving from the crafting
/// output slot to the corresponding inventory tab. It works by directly
/// manipulating the particle array of a Particle System.
/// </summary>
public class ItemParticleController : MonoBehaviour
{
    private ParticleSystem ps;
    private ParticleSystem.Particle[] particles;

    [Header("Animation Target")]
    public Transform target; // destination

    [Header("Animation Timgings")]
    public float popDuration = 0.3f;
    public float waitDuration = 0.2f;
    public float flyDuration = 0.5f;

    private float timer;
    private bool particlesInitialized = false;

    // A reference to the original prefab, needed to return the instance to the correct pool.
    public GameObject sourcePrefab;

    // Arrival callback
    private Action _onArriveCallback;
    private bool _hasTriggeredCallback = false;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[ps.main.maxParticles];
    }

    /// <summary>
    /// Starts the particle flight animation.
    /// This is the main entry point called by other scripts
    /// </summary>
    /// <param name="newTarget">The destination Transform</param>
    /// <param name="prefab">The original prefab of this particle system instance.</param>
    public void Play(Transform newTarget, GameObject prefab, Action onArrive = null)
    {
        this.target = newTarget;
        this.sourcePrefab = prefab;
        this._onArriveCallback = onArrive;

        // Reset state variables for a new animation sequence.
        timer = 0;
        particlesInitialized = false;
        gameObject.SetActive(true);
        ps.Play();
    }

    private void Update()
    {
        if (target == null) return;

        // Initialize particles position
        if (!particlesInitialized && ps.particleCount > 0)
        {
            int numParticlesAlive = ps.GetParticles(particles);
            ps.SetParticles(particles, numParticlesAlive);
            particlesInitialized = true;
        }

        timer += Time.deltaTime;
        int numParticles = ps.GetParticles(particles);

        float startFlyTime = popDuration + waitDuration;

        for (int i = 0; i < numParticles; i++)
        {
            if (timer < popDuration)
            {
                // 1. pop phase: controlled by Unity
            }
            else if (timer < startFlyTime)
            {
                // 2. hover phase: slow down
                particles[i].velocity = Vector3.Lerp(particles[i].velocity, Vector3.zero, Time.deltaTime * 5f);
            }
            else
            {
                // 3. fly phase
                float flyProgress = (timer - startFlyTime) / flyDuration;
                Vector3 targetPos = target.position;
                float dist = Vector3.Distance(particles[i].position, targetPos);

                // If nearly finished or very close, snap directly to target.
                if (flyProgress > 0.85f || dist < 2.0f)
                {
                    // A. Kill velocity to stop orbiting
                    particles[i].velocity = Vector3.zero;

                    // B. Lerp position directly
                    particles[i].position = Vector3.Lerp(particles[i].position, targetPos, Time.deltaTime * 20f);

                    // C. Shrink to zero to simulate "entering" the target
                    if (dist < 0.1f)
                    {
                        particles[i].startSize = 0f;
                    }
                }
                else
                {
                    // --- normal arc flight logic ---
                    float moveSpeed = 10f * (1f + flyProgress * 2f);
                    float turnPower = Mathf.Lerp(2f, 20f, flyProgress);

                    Vector3 toTarget = targetPos - particles[i].position;
                    Vector3 dirToTarget = toTarget.normalized;

                    Vector3 currentDir = particles[i].velocity.normalized;

                    // Only steer if moving, otherwise(velocity is zero) face target immediately
                    if (particles[i].velocity.sqrMagnitude < 0.1f) currentDir = dirToTarget;

                    Vector3 newDir = Vector3.Slerp(currentDir, dirToTarget, Time.deltaTime * turnPower);
                    particles[i].velocity = newDir * moveSpeed;
                }
            }
        }

        ps.SetParticles(particles, numParticles);

        float totalDuration = popDuration + waitDuration + flyDuration;

        if (timer >= totalDuration && !_hasTriggeredCallback)
        {
            _hasTriggeredCallback = true;
            _onArriveCallback?.Invoke();
        }

        if (timer > totalDuration + 0.1f) // time buffer
        {
            if (sourcePrefab != null && GameObjectPoolManager.Instance != null)
            {
                GameObjectPoolManager.Instance.ReturnToPool(sourcePrefab, this.gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
