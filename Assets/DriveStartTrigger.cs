using UnityEngine;

public class DriveStartTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FPSController player = other.GetComponent<FPSController>();
        if (player != null)
        {
            // 把“正前方”交给玩家去对齐
            player.EnterDriveMode(transform);
        }
    }
}