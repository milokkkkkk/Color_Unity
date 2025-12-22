using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLIght : MonoBehaviour
{
    public enum LightState
    {
        Red,
        Green
    }

    [Header("Runtime State")]
    public LightState currentState;

    private void Awake()
    {
        // 每个红绿灯在生成 / 关卡开始时
        // 各自随机一次状态
        currentState = Random.value > 0.5f
            ? LightState.Red
            : LightState.Green;
    }

    /// <summary>
    /// 对外只读接口
    /// </summary>
    public bool IsRed()
    {
        return currentState == LightState.Red;
    }

    public bool IsGreen()
    {
        return currentState == LightState.Green;
    }
}

