using System.Collections.Generic;
using UnityEngine;

public class PortalTraveller : MonoBehaviour {
    public Transform crossingReference; 

    public GameObject graphicsObject;
    public GameObject graphicsClone { get; set; }
    public Vector3 previousOffsetFromPortal { get; set; }

    public Material[] originalMaterials { get; set; }
    public Material[] cloneMaterials { get; set; }

    public virtual void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot) {
        Debug.Log($"[PortalTraveller] {gameObject.name} 开始传送: 从 {fromPortal.name} 到 {toPortal.name}");
        Debug.Log($"[PortalTraveller] 传送前位置: {transform.position}, 传送后位置: {pos}");
        transform.position = pos;
        transform.rotation = rot;
        Debug.Log($"[PortalTraveller] {gameObject.name} 传送完成");
    }

    // Called when first touches portal
    public virtual void EnterPortalThreshold () {
        Debug.Log($"[PortalTraveller] {gameObject.name} 进入传送门阈值");
        if (graphicsObject == null) {
            // 无可视化外观：跳过克隆与材质收集
            Debug.Log($"[PortalTraveller] {gameObject.name} graphicsObject 为空，跳过可视化处理");
            return;
        }

        if (graphicsClone == null) {
            graphicsClone = Instantiate (graphicsObject);
            graphicsClone.transform.parent = graphicsObject.transform.parent;
            graphicsClone.transform.localScale = graphicsObject.transform.localScale;
            originalMaterials = GetMaterials (graphicsObject);
            cloneMaterials = GetMaterials (graphicsClone);
        } else {
            graphicsClone.SetActive (true);
        }
    }

    // Called once no longer touching portal (excluding when teleporting)
    public virtual void ExitPortalThreshold () {
        Debug.Log($"[PortalTraveller] {gameObject.name} 退出传送门阈值");
        if (graphicsClone != null) {
            graphicsClone.SetActive (false);
        }
        // Disable slicing
        if (originalMaterials != null) {
            for (int i = 0; i < originalMaterials.Length; i++) {
                originalMaterials[i].SetVector ("sliceNormal", Vector3.zero);
            }
        }
    }

    public void SetSliceOffsetDst (float dst, bool clone) {
        if (originalMaterials == null) {
            return;
        }
        for (int i = 0; i < originalMaterials.Length; i++) {
            if (clone) {
                if (cloneMaterials != null && i < cloneMaterials.Length && cloneMaterials[i] != null) {
                    cloneMaterials[i].SetFloat ("sliceOffsetDst", dst);
                }
            } else {
                if (originalMaterials[i] != null) {
                    originalMaterials[i].SetFloat ("sliceOffsetDst", dst);
                }
            }
        }
    }

    Material[] GetMaterials (GameObject g) {
        if (g == null) {
            return new Material[0];
        }
        var renderers = g.GetComponentsInChildren<MeshRenderer> ();
        var matList = new List<Material> ();
        foreach (var renderer in renderers) {
            foreach (var mat in renderer.materials) {
                matList.Add (mat);
            }
        }
        return matList.ToArray ();
    }
}