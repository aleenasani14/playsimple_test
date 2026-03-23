using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LillyPad : MonoBehaviour
{
    [Header("References")]
    public Transform  centerPoint;          // exact landing position for hamster
    public GameObject coin;                 // coin child object
    public GameObject glowEffect;           // glow particle / sprite child
    public GameObject drownVFX;             // optional ripple/splash VFX on drown
    public GameObject waveRipples;
    public Vector3 waveRipplesOffset = new Vector3(0, -0.1f, 0);

    [Header("Drown Animation")]
    public float drownSinkDepth  = -2.5f;  // how far it sinks (world Y)
    public float drownSinkSpeed  = 1.2f;   // seconds to reach full depth
    public float respawnDelay    = 2.5f;   // seconds before it pops back up
    public float respawnRiseSpeed = 0.5f;  // seconds to rise back to surface

    [Header("Idle Bob (micro-animation)")]
    public bool  idleBobEnabled = true;
    public float bobAmplitude   = 0.04f;
    public float bobFrequency   = 1.1f;
    

    // ─────────────────────────────────────────────
    //  Public read state (used by HamsterController)
    // ─────────────────────────────────────────────
    public bool HasCoin    { get; private set; }
    public bool IsDrowning { get; private set; }

    /// <summary>Returns the world-space center point for hamster landing.</summary>
    public Vector3 CenterPoint =>
        centerPoint != null ? centerPoint.position : transform.position;

    // ─────────────────────────────────────────────
    //  Private
    // ─────────────────────────────────────────────
    private Vector3 _originalPosition;
    private float   _bobTimer;

    // ─────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────
    void Start()
    {
        _originalPosition = transform.position;

        HasCoin = Random.value < 0.5f;
        if (coin != null)       coin.SetActive(HasCoin);
        if (glowEffect != null) glowEffect.SetActive(false);
        if (drownVFX != null)   drownVFX.SetActive(false);

        if (idleBobEnabled)
        {
            // Stagger bob phase per pad so they don't all move in sync
            _bobTimer = Random.Range(0f, Mathf.PI * 2f);
            StartCoroutine(IdleBob());
        }
    }

    // ─────────────────────────────────────────────
    //  Idle Bob (micro-animation — makes scene feel alive)
    // ─────────────────────────────────────────────
    IEnumerator IdleBob()
    {
        while (true)
        {
            if (!IsDrowning)
            {
                _bobTimer += Time.deltaTime * bobFrequency;
                float offset = Mathf.Sin(_bobTimer) * bobAmplitude;
                transform.position = _originalPosition + Vector3.up * offset;
            }
            yield return null;
        }
    }

    // ─────────────────────────────────────────────
    //  Glow
    // ─────────────────────────────────────────────
    public void SetGlow(bool active)
    {
        if (glowEffect != null) glowEffect.SetActive(active);
        if (waveRipples != null) {
            waveRipples.transform.position = centerPoint.position + waveRipplesOffset;
            waveRipples.SetActive(active);
        }
    }

    // ─────────────────────────────────────────────
    //  Coin
    // ─────────────────────────────────────────────
    public void CollectCoin()
    {
        HasCoin = false;
        if (coin != null) coin.SetActive(false);
        // Optional: play a quick coin-pop animation here
        // e.g. StartCoroutine(CoinPopAnim());
    }

    // ─────────────────────────────────────────────
    //  Drown — sinks the pad, then respawns
    //  Returns IEnumerator so HamsterController can yield on it
    // ─────────────────────────────────────────────
    public IEnumerator DrownPad()
    {
        IsDrowning = true;
        SetGlow(false);

        // Spawn ripple/splash VFX
        if (drownVFX != null)
        {
            drownVFX.SetActive(true);
            Invoke(nameof(HideDrownVFX), 1.5f);
        }

        // ── Sink ──────────────────────────────────
        Vector3 startPos  = transform.position;
        Vector3 targetPos = new Vector3(startPos.x, drownSinkDepth, startPos.z);

        float elapsed = 0f;
        while (elapsed < drownSinkSpeed)
        {
            elapsed           += Time.deltaTime;
            float t            = elapsed / drownSinkSpeed;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            _originalPosition  = transform.position;   // keep bob origin in sync
            yield return null;
        }

        transform.position = targetPos;
        gameObject.SetActive(false);

        // ── Wait ──────────────────────────────────
        yield return new WaitForSeconds(respawnDelay);

        // ── Respawn ───────────────────────────────
        Vector3 surfacePos = new Vector3(startPos.x, startPos.y, startPos.z);
        transform.position = targetPos;
        gameObject.SetActive(true);
        IsDrowning         = false;

        elapsed = 0f;
        while (elapsed < respawnRiseSpeed)
        {
            elapsed           += Time.deltaTime;
            float t            = elapsed / respawnRiseSpeed;
            transform.position = Vector3.Lerp(targetPos, surfacePos, t);
            _originalPosition  = transform.position;
            yield return null;
        }

        transform.position = surfacePos;
        _originalPosition  = surfacePos;

        // Rerandomize coin after respawn
        HasCoin = Random.value < 0.5f;
        if (coin != null) coin.SetActive(HasCoin);
    }

    void HideDrownVFX()
    {
        if (drownVFX != null) drownVFX.SetActive(false);
    }

    // ─────────────────────────────────────────────
    //  Full Reset (called by RestartGame)
    // ─────────────────────────────────────────────
    public void ResetPad()
    {
        StopAllCoroutines();

        IsDrowning         = false;
        transform.position = _originalPosition;
        gameObject.SetActive(true);

        HasCoin = Random.value < 0.5f;
        if (coin != null)       coin.SetActive(HasCoin);
        if (glowEffect != null) glowEffect.SetActive(false);
        if (drownVFX != null)   drownVFX.SetActive(false);

        // Restart idle bob
        if (idleBobEnabled)
            StartCoroutine(IdleBob());
    }
}
