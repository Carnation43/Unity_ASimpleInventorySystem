using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class InspirationView : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] protected PlayerWallet_SO playerWallet;

    [Header("UI Reference")]
    [SerializeField] TMP_Text amountText;

    [Header("Color Settings")]
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Effects")]
    [Tooltip("Drag in the prefab of a single light sphere")]
    [SerializeField] private GameObject particlePrefab;

    [Header("Emit setting")]
    [Tooltip("particles per second")]
    [SerializeField] private float spawnRate = 15f;
    [SerializeField] private float startDelay = 0.2f;

    [Header("override flying parameters")]
    [SerializeField] private float overridePopDuration = 0.1f;
    [SerializeField] private float overrideWaitDuration = 0.1f;
    [SerializeField] private float overrideFlyDuration = 0.4f;
    // Total time consumed = 0.1 + 0.1 + 0.4 = 0.6 seconds.
    // If your long-press unlock time is 1.0 second,
    // then there will be 0.4 seconds of continuous particle emission.

    private Coroutine _chargeCoroutine;

    private void OnEnable()
    {
        RecipeBookController.OnUnlockChargeStart += StartChargeSequence;
        RecipeBookController.OnUnlockChargeCancel += StopChargeSequence;

        RecipeBookController.OnUnlockFailed += HandleFailed;

        if (playerWallet != null)
        {
            UpdateAmountText();
            playerWallet.OnInspirationChanged += UpdateAmountText;
        }
    }

    private void OnDisable()
    {
        RecipeBookController.OnUnlockChargeStart -= StartChargeSequence;
        RecipeBookController.OnUnlockChargeCancel -= StopChargeSequence;

        RecipeBookController.OnUnlockFailed -= HandleFailed;

        if (playerWallet != null)
        {
            playerWallet.OnInspirationChanged -= UpdateAmountText;
        }
    }

    private void UpdateAmountText()
    {
        if (playerWallet != null && amountText != null)
        {
            amountText.text = "Inspiration: " + playerWallet.CurrentInspiration.ToString();
        }
    }

    private void StartChargeSequence(Transform target, float unlockDuration)
    {
        StopChargeSequence();
        _chargeCoroutine = StartCoroutine(SpawnParticlesRoutine(target, unlockDuration));
    }

    private void StopChargeSequence()
    {
        if (_chargeCoroutine != null)
        {
            StopCoroutine(_chargeCoroutine);
            _chargeCoroutine = null;
        }

        if (amountText != null)
        {
            amountText.transform.localScale = Vector3.one;
        }
    }

    private IEnumerator SpawnParticlesRoutine(Transform target, float totalUnlockDuration)
    {
        yield return new WaitForSeconds(startDelay);

        float interval = 1f / spawnRate;
        float timer = startDelay;

        // 1. Calculate the total flight time of a single particle
        float particleLifeTime = overridePopDuration + overrideWaitDuration + overrideFlyDuration;

        // 2. Calculate the "ceasefire time"
        // If the total duration is 1.0s and the particle flight takes 0.6s. Then the emission must stop at 0.4s.
        // In this way, the last particle emitted at 0.4s will arrive exactly at 1.0s.
        float stopSpawningTime = totalUnlockDuration - particleLifeTime;

        // Prevent incorrect parameter settings from failing to present visual effects
        if (stopSpawningTime <= 0)
        {
            stopSpawningTime = 0.1f;
            Debug.Log("Recipe->InspirationView: inspector parameter settings may error");
        }

        // 3. Loop the launch until the ceasefire time is reached
        while (timer < stopSpawningTime)
        {
            SpawnSingleParticle(target);

            // Text shaking effect
            if (amountText != null)
            {
                amountText.transform.DOKill();
                amountText.transform.localScale = Vector3.one;
                amountText.transform.DOPunchScale(Vector3.one * 0.15f, interval, 1, 0);
            }

            yield return new WaitForSeconds(interval);
            timer += interval;
        }
    }

    private void SpawnSingleParticle(Transform target)
    {
        if (particlePrefab == null || target == null) return;

        Vector3 startPos = transform.position;
        GameObject flyObj = GameObjectPoolManager.Instance.GetFromPool(particlePrefab, startPos, Quaternion.identity);

        if (transform != null) flyObj.transform.SetParent(transform, true);
        flyObj.transform.localScale = Vector3.one;

        ItemParticleController controller = flyObj.GetComponent<ItemParticleController>();
        if (controller != null)
        {
            controller.popDuration = overridePopDuration;
            controller.waitDuration = overrideWaitDuration;
            controller.flyDuration = overrideFlyDuration;

            controller.Play(target, particlePrefab);
        }
    }

    private void HandleFailed()
    {
        if (amountText != null)
        {
            amountText.DOKill();
            amountText.rectTransform.DOKill();
            amountText.color = normalColor;
            amountText.transform.localScale = Vector3.one;

            amountText.DOColor(errorColor, 0.15f).SetLoops(2, LoopType.Yoyo);

            amountText.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f);
        }
    }
}