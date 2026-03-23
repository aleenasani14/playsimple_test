using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIManager : MonoBehaviour
{
    [Header("HUD")]
    public TextMeshProUGUI   coinCountText;            // top-bar coin display

    [Header("Victory Panel")]
    public GameObject victoryPanel;
    public TextMeshProUGUI      victoryRewardText;    // "You collected X coins!"
    public TextMeshProUGUI       victoryTitleText;
    // public Button     victoryRestartButton;

    [Header("Drown Panel")]
    public GameObject drownPanel;
    public TextMeshProUGUI       drownSubText;
    public Button     drownRestartButton;

    [Header("Animation")]
    public float panelFadeInDuration = 0.4f;

    [Header("References")]
    public HamsterController hamster;

    // ─────────────────────────────────────────────
    //  Init
    // ─────────────────────────────────────────────
    void Start()
    {
        HideAll();

        // if (victoryRestartButton != null)
        //     victoryRestartButton.onClick.AddListener(OnRestartClicked);

        if (drownRestartButton != null)
            drownRestartButton.onClick.AddListener(OnRestartClicked);
    }

    // ─────────────────────────────────────────────
    //  Public API  (called by HamsterController)
    // ─────────────────────────────────────────────

    /// <summary>Updates the always-visible coin counter.</summary>
    public void UpdateCoinDisplay(int coins)
    {
        if (coinCountText != null)
            coinCountText.text = coins.ToString();
    }

    /// <summary>Shows the Victory panel with reward info.</summary>
    public void ShowVictoryPanel(int coins)
    {
        if (victoryPanel == null) return;

        if (victoryRewardText != null)
            victoryRewardText.text = coins > 0
                ? $"You collected {coins} coin{(coins == 1 ? "" : "s")}!"
                : "You made it across!";

        if (victoryTitleText != null)
            victoryTitleText.text = "VICTORY! 🎉";

        victoryPanel.SetActive(true);
        StartCoroutine(FadeInPanel(victoryPanel));
    }

    /// <summary>Shows the Drown / Game-Over panel.</summary>
    public void ShowDrownPanel(int coins)
    {
        if (drownPanel == null) return;

        if (drownSubText != null)
            drownSubText.text = coins > 0
                ? $"You had {coins} coin{(coins == 1 ? "" : "s")} — so close!"
                : "Lily drowned... Try again!";

        drownPanel.SetActive(true);
        StartCoroutine(FadeInPanel(drownPanel));
    }

    // ─────────────────────────────────────────────
    //  Restart
    // ─────────────────────────────────────────────
    public void OnRestartClicked()
    {
        HideAll();
        if (hamster != null) hamster.RestartGame();
    }

    // ─────────────────────────────────────────────
    //  Helpers
    // ─────────────────────────────────────────────
    void HideAll()
    {
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (drownPanel   != null) drownPanel.SetActive(false);
    }

    /// <summary>Simple alpha fade-in for a panel with a CanvasGroup component.</summary>
    IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) yield break;

        cg.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < panelFadeInDuration)
        {
            elapsed  += Time.deltaTime;
            cg.alpha  = Mathf.Clamp01(elapsed / panelFadeInDuration);
            yield return null;
        }

        cg.alpha = 1f;
    }
}
