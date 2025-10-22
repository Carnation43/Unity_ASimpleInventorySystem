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
    public void Play(Transform newTarget, GameObject prefab)
    {
        this.target = newTarget;
        this.sourcePrefab = prefab;

        // Reset state variables for a new animation sequence.
        timer = 0;
        particlesInitialized = false;
        gameObject.SetActive(true);
        ps.Play();
    }

    private void Update()
    {
        if (target == null) return;

        // Initialize particles
        if (!particlesInitialized && ps.particleCount > 0)
        {
            int numParticlesAlive = ps.GetParticles(particles);
            for (int i = 0; i < numParticlesAlive; i++)
            {
                // Set the particle position to the current emitter position
                particles[i].position = transform.position;
            }
            ps.SetParticles(particles, numParticlesAlive);
            particlesInitialized = true;
        }

        timer += Time.deltaTime;
        int numParticles = ps.GetParticles(particles);

        for (int i = 0; i < numParticles; i++)
        {
            if (timer < popDuration)
            {
                // emission will do its job
            }
            else if (timer < popDuration + waitDuration)
            {
                particles[i].velocity *= 0.4f;
            }
            else
            {
                // 1. Calculate the direction vector towards the target.
                Vector3 directionTarget = (target.position - particles[i].position).normalized;

                // 2. Calculate the total animation time and how much is left
                float totalFlyTime = popDuration + waitDuration + flyDuration;
                float remainingTime = totalFlyTime - timer;

                // 3. Calculate the remaining distance to the target.
                float remainingDistance = Vector3.Distance(particles[i].position, target.position);

                // 4. Calculate the required speed to cover the remaining distance
                // in the remaining time. Add a smaill epsilon to avoid division by zero.
                float speed = remainingDistance / (remainingTime + 0.001f);

                // 5. Apply the new velocity to the particle
                particles[i].velocity = directionTarget * speed;
            }
        }

        // Apply all particle changes back to the system
        ps.SetParticles(particles, numParticles);

        // Clean up
        if (timer > popDuration + waitDuration + flyDuration)
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
