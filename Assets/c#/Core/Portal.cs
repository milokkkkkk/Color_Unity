using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {
    [Header ("Main Settings")]
    public Portal linkedPortal;
    public MeshRenderer screen;
    public int recursionLimit = 5;

    [Header ("Advanced Settings")]
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    // Private variables
    RenderTexture viewTexture;
    Camera portalCam;
    Camera playerCam;
    Material firstRecursionMat;
    List<PortalTraveller> trackedTravellers;
    MeshFilter screenMeshFilter;

    void Awake () {
        Debug.Log($"[Portal] {gameObject.name} 初始化传送门");
        playerCam = Camera.main;
        portalCam = GetComponentInChildren<Camera> ();
        portalCam.enabled = false;
        // Ensure URP renders this camera into target texture even if not active in camera stack
        portalCam.forceIntoRenderTexture = true;
        portalCam.allowMSAA = false;
        portalCam.allowHDR = false;
        trackedTravellers = new List<PortalTraveller> ();
        screenMeshFilter = screen.GetComponent<MeshFilter> ();
        // 默认先隐藏纹理显示占位色，待纹理准备好后再开启
        screen.material.SetInt ("displayMask", 0);
        if (linkedPortal != null) {
            Debug.Log($"[Portal] {gameObject.name} 已连接到传送门: {linkedPortal.name}");
            CreateViewTexture ();
        } else {
            Debug.LogWarning($"[Portal] {gameObject.name} 没有设置 linkedPortal！");
        }
    }

    void LateUpdate () {
        HandleTravellers ();
    }

    void HandleTravellers () {
    if (trackedTravellers.Count == 0) return;

    for (int i = 0; i < trackedTravellers.Count; i++) {
        var traveller = trackedTravellers[i];
        Transform refT = traveller.crossingReference != null ? traveller.crossingReference : traveller.transform;

        // 依旧用原始 m 矩阵
        var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * traveller.transform.localToWorldMatrix;

        // —— 用 refT 计算“当前侧/之前侧” —— 
        Vector3 curOffset = refT.position - transform.position;
        int portalSide    = System.Math.Sign(Vector3.Dot(curOffset, transform.forward));
        int portalSideOld = System.Math.Sign(Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward));

        // 临时调试：看是否真的跨零
        // Debug.Log($"dot={Vector3.Dot(curOffset, transform.forward):F3}, old={Vector3.Dot(traveller.previousOffsetFromPortal, transform.forward):F3}");

        if (portalSide != portalSideOld) {
            Debug.Log($"[Portal] {name} 检测到跨面 -> 传送 {traveller.name}");
            var positionOld = traveller.transform.position;
            var rotOld      = traveller.transform.rotation;

            traveller.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);
            if (traveller.graphicsClone != null) {
                traveller.graphicsClone.transform.SetPositionAndRotation(positionOld, rotOld);
            }

            // 传送后立刻让对端开始跟踪
            linkedPortal.OnTravellerEnterPortal(traveller);
            trackedTravellers.RemoveAt(i); 
            i--;
        } else {
            // 未跨面：更新 clone 与 previousOffset（仍用 refT！）
            if (traveller.graphicsClone != null) {
                traveller.graphicsClone.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
            }
            traveller.previousOffsetFromPortal = curOffset;
        }
    }
}


    // Called before any portal cameras are rendered for the current frame
    public void PrePortalRender () {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams (traveller);
        }
    }

    // Manually render the camera attached to this portal
    // Called after PrePortalRender, and before PostPortalRender
    public void Render () {

        // 确保首先创建并绑定 RT，即使不可见也让 _MainTex 先完成赋值
        CreateViewTexture ();

        // Skip rendering the view from this portal if player is not looking at the linked portal
        if (!CameraUtility.VisibleFromCamera (linkedPortal.screen, playerCam)) {
            return;
        }

        var localToWorldMatrix = playerCam.transform.localToWorldMatrix;
        var renderPositions = new Vector3[recursionLimit];
        var renderRotations = new Quaternion[recursionLimit];

        int startIndex = 0;
        portalCam.projectionMatrix = playerCam.projectionMatrix;
        for (int i = 0; i < recursionLimit; i++) {
            if (i > 0) {
                // No need for recursive rendering if linked portal is not visible through this portal
                if (!CameraUtility.BoundsOverlap (screenMeshFilter, linkedPortal.screenMeshFilter, portalCam)) {
                    break;
                }
            }
            localToWorldMatrix = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * localToWorldMatrix;
            int renderOrderIndex = recursionLimit - i - 1;
            renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn (3);
            renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

            portalCam.transform.SetPositionAndRotation (renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
            startIndex = renderOrderIndex;
        }

        // Hide screen so that camera can see through portal
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        linkedPortal.screen.material.SetInt ("displayMask", 0);

        for (int i = startIndex; i < recursionLimit; i++) {
            portalCam.transform.SetPositionAndRotation (renderPositions[i], renderRotations[i]);
            SetNearClipPlane ();
            HandleClipping ();
            portalCam.Render ();

            if (i == startIndex) {
                linkedPortal.screen.material.SetInt ("displayMask", 1);
            }
        }

        // Unhide objects hidden at start of render
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    void HandleClipping () {
        // There are two main graphical issues when slicing travellers
        // 1. Tiny sliver of mesh drawn on backside of portal
        //    Ideally the oblique clip plane would sort this out, but even with 0 offset, tiny sliver still visible
        // 2. Tiny seam between the sliced mesh, and the rest of the model drawn onto the portal screen
        // This function tries to address these issues by modifying the slice parameters when rendering the view from the portal
        // Would be great if this could be fixed more elegantly, but this is the best I can figure out for now
        const float hideDst = -1000;
        const float showDst = 1000;
        float screenThickness = linkedPortal.ProtectScreenFromClipping (portalCam.transform.position);

        foreach (var traveller in trackedTravellers) {
            if (SameSideOfPortal (traveller.transform.position, portalCamPos)) {
                // Addresses issue 1
                traveller.SetSliceOffsetDst (hideDst, false);
            } else {
                // Addresses issue 2
                traveller.SetSliceOffsetDst (showDst, false);
            }

            // Ensure clone is properly sliced, in case it's visible through this portal:
            int cloneSideOfLinkedPortal = -SideOfPortal (traveller.transform.position);
            bool camSameSideAsClone = linkedPortal.SideOfPortal (portalCamPos) == cloneSideOfLinkedPortal;
            if (camSameSideAsClone) {
                traveller.SetSliceOffsetDst (screenThickness, true);
            } else {
                traveller.SetSliceOffsetDst (-screenThickness, true);
            }
        }

        var offsetFromPortalToCam = portalCamPos - transform.position;
		foreach (var linkedTraveller in linkedPortal.trackedTravellers) {
			if (linkedTraveller.graphicsObject == null || linkedTraveller.graphicsClone == null) {
				continue;
			}
			var travellerPos = linkedTraveller.graphicsObject.transform.position;
			var clonePos = linkedTraveller.graphicsClone.transform.position;
            // Handle clone of linked portal coming through this portal:
            bool cloneOnSameSideAsCam = linkedPortal.SideOfPortal (travellerPos) != SideOfPortal (portalCamPos);
            if (cloneOnSameSideAsCam) {
                // Addresses issue 1
                linkedTraveller.SetSliceOffsetDst (hideDst, true);
            } else {
                // Addresses issue 2
                linkedTraveller.SetSliceOffsetDst (showDst, true);
            }

            // Ensure traveller of linked portal is properly sliced, in case it's visible through this portal:
            bool camSameSideAsTraveller = linkedPortal.SameSideOfPortal (linkedTraveller.transform.position, portalCamPos);
            if (camSameSideAsTraveller) {
                linkedTraveller.SetSliceOffsetDst (screenThickness, false);
            } else {
                linkedTraveller.SetSliceOffsetDst (-screenThickness, false);
            }
        }
    }

    // Called once all portals have been rendered, but before the player camera renders
    public void PostPortalRender () {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams (traveller);
        }
        ProtectScreenFromClipping (playerCam.transform.position);
    }
    void CreateViewTexture () {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height) {
            if (viewTexture != null) {
                viewTexture.Release ();
            }
            viewTexture = new RenderTexture (Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            viewTexture.antiAliasing = 1;
            viewTexture.useMipMap = false;
            viewTexture.autoGenerateMips = false;
            viewTexture.Create ();
            // Render the view from the portal camera to the view texture
            portalCam.targetTexture = viewTexture;
            // Display the view texture on the screen of the linked portal
            linkedPortal.screen.material.SetTexture ("_MainTex", viewTexture);
            linkedPortal.screen.material.SetInt ("displayMask", 1);
        }
    }

    // Sets the thickness of the portal screen so as not to clip with camera near plane when player goes through
    float ProtectScreenFromClipping (Vector3 viewPoint) {
        float halfHeight = playerCam.nearClipPlane * Mathf.Tan (playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * playerCam.aspect;
        float dstToNearClipPlaneCorner = new Vector3 (halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot (transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3 (screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        return screenThickness;
    }

    void UpdateSliceParams (PortalTraveller traveller) {
        // Calculate slice normal
        int side = SideOfPortal (traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;

        // Calculate slice centre
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = linkedPortal.transform.position;

        // Adjust slice offset so that when player standing on other side of portal to the object, the slice doesn't clip through
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = screen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal (playerCam.transform.position, traveller.transform.position);
        if (!playerSameSideAsTraveller) {
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsCloneAppearing = side != linkedPortal.SideOfPortal (playerCam.transform.position);
        if (!playerSameSideAsCloneAppearing) {
            cloneSliceOffsetDst = -screenThickness;
        }

		// Apply parameters (在无可视化外观时跳过)
		if (traveller.originalMaterials == null || traveller.cloneMaterials == null) {
			return;
		}
		for (int i = 0; i < traveller.originalMaterials.Length; i++) {
			if (traveller.originalMaterials[i] != null) {
				traveller.originalMaterials[i].SetVector ("sliceCentre", slicePos);
				traveller.originalMaterials[i].SetVector ("sliceNormal", sliceNormal);
				traveller.originalMaterials[i].SetFloat ("sliceOffsetDst", sliceOffsetDst);
			}

			if (i < traveller.cloneMaterials.Length && traveller.cloneMaterials[i] != null) {
				traveller.cloneMaterials[i].SetVector ("sliceCentre", cloneSlicePos);
				traveller.cloneMaterials[i].SetVector ("sliceNormal", cloneSliceNormal);
				traveller.cloneMaterials[i].SetFloat ("sliceOffsetDst", cloneSliceOffsetDst);
			}

		}

    }

    // Use custom projection matrix to align portal camera's near clip plane with the surface of the portal
    // Note that this affects precision of the depth buffer, which can cause issues with effects like screenspace AO
    void SetNearClipPlane () {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        Transform clipPlane = transform;
        int dot = System.Math.Sign (Vector3.Dot (clipPlane.forward, transform.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint (clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector (clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot (camSpacePos, camSpaceNormal) + nearClipOffset;

        // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
        if (Mathf.Abs (camSpaceDst) > nearClipLimit) {
            Vector4 clipPlaneCameraSpace = new Vector4 (camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            // Update projection based on new clip plane
            // Calculate matrix with player cam so that player camera settings (fov, etc) are used
            portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix (clipPlaneCameraSpace);
        } else {
            portalCam.projectionMatrix = playerCam.projectionMatrix;
        }
    }

   void OnTravellerEnterPortal (PortalTraveller traveller) {
    if (!trackedTravellers.Contains(traveller)) {
        Debug.Log($"[Portal] {gameObject.name} 开始跟踪传送者: {traveller.name}");
        traveller.EnterPortalThreshold();

        // 关键：用 crossingReference（若为空退回根节点）
        Transform refT = traveller.crossingReference != null ? traveller.crossingReference : traveller.transform;
        traveller.previousOffsetFromPortal = refT.position - transform.position;

        trackedTravellers.Add(traveller);
        Debug.Log($"[Portal] {gameObject.name} 当前跟踪的传送者数量: {trackedTravellers.Count}");
    } else {
        Debug.Log($"[Portal] {gameObject.name} 传送者 {traveller.name} 已在跟踪列表中");
    }
}


    void OnTriggerEnter (Collider other) {
        Debug.Log($"[Portal] {gameObject.name} 检测到物体进入触发器: {other.name}");
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller) {
            Debug.Log($"[Portal] {gameObject.name} 发现 PortalTraveller: {traveller.name}");
            OnTravellerEnterPortal (traveller);
        } else {
            Debug.Log($"[Portal] {gameObject.name} 物体 {other.name} 没有 PortalTraveller 组件");
        }
    }

    void OnTriggerExit (Collider other) {
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller && trackedTravellers.Contains (traveller)) {
            traveller.ExitPortalThreshold ();
            trackedTravellers.Remove (traveller);
        }
    }

    /*
     ** Some helper/convenience stuff:
     */

    int SideOfPortal (Vector3 pos) {
        return System.Math.Sign (Vector3.Dot (pos - transform.position, transform.forward));
    }

    bool SameSideOfPortal (Vector3 posA, Vector3 posB) {
        return SideOfPortal (posA) == SideOfPortal (posB);
    }

    Vector3 portalCamPos {
        get {
            return portalCam.transform.position;
        }
    }

    void OnValidate () {
        if (linkedPortal != null) {
            linkedPortal.linkedPortal = this;
        }
    }
}