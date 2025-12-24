using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public enum LightState
    {
        Red,
        Green
    }

    [Header("State")]
    public LightState currentState;

    [Header("Light Mesh")]
    public Renderer redLightRenderer;    // Cylinder1
    public Renderer greenLightRenderer;  // Cylinder3

    [Header("Materials")]
    public Material lightOnMat;
    public Material lightOffMat;

    private void Awake()
    {
        RandomizeState();
        UpdateVisual();
    }

    // =========================
    // 状态切换
    // =========================
    public void SwitchState()
    {
        currentState = currentState == LightState.Red
            ? LightState.Green
            : LightState.Red;

        UpdateVisual();
    }

    public void ResetLight()
    {
        RandomizeState();
        UpdateVisual();
    }

    private void RandomizeState()
    {
        currentState = Random.value > 0.5f
            ? LightState.Red
            : LightState.Green;
    }

    // =========================
    // 视觉更新
    // =========================
    private void UpdateVisual()
    {
        if (currentState == LightState.Red)
        {
            redLightRenderer.material = lightOnMat;
            greenLightRenderer.material = lightOffMat;
        }
        else
        {
            redLightRenderer.material = lightOffMat;
            greenLightRenderer.material = lightOnMat;
        }
    }

    // 对外接口
    public bool IsRed() => currentState == LightState.Red;
    public bool IsGreen() => currentState == LightState.Green;
}