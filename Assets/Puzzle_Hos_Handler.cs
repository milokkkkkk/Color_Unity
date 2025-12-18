using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Puzzle_Hos_Handler : MonoBehaviour
{
    public Transform startPos;
    public Transform goodPos;
    public GameObject semangCard;

    public float barLength;
    public GameObject timerBarPanel;
    public Image bar;

    public static Puzzle_Hos_Handler instance;

    public float moveDuration = 1;

    private Coroutine co;

    // =========================
    // 【新增】Puzzle 数据
    // =========================
    public List<SemangCardData> semangCards;   // 色盲卡题库
    public int requiredCorrect = 4;             // 连续正确次数

    // =========================
    // 【新增】输入 UI
    // =========================
    public GameObject inputPanel;
    public TMP_InputField inputField;
    public TextMeshProUGUI resultText;

    // =========================
    // 【新增】运行时状态
    // =========================
    private SemangCardData currentCard;
    private int correctCount = 0;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        timerBarPanel.SetActive(false);
        semangCard.SetActive(false);

        inputPanel.SetActive(false);
        resultText.gameObject.SetActive(false);

        Puzzle_Hos_Handler.instance.RestartSemangReadingTiming();
    }

    void Update() { }

    public void RestartSemangReadingTiming()
    {
        if (co != null)
            StopCoroutine(co);

        co = StartCoroutine(SemangReadingIE());
    }

    private IEnumerator SemangReadingIE()
    {
        // =========================
        // 原有：生成并展示色盲卡
        // =========================

        // 随机抽一张卡（新增，但不改流程）
        int index = Random.Range(0, semangCards.Count);
        currentCard = semangCards[index];
        semangCard.GetComponent<Image>().sprite = currentCard.image;

        semangCard.transform.position = startPos.position;
        semangCard.transform.rotation = startPos.rotation;
        semangCard.SetActive(true);

        semangCard.transform.DOLocalMove(
            goodPos.localPosition,
            moveDuration
        );

        semangCard.transform
            .DOLocalRotateQuaternion(goodPos.localRotation, moveDuration)
            .SetEase(Ease.OutBack);

        yield return new WaitForSeconds(moveDuration);

        // =========================
        // 原有：计时条
        // =========================

        timerBarPanel.SetActive(true);

        var totalTime = 5f;
        var restTime = totalTime;

        RectTransform rect = bar.GetComponent<RectTransform>();
        var sizeDelta = rect.sizeDelta;

        while (restTime > 0)
        {
            restTime -= Time.deltaTime;
            var ratio = restTime / totalTime;
            sizeDelta.x = barLength * ratio;
            rect.sizeDelta = sizeDelta;
            yield return null;
        }

        sizeDelta.x = 0;
        rect.sizeDelta = sizeDelta;

        yield return new WaitForSeconds(0.25f);

        // =========================
        // 原有：色盲卡退场
        // =========================

        timerBarPanel.SetActive(false);

        semangCard.transform.DOLocalMove(
            startPos.localPosition,
            moveDuration
        );

        semangCard.transform
            .DOLocalRotateQuaternion(startPos.localRotation, moveDuration)
            .SetEase(Ease.InBack);

        yield return new WaitForSeconds(moveDuration);

        semangCard.SetActive(false);

        // =========================
        // 【新增】进入答题阶段（原 TODO）
        // =========================

        inputField.text = "";
        inputPanel.SetActive(true);

        yield return null;
    }

    // =========================
    // 【新增】按钮调用
    // =========================
    public void SubmitAnswer()
    {
        if (!int.TryParse(inputField.text, out int input))
        {
            PuzzleFail();
            return;
        }

        if (input == currentCard.answer)
        {
            PuzzleCorrect();
        }
        else
        {
            PuzzleFail();
        }
    }

    // =========================
    // 【新增】结果处理
    // =========================
    void PuzzleCorrect()
    {
        correctCount++;
        inputPanel.SetActive(false);

        if (correctCount >= requiredCorrect)
        {
            PuzzleComplete();
        }
        else
        {
            RestartSemangReadingTiming();
        }
    }

    void PuzzleFail()
    {
        inputPanel.SetActive(false);
        correctCount = 0;
        RestartSemangReadingTiming();
    }

    void PuzzleComplete()
    {
        inputPanel.SetActive(false);

        // 等同按了一次 ESC
        FPSController player = FindObjectOfType<FPSController>();
        if (player != null)
            player.EnablePlayer();

        resultText.text = "你是正常的，体检结束";
        resultText.gameObject.SetActive(true);
    }
}
