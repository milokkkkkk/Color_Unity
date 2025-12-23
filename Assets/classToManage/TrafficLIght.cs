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
        RandomizeState();
    }


    public void ResetLight()
    {
        RandomizeState();
    }

    // =========================
    // 内部统一随机函数
    // =========================
    private void RandomizeState()
    {
        currentState = Random.value > 0.5f
            ? LightState.Red
            : LightState.Green;
    }

    // 对外只读接口（你原来就有）
    public bool IsRed()
    {
        return currentState == LightState.Red;
    }

    public bool IsGreen()
    {
        return currentState == LightState.Green;
    }
}