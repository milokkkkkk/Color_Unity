using UnityEngine;
using System.Collections;

public class TrafficLightTimerTrigger : MonoBehaviour
{
    [Header("Reference")]
    public TrafficLight trafficLight;

    [Header("Interval (Random)")]
    public float minInterval = 4f;
    public float maxInterval = 8f;

    [Header("Warning")]
    public float warningTime = 1f;           // 切换前多久提示
    public AudioSource audioSource;
    public AudioClip warningClip;

    private Coroutine timerCo;

    private void OnTriggerEnter(Collider other)
    {
        FPSController player = other.GetComponent<FPSController>();
        if (player == null)
            return;

        if (timerCo == null)
        {
            timerCo = StartCoroutine(TimerIE());
        }
    }

    IEnumerator TimerIE()
    {
        while (true)
        {
            float interval = Random.Range(minInterval, maxInterval);

            // 防止 interval 小于 warningTime
            float mainWait = Mathf.Max(0f, interval - warningTime);

            Debug.Log(
                $"[TrafficLightTimer] 本轮切换时间 = {interval:F2}s，" +
                $"提示音提前 {warningTime}s"
            );

            // 第一段等待
            yield return new WaitForSeconds(mainWait);

            // 提示音
            if (audioSource != null && warningClip != null)
            {
                audioSource.PlayOneShot(warningClip);
            }

            Debug.Log("[TrafficLightTimer] 提示音播放");

            // 最后一秒
            yield return new WaitForSeconds(warningTime);

            // 真正切换灯
            trafficLight.SwitchState();
        }
    }

    // GameOver 或关卡重置时调用
    public void ResetTimer()
    {
        if (timerCo != null)
        {
            StopCoroutine(timerCo);
            timerCo = null;
        }
    }
}