using UnityEngine;

public class SpeedDetect : MonoBehaviour
{
    [Header("Reference")]
    public TrafficLight trafficLight;   // 父物体上的红绿灯组件

    [Header("Green Light Rule")]
    public float minGreenSpeed = 1.5f;
    public int maxSlowCount = 3;

    private int slowCount = 0;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        FPSController player = other.GetComponent<FPSController>();

        float speed = player.driveSpeed;
        string lightState = trafficLight.IsRed() ? "RED" : "GREEN";

        Debug.Log(
            $"[SpeedDetect] 触发检测 | 灯色: {lightState} | 当前速度: {speed:F2}"
        );

        if (trafficLight.IsRed())
        {
            if (speed > 0f)
            {
                Debug.Log("[SpeedDetect][违规] 红灯仍在移动  GameOver");
                player.GameOver();
            }
            else
            {
                Debug.Log("[SpeedDetect][通过] 红灯已停下");
            }
            return;
        }

        if (trafficLight.IsGreen())
        {
            if (speed < minGreenSpeed)
            {
                slowCount++;

                Debug.Log(

                    $"[SpeedDetect][过慢] slowCount={slowCount}/{maxSlowCount}"
                );

                if (slowCount >= maxSlowCount)
                {
                    Debug.Log("[SpeedDetect][GameOver] 绿灯过慢次数达上限");
                    player.GameOver();
                }
            }
            else
            {
                Debug.Log("[SpeedDetect][通过] 绿灯速度正常");
            }
        }
    }

    // ======================
    // 给 GameOver 用的 Reset
    // ======================
    public void ResetDetect()
    {
        slowCount = 0;
        hasTriggered = false;
    }
}
