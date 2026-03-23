using UnityEngine;

public class CoinAnimator : MonoBehaviour
{
    [Header("Sprite Sheet")]
    public Sprite[] frames;          // drag all frames from your sprite sheet here
    public float fps = 12f;          // how fast it cycles through frames

    private SpriteRenderer sr;
    private float timer;
    private int currentFrame;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        timer += Time.deltaTime;

        if (timer >= 1f / fps)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % frames.Length;
            sr.sprite = frames[currentFrame];
        }
    }
}