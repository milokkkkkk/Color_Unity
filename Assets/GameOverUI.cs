using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("UI Elements")]
    public GameObject root;
    public Text titleText;
    public Text reasonText;
    public Button exitButton;

    void Awake()
    {
        Instance = this;
        root.SetActive(false);
        exitButton.onClick.AddListener(Hide);
    }

    public static void Show(string reason)
    {
        if (Instance == null) return;
        Instance.root.SetActive(true);
        Instance.titleText.text = "Game Over";
        Instance.reasonText.text = reason;
        Time.timeScale = 0f; // ‘›Õ£”Œœ∑
    }

    void Hide()
    {
        Time.timeScale = 1f;
        root.SetActive(false);
    }
}
