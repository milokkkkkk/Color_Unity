using UnityEngine;

public class RoadLightGenerator : MonoBehaviour
{
    [Header("Reference")]
    public Transform ground;              // Ground 或 Cube(13)
    public GameObject roadLightPrefab;

    [Header("Spacing")]
    public float minSpacing = 20f;
    public float maxSpacing = 30f;

    [Header("Placement")]
    public float sideOffset = 2f;          // 向路边偏移
    public float heightOffset = 0f;        // 高度微调

    [Header("Road Length")]
    public float roadLength = 300f;        // 路的总长度（你自己填）

    private void Start()
    {
        GenerateLights();
    }

    void GenerateLights()
    {
        Collider groundCol = ground.GetComponent<Collider>();
        Bounds bounds = groundCol.bounds;

        // 假设道路沿 Z 轴（如果不是，告诉我，我给你改）
        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        // 道路起点 = 最小 Z 边
        Vector3 startPos = new Vector3(
            bounds.center.x,
            bounds.max.y,      // 地表高度
            bounds.min.z
        );

        float currentDistance = 0f;

        while (currentDistance < roadLength)
        {
            float spacing = Random.Range(minSpacing, maxSpacing);
            currentDistance += spacing;

            if (currentDistance >= roadLength)
                break;

            Vector3 pos =
                startPos
                + forward * currentDistance
                + right * sideOffset;

            pos.y = bounds.max.y + heightOffset;

            Quaternion rot = Quaternion.LookRotation(forward);

            Instantiate(roadLightPrefab, pos, rot);
        }
    }

}
