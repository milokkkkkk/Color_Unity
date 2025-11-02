using System.Collections;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    public Camera playerCamera;
    private Portal[] portals;
    private bool _renderScheduled;

    void Awake()
    {
        portals = FindObjectsOfType<Portal>();

        if (playerCamera == null)
            playerCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // 每帧只排一次 render，防止多个 LateUpdate 重复排
        if (_renderScheduled) return;
        _renderScheduled = true;
        StartCoroutine(RenderPortalsAtEndOfFrame());
    }

    private IEnumerator RenderPortalsAtEndOfFrame()
    {
        // 等这一帧真正结束（SRP 也跑完）再渲
        yield return new WaitForEndOfFrame();

        // 1. 更新切割参数
        for (int i = 0; i < portals.Length; i++)
            portals[i].PrePortalRender();

        // 2. 真正去渲 portal 相机
        for (int i = 0; i < portals.Length; i++)
            portals[i].Render();

        // 3. 收尾
        for (int i = 0; i < portals.Length; i++)
            portals[i].PostPortalRender();

        _renderScheduled = false;
    }
}
