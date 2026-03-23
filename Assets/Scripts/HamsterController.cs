using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HamsterController : MonoBehaviour
{
    [Header("Pads & Goal")]
    public LillyPad[] lilyPads;
    public Transform treasureChest;

    [Header("Jump")]
    public float jumpDuration = 0.55f;
    public float jumpHeight = 1.8f;
    public float startTime = 0.3f;

    [Header("Probability")]
    [Range(0f, 1f)]
    public float drownProbability = 0.10f;

    [Header("Coin Trail")]
    public GameObject coinParticlePrefab;
    public float coinTrailDuration = 0.75f;

    [Header("UI")]
    public GameUIManager uiManager;
    public GameObject restartButton;
    public GameObject slider;

    [Header("Drown FX")]
    public GameObject drownParticlePrefab;
    public ParticleSystem waterSparkle;

    public Transform startPosition;

    // 🔥 Direction pattern
    bool reverseDirection = false;

    private int currentPadIndex = 0;
    private int coinsCollected = 0;
    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        coinsCollected = 0;
    }

    public void OnStartClick()
    {
        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        for (int nextIndex = 0; nextIndex < lilyPads.Length; nextIndex++)
        {
            yield return new WaitForSeconds(1f);

            lilyPads[currentPadIndex].SetGlow(false);

            yield return StartCoroutine(JumpToPad(lilyPads[nextIndex].CenterPoint));

            currentPadIndex = nextIndex;

            // 🔥 Flip every 4th pad
            if (currentPadIndex  == 2 || currentPadIndex  == 5 || currentPadIndex  == 8)
            {
                reverseDirection = !reverseDirection;
                Debug.Log("Flipping direction");
                   // Instantly flip the sprite visually while waiting on the pad
                Vector3 scale = transform.localScale;
                scale.x = -scale.x;
                transform.localScale = scale;
            }

            lilyPads[currentPadIndex].SetGlow(true);
            slider.GetComponent<Slider>().value = currentPadIndex + 1;

            // Coins
            if (lilyPads[currentPadIndex].HasCoin)
            {
                lilyPads[currentPadIndex].CollectCoin();
                coinsCollected += 100;
                uiManager?.UpdateCoinDisplay(coinsCollected);

                yield return StartCoroutine(
                    CoinTrailToChest(lilyPads[currentPadIndex].CenterPoint)
                );
            }

            // Drown
            if (Random.value < drownProbability)
            {
                yield return StartCoroutine(DrownSequence());
                yield break;
            }
        }

        // Final jump
        lilyPads[currentPadIndex].SetGlow(false);
        yield return new WaitForSeconds(0.4f);

        yield return StartCoroutine(JumpToPad(treasureChest.position));

        yield return new WaitForSeconds(0.5f);

        yield return StartCoroutine(VictorySequence());
    }

    // ─────────────────────────────────────────────
    // JUMP
    // ─────────────────────────────────────────────
    IEnumerator JumpToPad(Vector3 targetPos)
    {
        anim?.SetTrigger("JumpStart");

        yield return new WaitForSeconds(startTime);

        Vector3 startPos = transform.position;

      
        float elapsed = 0f;

        bool midTriggered = false;
        bool landTriggered = false;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);
            float smoothT = t * t * (3f - 2f * t);

            // MID
            if (!midTriggered && t > 0.1f)
            {
                anim?.SetBool("InAir", true);
                midTriggered = true;
            }

            // LAND
            if (!landTriggered && t > 0.65f)
            {
                anim?.SetBool("InAir", false);
                anim?.SetTrigger("JumpLand");
                landTriggered = true;
            }

            Vector3 pos = Vector3.Lerp(startPos, targetPos, smoothT);
            pos.y += Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = pos;

            yield return null;
        }

        transform.position = targetPos;

        yield return new WaitForSeconds(0.2f);
    }


    // ─────────────────────────────────────────────
    // DROWN
    // ─────────────────────────────────────────────
    IEnumerator DrownSequence()
    {
        LillyPad pad = lilyPads[currentPadIndex];
        pad.SetGlow(false);

        anim?.ResetTrigger("JumpStart");
        anim?.ResetTrigger("JumpLand");
        anim?.SetBool("InAir", false);
        anim?.SetTrigger("Drown");

        if (drownParticlePrefab != null)
        {
            GameObject fx = Instantiate(drownParticlePrefab, transform.position, Quaternion.identity);
            Destroy(fx, 2f);
        }

        if (waterSparkle != null)
        {
            ParticleSystem ps = Instantiate(waterSparkle, transform.position, Quaternion.identity);
            ps.Play();
            Destroy(ps.gameObject, 2f);
        }

        StartCoroutine(SinkWithPad(pad));

        yield return new WaitForSeconds(1.2f);

        uiManager?.ShowDrownPanel(coinsCollected);
        restartButton.SetActive(true);
    }

    IEnumerator SinkWithPad(LillyPad pad)
    {
        float offset = 0f;

        while (pad.IsDrowning)
        {
            offset += 0.6f * Time.deltaTime;

            Vector3 pos = pad.CenterPoint;
            pos.y -= offset;

            transform.position = pos;

            yield return null;
        }
    }

    // ─────────────────────────────────────────────
    // VICTORY
    // ─────────────────────────────────────────────
    IEnumerator VictorySequence()
    {
        anim?.SetTrigger("Victory");
        yield return new WaitForSeconds(0.6f);
        uiManager?.ShowVictoryPanel(coinsCollected);
    }

    // ─────────────────────────────────────────────
    // COIN TRAIL (UNCHANGED)
    // ─────────────────────────────────────────────
    IEnumerator CoinTrailToChest(Vector3 fromPos)
    {
        if (coinParticlePrefab == null || treasureChest == null)
            yield break;

        int coinCount = 5;

        for (int i = 0; i < coinCount; i++)
        {
            Vector3 scatter = new Vector3(
                Random.Range(-0.25f, 0.25f),
                Random.Range(-0.25f, 0.25f),
                0f
            );

            GameObject c = Instantiate(coinParticlePrefab, fromPos + scatter, Quaternion.identity);
            StartCoroutine(MoveCoinToChest(c, fromPos + scatter, i * 0.1f));
        }

        yield return new WaitForSeconds(coinTrailDuration + (coinCount * 0.1f) + 0.2f);
    }

    IEnumerator MoveCoinToChest(GameObject coin, Vector3 from, float delay)
    {
        if (coin == null) yield break;

        SpriteRenderer coinSr = coin.GetComponent<SpriteRenderer>();

        if (coinSr != null) coinSr.color = new Color(1f, 1f, 1f, 0f);
        coin.transform.localScale = Vector3.zero;

        yield return new WaitForSeconds(delay);

        if (coinSr != null) coinSr.color = Color.white;

        float popElapsed = 0f;
        float popDuration = 0.1f;

        while (popElapsed < popDuration)
        {
            popElapsed += Time.deltaTime;
            float pt = popElapsed / popDuration;

            float s = pt < 0.7f
                ? Mathf.Lerp(0f, 1.2f, pt / 0.7f)
                : Mathf.Lerp(1.2f, 1f, (pt - 0.7f) / 0.3f);

            coin.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }

        coin.transform.localScale = Vector3.one;

        Vector3 to = treasureChest.position;
        float elapsed = 0f;
        float arcHeight = Random.Range(0.6f, 1.4f);
        float arcSide = Random.Range(-0.3f, 0.3f);

        while (elapsed < coinTrailDuration)
        {
            if (coin == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / coinTrailDuration);
            float eased = Mathf.Pow(t, 1.8f);

            Vector3 pos = Vector3.Lerp(from, to, eased);
            pos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            pos.x += Mathf.Sin(t * Mathf.PI) * arcSide;
            coin.transform.position = pos;

            if (t > 0.8f)
            {
                float shrink = 1f - ((t - 0.8f) / 0.2f);
                coin.transform.localScale = new Vector3(shrink, shrink, 1f);

                if (coinSr != null)
                    coinSr.color = new Color(1f, 1f, 1f, shrink);
            }

            yield return null;
        }

        Destroy(coin);
    }

    // ─────────────────────────────────────────────
    // RESTART
    // ─────────────────────────────────────────────
    public void RestartGame()
    {
        StopAllCoroutines();

        currentPadIndex = 0;
        coinsCollected = 0;
        reverseDirection = false;

        slider.GetComponent<Slider>().value = 0;

        foreach (var pad in lilyPads)
            pad.ResetPad();

        transform.position = startPosition.position;

        anim?.SetTrigger("Idle");
        StartCoroutine(GameLoop());
    }
}