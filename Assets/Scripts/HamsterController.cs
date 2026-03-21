using System.Collections;
using UnityEngine;

public class HamsterController : MonoBehaviour
{
    public Transform[] lilyPads;           // assign in inspector, index 0 = start
    public float jumpDuration = 0.55f;
    public float jumpHeight   = 1.8f;      // good visible arc

    private int currentIndex = 0;
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();

        if (lilyPads == null || lilyPads.Length == 0)
        {
            Debug.LogError("No lily pads assigned!");
            return;
        }

        // Optional: snap to first pad at beginning
        transform.position = lilyPads[0].position;

        StartCoroutine(GameLoop());
    }

    IEnumerator GameLoop()
    {
        while (currentIndex < lilyPads.Length)
        {
            // Idle delay — feels more natural if slightly randomized
            float idleWait = Random.Range(0.35f, 0.65f);
            yield return new WaitForSeconds(idleWait);

            // Jump to **next** pad (currentIndex is the target now)
            yield return StartCoroutine(JumpToPad(lilyPads[currentIndex]));

            // After landing → decide fate
            if (Random.value < 0.12f)          // ~12% drown chance — tune this
            {
                Debug.Log("DROWN 💀");
                if (anim != null) anim.SetTrigger("Drown"); // ← if you have this anim
                yield return new WaitForSeconds(0.7f);
                ResetToStart();
                yield break;                    // stop this coroutine
            }

            currentIndex++;
        }

        Debug.Log("VICTORY 🎉");
        if (anim != null) anim.SetTrigger("Victory");   // if you have victory anim
        // → here you can later show UI, play sound, load next level, etc.
    }

    IEnumerator JumpToPad(Transform target)
    {
        if (anim != null) anim.SetBool("isJumping", true);

        // Small anticipation / wind-up (looks alive)
        yield return new WaitForSeconds(0.08f);

        Vector3 startPos = transform.position;
        Vector3 endPos   = target.position;

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;           // 0 → 1

            // Base horizontal/vertical movement
            Vector3 pos = Vector3.Lerp(startPos, endPos, t);

            // Parabolic arc (very classic & smooth)
            float arc = 4f * jumpHeight * t * (1f - t); // peaks at 0.5
            pos.y += arc;

            transform.position = pos;

            yield return null;
        }

        // Force exact final position (avoids float imprecision)
        transform.position = endPos;

        if (anim != null) anim.SetBool("isJumping", false);
    }

    void ResetToStart()
    {
        currentIndex = 0;
        transform.position = lilyPads[0].position;

        // Optional: reset animator if needed
        if (anim != null)
        {
            anim.SetBool("isJumping", false);
            // anim.Play("Idle", -1, 0f);   // if you want forced reset
        }

        StartCoroutine(GameLoop());
    }

    // Optional helper — call from Unity button / input later
    public void ManualReset()
    {
        StopAllCoroutines();
        ResetToStart();
    }
}