using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Street_level_Handler : MonoBehaviour { 
 private enum LightState
{
    Red,
    Green
}

private LightState state;

[Header("Fail Settings")]
public int maxWrongSlow = 3;
private int slowCount = 0;

[Header("Audio")]
public AudioSource hornAudio;   // 喇叭
public AudioSource crashAudio;  // 事故（可选）

private bool triggered = false;

private void Start()
{
    // 每一个红绿灯，自己随机
    state = Random.value > 0.5f ? LightState.Red : LightState.Green;
}

private void OnTriggerEnter(Collider other)
{
    if (triggered) return;

    FPSController player = other.GetComponent<FPSController>();
    if (player == null) return;

    triggered = true;

    // 判断玩家此刻的“行为倾向”
    if (state == LightState.Red)
    {
        // 红灯：任何继续前进都视为闯红灯
        GameOver("事故发生");
    }
    else
    {
        // 绿灯：如果玩家减速，视为阻碍交通
        if (Input.GetKey(KeyCode.S))
        {
            slowCount++;
            hornAudio?.Play();

            if (slowCount >= maxWrongSlow)
            {
                GameOver("交通中断");
            }
        }
    }
}

void GameOver(string reason)
{
    Debug.Log(reason);

    // 这里你接自己的失败流程
    // 比如：黑屏、停止输入、加载失败场景
}
}
