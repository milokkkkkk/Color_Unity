using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CarLevelUIController : MonoBehaviour
{
    [Header("Speed UI")]
    public TMP_Text speedText;  // 左上角速度显示


    [Header("Progress / HP UI")]
    public Slider progressSlider; // 右上角进度条 Fill Slider


    [Header("Config")]
    public float maxProgress = 3f; // 3次失败 GameOver


    private float currentProgress = 0f;


    void Start()
    {
        progressSlider.maxValue = maxProgress; // 3
        progressSlider.value = 0;
    }

    public void AddSlowPenalty()
    {
        currentProgress += 1f;
        progressSlider.value = currentProgress;

        if (currentProgress >= maxProgress)
        {
            TriggerGameOver("Too Slow At Green Light");
        }
    }


        public void UpdateSpeed(float speed)
    {
        Debug.Log("UpdateSpeed called: " + speed);
        if (speedText != null)
        {
            speedText.text = $"Speed: {Mathf.RoundToInt(speed)}";
        }
    }


    // 每次绿灯速度过慢时调用


    void UpdateProgress()
    {
        if (progressSlider != null)
        {
            progressSlider.value = currentProgress;
        }
    }

    void TriggerGameOver(string reason)
    {
        GameOverUI.Show(reason);
    }
}
